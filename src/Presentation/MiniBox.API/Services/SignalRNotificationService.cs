using Microsoft.AspNetCore.SignalR;
using MiniBox.Application.Common.Interfaces;
using MiniBox.API.Hubs;

namespace MiniBox.API.Services;

public sealed class SignalRNotificationService(IHubContext<StoreEventsHub> storeHub, IHubContext<PresenceHub> presenceHub) : ISignalRNotificationService
{
    private long _latestOnlineCount;
    private long _onlineCountVersion;
    private int _onlineBroadcastRunning;

    public Task NotifyOrderCreatedAsync(Guid orderId, string productName, CancellationToken cancellationToken = default) =>
        storeHub.Clients.All.SendAsync("OrderCreated", new { orderId, productName }, cancellationToken);
    public Task NotifyStockChangedAsync(Guid productId, int remaining, CancellationToken cancellationToken = default) =>
        storeHub.Clients.All.SendAsync("StockChanged", new { productId, remaining }, cancellationToken);
    public Task NotifyOnlineCountAsync(long count, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Exchange(ref _latestOnlineCount, count);
        Interlocked.Increment(ref _onlineCountVersion);
        return Interlocked.CompareExchange(ref _onlineBroadcastRunning, 1, 0) == 0
            ? BroadcastLatestOnlineCountAsync()
            : Task.CompletedTask;
    }

    private async Task BroadcastLatestOnlineCountAsync()
    {
        try
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250));
                var version = Interlocked.Read(ref _onlineCountVersion);
                var count = Interlocked.Read(ref _latestOnlineCount);
                await presenceHub.Clients.All.SendAsync("OnlineCountChanged", count);

                Interlocked.Exchange(ref _onlineBroadcastRunning, 0);
                if (version == Interlocked.Read(ref _onlineCountVersion)
                    || Interlocked.CompareExchange(ref _onlineBroadcastRunning, 1, 0) != 0)
                    return;
            }
        }
        catch
        {
            Interlocked.Exchange(ref _onlineBroadcastRunning, 0);
            throw;
        }
    }
}
