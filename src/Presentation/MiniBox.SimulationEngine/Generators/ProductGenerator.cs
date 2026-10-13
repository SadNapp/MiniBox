using MiniBox.Domain.Entities.Products;

namespace MiniBox.SimulationEngine.Generators;

public sealed class ProductGenerator
{
    private static readonly string[] Platforms = ["Raspberry Pi", "Orange Pi", "NVIDIA Jetson", "Intel NUC", "ASUS PN", "Minisforum"];
    private static readonly string[] Components = ["5 8GB Board", "NVMe 1TB SSD", "DDR5 32GB SODIMM", "Aluminum Case", "Active Cooler", "Power Adapter 65W", "Wi-Fi 6 Module"];

    public Product Generate(int index)
    {
        var platform = Platforms[Random.Shared.Next(Platforms.Length)];
        var component = Components[Random.Shared.Next(Components.Length)];
        return new Product
        {
            Sku = $"MBX-{index + 1:D6}",
            ProductName = $"{platform} {component} #{index + 1:D5}",
            Description = $"Generated MiniBox hardware component {index + 1}.",
            Price = decimal.Round((decimal)Random.Shared.NextDouble() * 480 + 20, 2),
            ProductAttributes =
            [
                new ProductAttribute { Key = "platform", Value = platform },
                new ProductAttribute { Key = "formFactor", Value = "mini" },
                new ProductAttribute { Key = "tdp", Value = $"{Random.Shared.Next(5, 100)}W" }
            ]
        };
    }
}
