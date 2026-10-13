using MiniBox.SimulationEngine.Models;
using MiniBox.SimulationEngine.Services;

namespace MiniBox.SimulationEngine.BackgroundWorkers;

public sealed class UserBehaviorSimulator(SimulatedApiClient api)
{
    public Task BrowseAsync(IReadOnlyList<SimulatedProduct> catalog, CancellationToken cancellationToken)
    {
        if (catalog.Count == 0) return Task.CompletedTask;
        var product = catalog[Random.Shared.Next(catalog.Count)];
        return api.GetProductAsync(product.Id, cancellationToken);
    }
}
