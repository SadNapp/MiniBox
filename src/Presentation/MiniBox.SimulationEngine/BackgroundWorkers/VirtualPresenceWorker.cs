using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using MiniBox.SimulationEngine.Models;

namespace MiniBox.SimulationEngine.BackgroundWorkers;

public sealed class VirtualPresenceWorker(
    IOptions<SimulationSettings> options,
    ILogger<VirtualPresenceWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        var target = Math.Clamp(settings.VirtualConnectionCount, 0, 5000);
        if (target == 0)
        {
            logger.LogInformation("Virtual SignalR presence is disabled.");
            return;
        }

        logger.LogInformation("Opening {ConnectionCount} virtual SignalR presence connections.", target);
        var clients = Enumerable.Range(0, target).Select(index => RunClientAsync(index, settings.TargetApiUrl, stoppingToken));
        await Task.WhenAll(clients);
    }

    private async Task RunClientAsync(int clientNumber, string apiUrl, CancellationToken stoppingToken)
    {
        var random = new Random(HashCode.Combine(clientNumber, Environment.TickCount));
        while (!stoppingToken.IsCancellationRequested)
        {
            HubConnection? connection = null;
            try
            {
                connection = new HubConnectionBuilder()
                    .WithUrl($"{apiUrl.TrimEnd('/')}/hubs/presence")
                    .WithAutomaticReconnect([TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(8), TimeSpan.FromSeconds(15)])
                    .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
                    .Build();

                await connection.StartAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(random.Next(180, 361)), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                if (clientNumber == 0 || clientNumber % 100 == 0)
                    logger.LogWarning(exception, "Virtual SignalR client {ClientNumber} could not connect; retrying.", clientNumber);
            }
            finally
            {
                if (connection is not null)
                {
                    try { await connection.StopAsync(CancellationToken.None); }
                    catch (Exception exception) { logger.LogDebug(exception, "Virtual SignalR client {ClientNumber} was already disconnected.", clientNumber); }
                    await connection.DisposeAsync();
                }
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                try { await Task.Delay(TimeSpan.FromSeconds(random.Next(2, 6)), stoppingToken); }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            }
        }
    }
}
