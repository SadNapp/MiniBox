using MiniBox.Application.Common.Interfaces;
using StackExchange.Redis;

namespace MiniBox.Infrastructure.Caching;

public sealed class RedisCacheService(IConnectionMultiplexer redis) : IRedisCacheService
{
    private readonly IDatabase _database = redis.GetDatabase();
    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default) => await _database.StringGetAsync(key).ConfigureAwait(false);
    public Task SetAsync(string key, string value, TimeSpan lifetime, CancellationToken cancellationToken = default) => _database.StringSetAsync(key, value, lifetime);
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => _database.KeyDeleteAsync(key);

    public async Task<long> IncrementAsync(string key, CancellationToken cancellationToken = default) =>
        await _database.StringIncrementAsync(key).ConfigureAwait(false);

    public async Task<long> AddPresenceAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _database.SetAddAsync("presence:connections", connectionId).ConfigureAwait(false);
        return await _database.SetLengthAsync("presence:connections").ConfigureAwait(false);
    }

    public async Task<long> RemovePresenceAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _database.SetRemoveAsync("presence:connections", connectionId).ConfigureAwait(false);
        return await _database.SetLengthAsync("presence:connections").ConfigureAwait(false);
    }
}
