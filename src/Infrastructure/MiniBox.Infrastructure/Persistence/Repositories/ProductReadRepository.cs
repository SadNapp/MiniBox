using Dapper;
using MiniBox.Application.Common.Interfaces;

namespace MiniBox.Infrastructure.Persistence.Repositories;

public sealed class ProductReadRepository(IDbConnectionFactory connectionFactory)
{
    public async Task<(IReadOnlyList<ProductDto> Items, int TotalCount)> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken,
        string? search = null, string? attributeKey = null, string? attributeValue = null, string? category = null,
        decimal? minPrice = null, decimal? maxPrice = null, string? sortBy = null, string? sortDirection = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        const string sql = """
            SELECT p."Id" AS "Id", p."Sku" AS "Sku", p."ProductName" AS "Name", p."Description",
                   p."Price" * (1 - p."Discount" / 100) AS "Price", p."Discount",
                   COALESCE(s."QuantityAvailable", 0) AS "Stock", COUNT(*) OVER() AS "TotalCount"
            FROM "Products" p
            LEFT JOIN "ProductStocks" s ON s."ProductId" = p."Id"
            LEFT JOIN "Categories" c ON c."Id" = p."CategoryId"
            WHERE (@Search IS NULL OR p."ProductName" ILIKE @Search OR p."Description" ILIKE @Search)
              AND (@AttributeKey IS NULL OR EXISTS (SELECT 1 FROM "ProductAttributes" pa WHERE pa."ProductId" = p."Id" AND lower(pa."Key") = lower(@AttributeKey) AND lower(pa."Value") = lower(@AttributeValue)))
              AND (@Category IS NULL OR c."Name" = @Category)
              AND (@MinPrice IS NULL OR p."Price" >= @MinPrice)
              AND (@MaxPrice IS NULL OR p."Price" <= @MaxPrice)
            ORDER BY CASE WHEN @SortBy = 'price' AND @SortDirection = 'asc' THEN p."Price" END ASC,
              CASE WHEN @SortBy = 'price' AND @SortDirection = 'desc' THEN p."Price" END DESC,
              CASE WHEN @SortBy = 'name' AND @SortDirection = 'desc' THEN p."ProductName" END DESC,
              CASE WHEN @SortBy = 'name' AND @SortDirection <> 'desc' THEN p."ProductName" END ASC, p."ProductName" ASC
            OFFSET @Offset LIMIT @Limit
            """;
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";
        var normalizedAttributeKey = string.IsNullOrWhiteSpace(attributeKey) || string.IsNullOrWhiteSpace(attributeValue) ? null : attributeKey.Trim();
        var normalizedAttributeValue = normalizedAttributeKey is null ? null : attributeValue!.Trim();
        using var connection = connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<ProductRow>(new CommandDefinition(sql,
            new { Offset = (page - 1) * pageSize, Limit = pageSize, Search = normalizedSearch, AttributeKey = normalizedAttributeKey, AttributeValue = normalizedAttributeValue,
                Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim(), MinPrice = minPrice, MaxPrice = maxPrice,
                SortBy = string.IsNullOrWhiteSpace(sortBy) ? "name" : sortBy.Trim().ToLowerInvariant(),
                SortDirection = string.IsNullOrWhiteSpace(sortDirection) ? "asc" : sortDirection.Trim().ToLowerInvariant() }, cancellationToken: cancellationToken));
        var list = rows.AsList();
        var totalCount = list.Count == 0
            ? await connection.ExecuteScalarAsync<int>(new CommandDefinition("""
                SELECT COUNT(*) FROM "Products" p
                LEFT JOIN "Categories" c ON c."Id" = p."CategoryId"
                WHERE (@Search IS NULL OR p."ProductName" ILIKE @Search OR p."Description" ILIKE @Search)
                  AND (@AttributeKey IS NULL OR EXISTS (SELECT 1 FROM "ProductAttributes" pa WHERE pa."ProductId" = p."Id" AND lower(pa."Key") = lower(@AttributeKey) AND lower(pa."Value") = lower(@AttributeValue)))
                  AND (@Category IS NULL OR c."Name" = @Category)
                  AND (@MinPrice IS NULL OR p."Price" >= @MinPrice)
                  AND (@MaxPrice IS NULL OR p."Price" <= @MaxPrice)
                """, new { Search = normalizedSearch, AttributeKey = normalizedAttributeKey, AttributeValue = normalizedAttributeValue,
                    Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim(), MinPrice = minPrice, MaxPrice = maxPrice }, cancellationToken: cancellationToken))
            : checked((int)list[0].TotalCount);
        return (list.Select(row => new ProductDto(row.Id, row.Sku, row.Name, row.Description, row.Price, row.Stock, row.Discount)).ToArray(), totalCount);
    }

    private sealed record ProductRow(Guid Id, string Sku, string Name, string Description, decimal Price, decimal Discount, int Stock, long TotalCount);
}
