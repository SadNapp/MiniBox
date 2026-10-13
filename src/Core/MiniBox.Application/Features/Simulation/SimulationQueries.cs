using FluentValidation;
using MediatR;
using MiniBox.Application.Common.Interfaces;

namespace MiniBox.Application.Features.Simulation;

public interface IOnlineUserCounter
{
    Task<long> GetCountAsync(CancellationToken cancellationToken);
}

public sealed record GetActiveUsersOnlineQuery : IRequest<long>;

public sealed class GetActiveUsersOnlineHandler(IOnlineUserCounter onlineUsers) : IRequestHandler<GetActiveUsersOnlineQuery, long>
{
    public Task<long> Handle(GetActiveUsersOnlineQuery request, CancellationToken cancellationToken) => onlineUsers.GetCountAsync(cancellationToken);
}

public sealed record ProcessSimulatedCheckoutCommand(string CustomerName, string Email, Guid ProductId, int Quantity) : IRequest<CreateOrderResult>;

public sealed class ProcessSimulatedCheckoutHandler(IOrderService orders) : IRequestHandler<ProcessSimulatedCheckoutCommand, CreateOrderResult>
{
    public Task<CreateOrderResult> Handle(ProcessSimulatedCheckoutCommand request, CancellationToken cancellationToken) =>
        orders.CreateAsync(new CreateOrderCommand(request.CustomerName, request.Email, [new CreateOrderLine(request.ProductId, request.Quantity)],
            DeliveryStreet: "Demo street 1", DeliveryCity: "Київ", DeliveryPostalCode: "01001", DeliveryCountry: "Україна"), cancellationToken);
}

public sealed class ProcessSimulatedCheckoutValidator : AbstractValidator<ProcessSimulatedCheckoutCommand>
{
    public ProcessSimulatedCheckoutValidator()
    {
        RuleFor(command => command.CustomerName).NotEmpty().MaximumLength(160);
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Quantity).GreaterThan(0);
    }
}
