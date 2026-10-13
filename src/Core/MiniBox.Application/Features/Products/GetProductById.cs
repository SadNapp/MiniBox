using FluentValidation;
using MediatR;
using MiniBox.Application.Common.Interfaces;

namespace MiniBox.Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId) : IRequest<ProductDto?>;

public sealed class GetProductByIdHandler(IProductCatalog catalog) : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken) =>
        catalog.GetByIdAsync(request.ProductId, cancellationToken);
}

public sealed class GetProductByIdValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdValidator() => RuleFor(query => query.ProductId).NotEmpty();
}
