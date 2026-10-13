using MediatR;
using MiniBox.Application.Common.Interfaces;

namespace MiniBox.Application.Features.Products.Queries.GetProductsCatalog;

public sealed record GetProductsCatalogQuery(int Page = 1, int PageSize = 24, string? Search = null, string? AttributeKey = null, string? AttributeValue = null, string? Category = null, decimal? MinPrice = null, decimal? MaxPrice = null, string? SortBy = null, string? SortDirection = null) : IRequest<ProductPage>;

public sealed class GetProductsCatalogHandler(IProductCatalog catalog) : IRequestHandler<GetProductsCatalogQuery, ProductPage>
{
    public Task<ProductPage> Handle(GetProductsCatalogQuery request, CancellationToken cancellationToken) =>
        catalog.GetPageAsync(request.Page, request.PageSize, cancellationToken, request.Search, request.AttributeKey, request.AttributeValue, request.Category, request.MinPrice, request.MaxPrice, request.SortBy, request.SortDirection);
}
