using MiniBox.Application.Common.Interfaces;
using StackExchange.Redis;

namespace MiniBox.Infrastructure.Caching;

public sealed class RedisDistributedLockService(IConnectionMultiplexer redis) : IDistributedLockService
{
    public async Task<IAsyncDisposable?> AcquireAsync(string resource, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = $"lock:{resource}";
        var token = Guid.NewGuid().ToString("N");
        var database = redis.GetDatabase();
        var acquired = await database.StringSetAsync(key, token, expiry, When.NotExists).ConfigureAwait(false);
        return acquired ? new RedisLock(database, key, token, resource) : null;
    }

    public async Task RenewAsync(IAsyncDisposable handle, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (handle is not RedisLock redisLock) throw new ArgumentException("Lock handle was not created by this service.", nameof(handle));
        await redisLock.RenewAsync(expiry);
    }

    private sealed class RedisLock(IDatabase database, string key, string token, string resource) : IAsyncDisposable
    {
        private const string ReleaseScript = "if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end";
        private const string RenewScript = "if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('pexpire', KEYS[1], ARGV[2]) else return 0 end";
        public async Task RenewAsync(TimeSpan expiry)
        {
            var renewed = await database.ScriptEvaluateAsync(RenewScript, [key], [token, (long)expiry.TotalMilliseconds]).ConfigureAwait(false);
            if ((long)renewed == 0) throw new InvalidOperationException($"Distributed lock was lost: {resource}.");
        }
        public async ValueTask DisposeAsync()
        {
            await database.ScriptEvaluateAsync(ReleaseScript, [key], [token]).ConfigureAwait(false);
        }
    }
}
