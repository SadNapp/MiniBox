using Microsoft.EntityFrameworkCore;
using MiniBox.Application.Common.Interfaces;
using MiniBox.Infrastructure.Persistence;

namespace MiniBox.IntegrationTests.Infrastructure;

public sealed class DatabaseFixture : IDisposable
{
    private readonly string _databaseName = $"minibox-tests-{Guid.NewGuid():N}";
    private readonly TestCurrentUser _currentUser = new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
    private readonly TestClock _clock = new(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc));

    public ApplicationDbContext CreateContext() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(_databaseName).Options,
        _currentUser, _clock);

    public void Dispose() { }

    private sealed record TestCurrentUser(Guid? UserId) : ICurrentUserService;
    private sealed record TestClock(DateTime UtcNow) : IDateTimeService;
}
