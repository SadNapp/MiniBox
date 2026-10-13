using Microsoft.EntityFrameworkCore;
using MiniBox.Application.Common.Interfaces;
using MiniBox.Domain.Entities.Orders;
using MiniBox.Infrastructure.Persistence;
using MiniBox.Domain.Events;

namespace MiniBox.Infrastructure.Services;

public sealed class OrderService(ApplicationDbContext db, ISignalRNotificationService notifications, IProductCatalog catalog,
    IDistributedLockService locks, IDomainEventDispatcher domainEvents) : IOrderService
{
    public async Task<IReadOnlyList<OrderSummaryDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.Orders.AsNoTracking().Where(order => order.UserId == userId).OrderByDescending(order => order.CreatedAt)
            .Select(order => new OrderSummaryDto(order.Id, order.OrderNumber, order.CreatedAt, order.Status.ToString(),
                order.Items.Sum(item => item.UnitPrice * item.Quantity),
                order.Items.Select(item => new OrderItemDto(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice)).ToList(),
                order.DeliveryStreet, order.DeliveryCity, order.DeliveryPostalCode, order.DeliveryCountry, order.DeliveryNotes))
            .ToListAsync(cancellationToken);

    public async Task<CreateOrderResult> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var grouped = command.Items.GroupBy(item => item.ProductId)
            .Select(group => new CreateOrderLine(group.Key, group.Sum(item => item.Quantity))).ToArray();
        var lockHandles = new List<IAsyncDisposable>();
        using var lockRenewalCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task? lockRenewal = null;
        try
        {
            foreach (var item in grouped.OrderBy(item => item.ProductId))
            {
                var handle = await locks.AcquireAsync($"stock:{item.ProductId}", TimeSpan.FromSeconds(15), cancellationToken);
                if (handle is null) throw new InvalidOperationException("Товар зараз оформлює інший покупець. Спробуйте ще раз.");
                lockHandles.Add(handle);
            }
            lockRenewal = RenewLocksAsync(lockHandles.ToArray(), lockRenewalCancellation.Token);

            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            var productIds = grouped.Select(item => item.ProductId).ToArray();
            var products = await db.Products.Where(product => productIds.Contains(product.Id)).ToDictionaryAsync(product => product.Id, cancellationToken);
            if (products.Count != productIds.Length) throw new KeyNotFoundException("Один або кілька товарів не знайдено.");
            var stocks = await db.ProductStocks.Where(stock => productIds.Contains(stock.ProductId)).ToDictionaryAsync(stock => stock.ProductId, cancellationToken);
            if (stocks.Count != productIds.Length) throw new InvalidOperationException("Для одного або кількох товарів не налаштовано склад.");

            var order = new MiniBox.Domain.Entities.Orders.Order
            {
                OrderNumber = $"MB-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..40],
                CustomerName = command.CustomerName, CustomerEmail = command.Email, UserId = command.UserId ?? Guid.Empty,
                DeliveryStreet = command.DeliveryStreet, DeliveryCity = command.DeliveryCity, DeliveryPostalCode = command.DeliveryPostalCode,
                DeliveryCountry = command.DeliveryCountry, DeliveryNotes = command.DeliveryNotes
            };
            var stockUpdates = new List<(Guid Id, int Remaining)>();
            var stockLowEvents = new List<MiniBox.Domain.Common.IDomainEvent>();
            foreach (var line in grouped)
            {
                if (products[line.ProductId].Price < 0 || products[line.ProductId].Discount is < 0 or > 100)
                    throw new InvalidOperationException($"Некоректна ціна товару: {products[line.ProductId].ProductName}.");
                var changed = await db.ProductStocks.Where(stock => stock.ProductId == line.ProductId && stock.QuantityAvailable >= line.Quantity)
                    .ExecuteUpdateAsync(update => update.SetProperty(stock => stock.QuantityAvailable, stock => stock.QuantityAvailable - line.Quantity), cancellationToken);
                if (changed != 1) throw new InvalidOperationException($"Недостатньо товару на складі: {products[line.ProductId].ProductName}.");
                var remaining = stocks[line.ProductId].QuantityAvailable - line.Quantity;
                stockUpdates.Add((line.ProductId, remaining));
                if (remaining <= 5)
                    stockLowEvents.Add(new StockLowDomainEvent(line.ProductId, remaining, DateTime.UtcNow));
                order.Items.Add(new OrderItem { ProductId = line.ProductId, ProductName = products[line.ProductId].ProductName,
                    UnitPrice = products[line.ProductId].Price * (1m - products[line.ProductId].Discount / 100m), Quantity = line.Quantity });
            }
            db.Orders.Add(order);
            order.AddDomainEvent(new OrderCreatedDomainEvent(order.Id, command.UserId ?? Guid.Empty,
                order.Items.Sum(item => item.UnitPrice * item.Quantity),
                string.Join(", ", order.Items.Select(item => item.ProductName)), DateTime.UtcNow));
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            await domainEvents.DispatchAsync(order.DomainEvents.Concat(stockLowEvents), cancellationToken);
            order.ClearDomainEvents();
            await catalog.InvalidateCatalogAsync(cancellationToken);
            foreach (var update in stockUpdates)
                await notifications.NotifyStockChangedAsync(update.Id, update.Remaining, cancellationToken);
            return new CreateOrderResult(order.Id, order.OrderNumber, order.TotalPrice);
        }
        finally
        {
            lockRenewalCancellation.Cancel();
            if (lockRenewal is not null)
            {
                try { await lockRenewal; }
                catch (OperationCanceledException) when (lockRenewalCancellation.IsCancellationRequested) { }
            }
            foreach (var handle in lockHandles.AsEnumerable().Reverse()) await handle.DisposeAsync();
        }
    }

    private async Task RenewLocksAsync(IReadOnlyCollection<IAsyncDisposable> handles, CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (await timer.WaitForNextTickAsync(cancellationToken))
            foreach (var handle in handles)
                await locks.RenewAsync(handle, TimeSpan.FromSeconds(15), cancellationToken);
    }
}


