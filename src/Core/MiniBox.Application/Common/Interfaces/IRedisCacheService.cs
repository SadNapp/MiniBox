namespace MiniBox.Application.Common.Interfaces;

public interface IRedisCacheService
{
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, string value, TimeSpan lifetime, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<long> AddPresenceAsync(string connectionId, CancellationToken cancellationToken = default);
    Task<long> RemovePresenceAsync(string connectionId, CancellationToken cancellationToken = default);
    Task<long> IncrementAsync(string key, CancellationToken cancellationToken = default);
}
