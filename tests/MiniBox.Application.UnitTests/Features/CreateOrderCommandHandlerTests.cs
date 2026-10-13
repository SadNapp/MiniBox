namespace MiniBox.Application.UnitTests.Features;

public class CreateOrderCommandHandlerTests
{
    [Test]
    public async Task Handle_UsesCurrentUserAndTrimsCustomerData()
    {
        var orderService = new FakeOrderService();
        var currentUser = new FakeCurrentUserService { UserId = Guid.NewGuid() };
        var handler = new MiniBox.Application.Features.Orders.Commands.CreateOrder.CreateOrderHandler(orderService, currentUser);
        var request = new MiniBox.Application.Features.Orders.Commands.CreateOrder.CreateOrderRequest(
            "  Ada Lovelace ", [new MiniBox.Application.Common.Interfaces.CreateOrderLine(Guid.NewGuid(), 2)], " ada@example.test ", "  Baker St 1 ", " London ", " NW1 ");

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.That(result.OrderId, Is.EqualTo(orderService.Result.OrderId));
        Assert.That(orderService.Command!.CustomerName, Is.EqualTo("Ada Lovelace"));
        Assert.That(orderService.Command.Email, Is.EqualTo("ada@example.test"));
        Assert.That(orderService.Command.UserId, Is.EqualTo(currentUser.UserId));
        Assert.That(orderService.Command.Items.Single().Quantity, Is.EqualTo(2));
        Assert.That(orderService.Command.DeliveryStreet, Is.EqualTo("Baker St 1"));
    }

    [Test]
    public void Validator_RejectsInvalidOrderLines()
    {
        var validator = new MiniBox.Application.Features.Orders.Commands.CreateOrder.CreateOrderValidator();
        var request = new MiniBox.Application.Features.Orders.Commands.CreateOrder.CreateOrderRequest(
            "A", [new MiniBox.Application.Common.Interfaces.CreateOrderLine(Guid.Empty, 0)], "bad-email", "", "", "");

        var result = validator.Validate(request);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Is.Not.Empty);
    }

    private sealed class FakeCurrentUserService : MiniBox.Application.Common.Interfaces.ICurrentUserService
    {
        public Guid? UserId { get; init; }
    }

    private sealed class FakeOrderService : MiniBox.Application.Common.Interfaces.IOrderService
    {
        public MiniBox.Application.Common.Interfaces.CreateOrderCommand? Command { get; private set; }
        public MiniBox.Application.Common.Interfaces.CreateOrderResult Result { get; } = new(Guid.NewGuid(), "MB-TEST", 30m);

        public Task<MiniBox.Application.Common.Interfaces.CreateOrderResult> CreateAsync(
            MiniBox.Application.Common.Interfaces.CreateOrderCommand command, CancellationToken cancellationToken)
        {
            Command = command;
            return Task.FromResult(Result);
        }

        public Task<IReadOnlyList<MiniBox.Application.Common.Interfaces.OrderSummaryDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<MiniBox.Application.Common.Interfaces.OrderSummaryDto>>([]);
    }
}
