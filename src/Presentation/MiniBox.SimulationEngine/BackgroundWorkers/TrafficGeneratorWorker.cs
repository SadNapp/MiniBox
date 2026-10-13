using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MiniBox.SimulationEngine.Models;
using MiniBox.SimulationEngine.Services;

namespace MiniBox.SimulationEngine.BackgroundWorkers;

public sealed class TrafficGeneratorWorker(
    SimulatedApiClient api,
    UserBehaviorSimulator behavior,
    LiveCheckoutSimulator checkout,
    IServiceScopeFactory scopeFactory,
    IOptions<SimulationSettings> options,
    ILogger<TrafficGeneratorWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        await using (var scope = scopeFactory.CreateAsyncScope())
            await scope.ServiceProvider.GetRequiredService<DataSeederService>().SeedAsync(stoppingToken);
        await api.AuthenticateAsync(stoppingToken);
        logger.LogInformation("Traffic simulation started with {UserCount} virtual users at {RequestsPerSecond} requests per second.", settings.UserCount, settings.RequestsPerSecond);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        IReadOnlyList<MiniBox.SimulationEngine.Models.SimulatedProduct> catalog = [];
        var catalogRefresh = DateTimeOffset.MinValue;
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var catalogRefreshed = false;
                if (DateTimeOffset.UtcNow >= catalogRefresh)
                {
                    catalog = await api.GetProductsAsync(stoppingToken);
                    catalogRefresh = DateTimeOffset.UtcNow.AddSeconds(30);
                    catalogRefreshed = true;
                }
                if (catalog.Count == 0) continue;
                var requests = Math.Clamp(settings.RequestsPerSecond, 1, 100);
                var actionCount = Math.Max(0, requests - (catalogRefreshed ? 1 : 0));
                var tasks = Enumerable.Range(0, actionCount).Select(async _ =>
                {
                    if (Random.Shared.NextDouble() < 0.08)
                        await checkout.TryCheckoutAsync(catalog, stoppingToken);
                    else
                        await behavior.BrowseAsync(catalog, stoppingToken);
                });
                await Task.WhenAll(tasks);
            }
            catch (HttpRequestException exception) { logger.LogWarning(exception, "Simulation could not reach API; will retry next interval."); }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested) { logger.LogWarning("A simulation request timed out; will retry next interval."); }
        }
    }
}
