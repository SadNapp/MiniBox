namespace MiniBox.SimulationEngine.Models;

public sealed class SimulationSettings
{
    public string TargetApiUrl { get; set; } = "http://localhost:8081";
    public int UserCount { get; set; } = 100;
    public int VirtualConnectionCount { get; set; } = 1200;
    public int RequestsPerSecond { get; set; } = 10;
    public int SeedBatchSize { get; set; } = 500;
    public int SeedProductCount { get; set; }
    public int SeedUserCount { get; set; }
    public string VirtualUserPassword { get; set; } = string.Empty;
}
