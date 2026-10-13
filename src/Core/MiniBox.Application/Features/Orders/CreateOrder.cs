using FluentValidation;
using MediatR;
using MiniBox.Application.Common.Interfaces;

namespace MiniBox.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderRequest(string CustomerName, IReadOnlyList<CreateOrderLine> Items, string Email,
    string DeliveryStreet, string DeliveryCity, string DeliveryPostalCode, string DeliveryCountry = "Україна", string? DeliveryNotes = null) : IRequest<CreateOrderResult>;

public sealed class CreateOrderHandler(IOrderService orders, ICurrentUserService currentUser) : IRequestHandler<CreateOrderRequest, CreateOrderResult>
{
    public Task<CreateOrderResult> Handle(CreateOrderRequest request, CancellationToken cancellationToken) =>
        orders.CreateAsync(new CreateOrderCommand(request.CustomerName.Trim(), request.Email.Trim(), request.Items, currentUser.UserId,
            request.DeliveryStreet.Trim(), request.DeliveryCity.Trim(), request.DeliveryPostalCode.Trim(), request.DeliveryCountry.Trim(), request.DeliveryNotes?.Trim()), cancellationToken);
}

public sealed class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(request => request.CustomerName).NotEmpty().MaximumLength(160);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(request => request.Items).Cascade(CascadeMode.Stop).NotNull().NotEmpty()
            .Must(items => items.All(item => item.ProductId != Guid.Empty && item.Quantity > 0));
        RuleFor(request => request.DeliveryStreet).NotEmpty().MaximumLength(180);
        RuleFor(request => request.DeliveryCity).NotEmpty().MaximumLength(120);
        RuleFor(request => request.DeliveryPostalCode).NotEmpty().MaximumLength(20);
        RuleFor(request => request.DeliveryCountry).NotEmpty().MaximumLength(80);
        RuleFor(request => request.DeliveryNotes).MaximumLength(500);
    }
}
