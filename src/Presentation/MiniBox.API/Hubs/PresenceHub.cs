using Microsoft.AspNetCore.SignalR;
using MiniBox.Application.Common.Interfaces;

namespace MiniBox.API.Hubs;

public sealed class PresenceHub(IRedisCacheService cache, ISignalRNotificationService notifications) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var count = await cache.AddPresenceAsync(Context.ConnectionId, Context.ConnectionAborted);
        await notifications.NotifyOnlineCountAsync(count, Context.ConnectionAborted);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var count = await cache.RemovePresenceAsync(Context.ConnectionId);
        await notifications.NotifyOnlineCountAsync(count);
        await base.OnDisconnectedAsync(exception);
    }
}
