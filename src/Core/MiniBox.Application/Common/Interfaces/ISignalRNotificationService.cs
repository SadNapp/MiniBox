namespace MiniBox.Application.Common.Interfaces;

public interface ISignalRNotificationService
{
    Task NotifyOrderCreatedAsync(Guid orderId, string productName, CancellationToken cancellationToken = default);
    Task NotifyStockChangedAsync(Guid productId, int remaining, CancellationToken cancellationToken = default);
    Task NotifyOnlineCountAsync(long count, CancellationToken cancellationToken = default);
}
