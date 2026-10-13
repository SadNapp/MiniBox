using MiniBox.Application.Common.Interfaces;
using MiniBox.Infrastructure.Authentication;
using MiniBox.Infrastructure.Caching;
using MiniBox.Infrastructure.Persistence;
using MiniBox.Infrastructure.Persistence.Repositories;
using MiniBox.Infrastructure.Services;
using MiniBox.Application.Features.Simulation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace MiniBox.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, string redisConnection, string jwtSigningKey)
    {
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString, npgsql =>
            npgsql.ConfigureDataSource(dataSource => dataSource.EnableDynamicJson())));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton<PasswordHasher>();
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton(_ => new JwtTokenGenerator(jwtSigningKey));
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
        services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(connectionString));
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IProductStockService, ProductStockService>();
        services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
        services.AddSingleton<IOnlineUserCounter, RedisOnlineUserCounter>();
        services.AddScoped<IProductCatalog, ProductCatalog>();
        services.AddScoped<ProductReadRepository>();
        services.AddSingleton<IRedisCacheService, RedisCacheService>();
        services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();
        services.AddHealthChecks().AddCheck<RedisHealthCheck>("redis");
        return services;
    }
}

public sealed class RedisOnlineUserCounter(IConnectionMultiplexer redis) : IOnlineUserCounter
{
    public async Task<long> GetCountAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await redis.GetDatabase().SetLengthAsync("presence:connections");
    }
}

public sealed class RedisHealthCheck(IConnectionMultiplexer redis) : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
{
    public async Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try { await redis.GetDatabase().PingAsync(); return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(); }
        catch (Exception exception) { return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Redis unavailable", exception); }
    }
}
