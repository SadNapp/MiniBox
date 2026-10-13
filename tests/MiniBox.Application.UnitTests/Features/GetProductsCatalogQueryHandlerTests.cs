namespace MiniBox.Application.UnitTests.Features;

public class GetProductsCatalogQueryHandlerTests
{
    [Test]
    public async Task Handle_ForwardsPagingAndFiltersToCatalog()
    {
        var catalog = new FakeProductCatalog();
        var handler = new MiniBox.Application.Features.Products.Queries.GetProductsCatalog.GetProductsCatalogHandler(catalog);
        var expected = new MiniBox.Application.Common.Interfaces.ProductPage([], 2, 12, 27);

        catalog.Page = expected;
        var result = await handler.Handle(new MiniBox.Application.Features.Products.Queries.GetProductsCatalog.GetProductsCatalogQuery(
            2, 12, "board", "platform", "mini", "Components"), CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        Assert.That(catalog.Arguments, Is.EqualTo((2, 12, "board", "platform", "mini", "Components")));
    }

    private sealed class FakeProductCatalog : MiniBox.Application.Common.Interfaces.IProductCatalog
    {
        public MiniBox.Application.Common.Interfaces.ProductPage Page { get; set; } = new([], 1, 24, 0);
        public (int Page, int PageSize, string? Search, string? AttributeKey, string? AttributeValue, string? Category) Arguments { get; private set; }

        public Task<MiniBox.Application.Common.Interfaces.ProductPage> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken,
            string? search = null, string? attributeKey = null, string? attributeValue = null, string? category = null,
            decimal? minPrice = null, decimal? maxPrice = null, string? sortBy = null, string? sortDirection = null)
        {
            Arguments = (page, pageSize, search, attributeKey, attributeValue, category);
            return Task.FromResult(Page);
        }

        public Task<MiniBox.Application.Common.Interfaces.ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<MiniBox.Application.Common.Interfaces.ProductDto?>(null);
        public Task InvalidateCatalogAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
