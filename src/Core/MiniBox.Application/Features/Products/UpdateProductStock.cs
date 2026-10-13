using FluentValidation;
using MediatR;
using MiniBox.Application.Common.Interfaces;

namespace MiniBox.Application.Features.Products.Commands.UpdateStock;

public sealed record UpdateProductStockCommand(Guid ProductId, int QuantityAvailable) : IRequest<bool>;

public sealed class UpdateProductStockHandler(IProductStockService stock) : IRequestHandler<UpdateProductStockCommand, bool>
{
    public Task<bool> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken) =>
        stock.SetAvailableAsync(request.ProductId, request.QuantityAvailable, cancellationToken);
}

public sealed class UpdateProductStockValidator : AbstractValidator<UpdateProductStockCommand>
{
    public UpdateProductStockValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.QuantityAvailable).GreaterThanOrEqualTo(0);
    }
}
