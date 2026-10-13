namespace MiniBox.Application.Common.Interfaces;

public interface IDistributedLockService
{
    Task<IAsyncDisposable?> AcquireAsync(string resource, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task RenewAsync(IAsyncDisposable handle, TimeSpan expiry, CancellationToken cancellationToken = default);
}
