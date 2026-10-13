using MiniBox.SimulationEngine.Models;
using MiniBox.SimulationEngine.Services;

namespace MiniBox.SimulationEngine.BackgroundWorkers;

public sealed class LiveCheckoutSimulator(SimulatedApiClient api)
{
    public Task<bool> TryCheckoutAsync(IReadOnlyList<SimulatedProduct> catalog, CancellationToken cancellationToken)
    {
        var available = catalog.Where(product => product.Stock > 0).ToArray();
        if (available.Length == 0) return Task.FromResult(false);
        var product = available[Random.Shared.Next(available.Length)];
        return api.CreateOrderAsync(product, cancellationToken);
    }
}
