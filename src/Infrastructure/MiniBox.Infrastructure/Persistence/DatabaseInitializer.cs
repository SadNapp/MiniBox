using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniBox.Domain.Entities.Products;
using System.Data.Common;

namespace MiniBox.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    private const string BaselineMigrationId = "20261010000354_InitialCreate";

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await BaselineLegacyDatabaseAsync(db, cancellationToken);
        await db.Database.MigrateAsync(cancellationToken);
        if (!await db.Categories.AnyAsync(cancellationToken))
        {
            db.Categories.AddRange(
                new Category { Name = "Одноплатні комп’ютери", Description = "Raspberry Pi, Orange Pi та платформи для розробки." },
                new Category { Name = "Міні-ПК", Description = "Компактні настільні комп’ютери." },
                new Category { Name = "Компоненти", Description = "Пам’ять, накопичувачі, корпуси та аксесуари." });
            await db.SaveChangesAsync(cancellationToken);
        }
        if (await db.Products.AnyAsync(cancellationToken)) return;

        var categories = await db.Categories.ToDictionaryAsync(category => category.Name, category => category.Id, cancellationToken);
        var singleBoard = categories["Одноплатні комп’ютери"];
        var miniPc = categories["Міні-ПК"];
        var components = categories["Компоненти"];

        var products = new[]
        {
            new Product { Sku = "MBX-DEMO-001", CategoryId = singleBoard, ProductName = "Raspberry Pi 5 8GB", Description = "Потужний одноплатний комп'ютер для ваших проєктів.", Price = 89.00m },
            new Product { Sku = "MBX-DEMO-002", CategoryId = singleBoard, ProductName = "Raspberry Pi 5 4GB", Description = "Компактна платформа для розробки та домашньої автоматизації.", Price = 69.00m },
            new Product { Sku = "MBX-DEMO-003", CategoryId = singleBoard, ProductName = "NVIDIA Jetson Orin Nano", Description = "AI-комп'ютер для робототехніки та edge-рішень.", Price = 249.00m },
            new Product { Sku = "MBX-DEMO-004", CategoryId = singleBoard, ProductName = "Orange Pi 5 Plus 16GB", Description = "Високопродуктивна ARM-плата з підтримкою NVMe.", Price = 159.00m },
            new Product { Sku = "MBX-DEMO-005", CategoryId = miniPc, ProductName = "Intel NUC 13 Pro", Description = "Міні-ПК для робочого місця та домашньої лабораторії.", Price = 529.00m },
            new Product { Sku = "MBX-DEMO-006", CategoryId = components, ProductName = "Crucial DDR5 SODIMM 32GB", Description = "Оперативна пам'ять DDR5 для сумісних міні-ПК.", Price = 94.00m },
            new Product { Sku = "MBX-DEMO-007", CategoryId = components, ProductName = "Samsung 990 EVO 1TB", Description = "Швидкий NVMe SSD для компактних систем.", Price = 109.00m },
            new Product { Sku = "MBX-DEMO-008", CategoryId = components, ProductName = "Argon NEO 5 Case", Description = "Алюмінієвий корпус з активним охолодженням Raspberry Pi 5.", Price = 32.00m }
        };
        db.Products.AddRange(products);
        db.ProductStocks.AddRange(products.Select(product => new ProductStock { ProductId = product.Id, QuantityAvailable = 25 }));
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task BaselineLegacyDatabaseAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        await db.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            var connection = db.Database.GetDbConnection();
            var hasProducts = await ScalarBooleanAsync(connection, "SELECT to_regclass('public.\"Products\"') IS NOT NULL", cancellationToken);
            var hasHistory = await ScalarBooleanAsync(connection, "SELECT to_regclass('public.\"__EFMigrationsHistory\"') IS NOT NULL", cancellationToken);
            if (!hasProducts || hasHistory) return;

            // Earlier MiniBox builds used EnsureCreated. Mark their existing schema as the baseline so later migrations can upgrade it.
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            await db.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                    "MigrationId" character varying(150) NOT NULL PRIMARY KEY,
                    "ProductVersion" character varying(32) NOT NULL
                );
                """, cancellationToken);
            await db.Database.ExecuteSqlRawAsync($"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('{BaselineMigrationId}', '10.0.0') ON CONFLICT DO NOTHING", cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        finally { await db.Database.CloseConnectionAsync(); }
    }

    private static async Task<bool> ScalarBooleanAsync(DbConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is true;
    }
}
