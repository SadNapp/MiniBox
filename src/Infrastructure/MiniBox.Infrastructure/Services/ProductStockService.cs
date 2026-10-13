using Microsoft.EntityFrameworkCore;
using MiniBox.Application.Common.Interfaces;
using MiniBox.Infrastructure.Persistence;

namespace MiniBox.Infrastructure.Services;

public sealed class ProductStockService(ApplicationDbContext db, IProductCatalog catalog, ISignalRNotificationService notifications,
    IDomainEventDispatcher domainEvents, IDistributedLockService locks) : IProductStockService
{
    public async Task<bool> SetAvailableAsync(Guid productId, int quantity, CancellationToken cancellationToken)
    {
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Залишок не може бути від’ємним.");
        await using var handle = await locks.AcquireAsync($"stock:{productId}", TimeSpan.FromSeconds(15), cancellationToken);
        if (handle is null) throw new InvalidOperationException("Залишок товару зараз змінюється. Спробуйте ще раз.");
        var updated = await db.ProductStocks.Where(stock => stock.ProductId == productId)
            .ExecuteUpdateAsync(update => update.SetProperty(stock => stock.QuantityAvailable, quantity), cancellationToken);
        if (updated == 0) return false;
        if (quantity <= 5)
        {
            var stock = await db.ProductStocks.SingleAsync(item => item.ProductId == productId, cancellationToken);
            stock.AddDomainEvent(new MiniBox.Domain.Events.StockLowDomainEvent(productId, quantity, DateTime.UtcNow));
            await domainEvents.DispatchAsync(stock.DomainEvents, cancellationToken);
            stock.ClearDomainEvents();
        }
        await catalog.InvalidateCatalogAsync(cancellationToken);
        await notifications.NotifyStockChangedAsync(productId, quantity, cancellationToken);
        return true;
    }
}
