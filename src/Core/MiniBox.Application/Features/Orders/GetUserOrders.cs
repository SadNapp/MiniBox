using FluentValidation;
using MediatR;
using MiniBox.Application.Common.Interfaces;

namespace MiniBox.Application.Features.Orders.Queries.GetUserOrders;

public sealed record GetUserOrdersQuery(Guid UserId) : IRequest<IReadOnlyList<OrderSummaryDto>>;

public sealed class GetUserOrdersHandler(IOrderService orders) : IRequestHandler<GetUserOrdersQuery, IReadOnlyList<OrderSummaryDto>>
{
    public Task<IReadOnlyList<OrderSummaryDto>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken) =>
        orders.GetForUserAsync(request.UserId, cancellationToken);
}

public sealed class GetUserOrdersValidator : AbstractValidator<GetUserOrdersQuery>
{
    public GetUserOrdersValidator() => RuleFor(query => query.UserId).NotEmpty();
}
