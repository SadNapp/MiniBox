using Microsoft.EntityFrameworkCore;
using MiniBox.Domain.Entities.Products;
using MiniBox.Domain.Entities.Users;
using MiniBox.Infrastructure.Authentication;
using MiniBox.Infrastructure.Persistence;
using MiniBox.SimulationEngine.Generators;
using MiniBox.SimulationEngine.Models;
using Microsoft.Extensions.Options;

namespace MiniBox.SimulationEngine.Services;

public sealed class DataSeederService(
    ApplicationDbContext db,
    ProductGenerator products,
    PasswordHasher passwords,
    IOptions<SimulationSettings> settings,
    ILogger<DataSeederService> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var configuration = settings.Value;
        var batchSize = Math.Clamp(configuration.SeedBatchSize, 100, 2000);
        var existingProducts = await db.Products.CountAsync(cancellationToken);
        if (existingProducts < configuration.SeedProductCount)
        {
            var categoryIds = await db.Categories.Select(category => category.Id).ToArrayAsync(cancellationToken);
            for (var offset = existingProducts; offset < configuration.SeedProductCount; offset += batchSize)
            {
                var batch = Enumerable.Range(offset, Math.Min(batchSize, configuration.SeedProductCount - offset))
                    .Select(index =>
                    {
                        var product = products.Generate(index);
                        if (categoryIds.Length > 0) product.CategoryId = categoryIds[Random.Shared.Next(categoryIds.Length)];
                        return product;
                    }).ToArray();
                db.Products.AddRange(batch);
                db.ProductStocks.AddRange(batch.Select(product => new ProductStock { ProductId = product.Id, QuantityAvailable = Random.Shared.Next(5, 100) }));
                await db.SaveChangesAsync(cancellationToken);
            }
            logger.LogInformation("Seeded {Count} demo products.", configuration.SeedProductCount);
        }

        var targetUserCount = Math.Max(1, configuration.SeedUserCount);
        var existingUsers = await db.Users.CountAsync(cancellationToken);
        var virtualPasswordHash = passwords.Hash(configuration.VirtualUserPassword);
        if (existingUsers < targetUserCount)
        {
            for (var offset = existingUsers; offset < targetUserCount; offset += batchSize)
            {
                var count = Math.Min(batchSize, targetUserCount - offset);
                var batch = Enumerable.Range(offset, count).Select(index => new User
                {
                    FirstName = $"Virtual{index:D5}",
                    LastName = "Customer",
                    Email = $"virtual{index:D5}@minibox.local",
                    PasswordHash = virtualPasswordHash,
                    Role = UserRole.SimulatedUser,
                    City = "Kyiv"
                }).ToArray();
                db.Users.AddRange(batch);
                await db.SaveChangesAsync(cancellationToken);
            }
            logger.LogInformation("Seeded {Count} virtual users.", targetUserCount);
        }

        var simulatorUser = await db.Users.SingleAsync(user => user.Email == "virtual00000@minibox.local", cancellationToken);
        if (!passwords.Verify(configuration.VirtualUserPassword, simulatorUser.PasswordHash))
        {
            simulatorUser.PasswordHash = virtualPasswordHash;
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
