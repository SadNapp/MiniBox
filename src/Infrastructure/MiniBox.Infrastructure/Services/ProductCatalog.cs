using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MiniBox.Application.Common.Interfaces;
using MiniBox.Infrastructure.Persistence;
using MiniBox.Infrastructure.Persistence.Repositories;

namespace MiniBox.Infrastructure.Services;

public sealed class ProductCatalog(ApplicationDbContext db, ProductReadRepository reads, IRedisCacheService cache) : IProductCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ProductPage> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken,
        string? search = null, string? attributeKey = null, string? attributeValue = null, string? category = null,
        decimal? minPrice = null, decimal? maxPrice = null, string? sortBy = null, string? sortDirection = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var namespaceVersion = await cache.GetAsync("catalog:version", cancellationToken) ?? "0";
        var key = $"catalog:{namespaceVersion}:{page}:{pageSize}:{search?.Trim().ToLowerInvariant()}:{attributeKey?.Trim().ToLowerInvariant()}:{attributeValue?.Trim().ToLowerInvariant()}:{category?.Trim().ToLowerInvariant()}:{minPrice}:{maxPrice}:{sortBy}:{sortDirection}";
        var cached = await cache.GetAsync(key, cancellationToken);
        if (cached is not null)
        {
            var pageResult = JsonSerializer.Deserialize<ProductPage>(cached, JsonOptions);
            if (pageResult is not null) return pageResult;
        }
        var result = await reads.GetPageAsync(page, pageSize, cancellationToken, search, attributeKey, attributeValue, category, minPrice, maxPrice, sortBy, sortDirection);
        var response = new ProductPage(result.Items, page, pageSize, result.TotalCount);
        await cache.SetAsync(key, JsonSerializer.Serialize(response, JsonOptions), TimeSpan.FromSeconds(45), cancellationToken);
        return response;
    }

    public Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Products.AsNoTracking().Where(product => product.Id == id).Select(product => new ProductDto(
            product.Id, product.Sku, product.ProductName, product.Description,
            product.Price * (1m - product.Discount / 100m),
            db.ProductStocks.Where(stock => stock.ProductId == product.Id).Select(stock => stock.QuantityAvailable).FirstOrDefault(), product.Discount))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task InvalidateCatalogAsync(CancellationToken cancellationToken) =>
        await cache.IncrementAsync("catalog:version", cancellationToken);
}


