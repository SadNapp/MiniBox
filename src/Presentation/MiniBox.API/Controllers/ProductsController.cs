using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiniBox.Application.Common.Interfaces;
using MiniBox.Application.Features.Products.Queries.GetProductsCatalog;
using MiniBox.Application.Features.Products.Commands.UpdateStock;
using MiniBox.Domain.Entities.Users;

namespace MiniBox.API.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 24,
        [FromQuery] string? search = null, [FromQuery] string? attributeKey = null, [FromQuery] string? attributeValue = null,
        [FromQuery] string? category = null,
        [FromQuery] decimal? minPrice = null, [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new GetProductsCatalogQuery(page, pageSize, search, attributeKey, attributeValue, category, minPrice, maxPrice, sortBy, sortDirection), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var product = await mediator.Send(new MiniBox.Application.Features.Products.Queries.GetProductById.GetProductByIdQuery(id), cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id:guid}/stock")]
    public async Task<IActionResult> UpdateStock(Guid id, UpdateProductStockRequest request, CancellationToken cancellationToken)
    {
        var updated = await mediator.Send(new UpdateProductStockCommand(id, request.QuantityAvailable), cancellationToken);
        return updated ? NoContent() : NotFound();
    }
}

public sealed record UpdateProductStockRequest(int QuantityAvailable);
