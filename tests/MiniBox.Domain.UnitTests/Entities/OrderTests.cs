namespace MiniBox.Domain.UnitTests.Entities;

public class OrderTests
{
    [Test]
    public void TotalPrice_SumsLineTotals()
    {
        var order = new MiniBox.Domain.Entities.Orders.Order();
        order.Items.Add(new MiniBox.Domain.Entities.Orders.OrderItem { ProductId = Guid.NewGuid(), ProductName = "Board", UnitPrice = 50m, Quantity = 2 });
        order.Items.Add(new MiniBox.Domain.Entities.Orders.OrderItem { ProductId = Guid.NewGuid(), ProductName = "Case", UnitPrice = 15m, Quantity = 1 });

        Assert.That(order.TotalPrice, Is.EqualTo(115m));
    }

    [Test]
    public void DomainEvents_CanBeRaisedAndCleared()
    {
        var order = new MiniBox.Domain.Entities.Orders.Order();
        var domainEvent = new MiniBox.Domain.Events.OrderCreatedDomainEvent(order.Id, Guid.NewGuid(), 42m, "Board", DateTime.UtcNow);

        order.AddDomainEvent(domainEvent);
        Assert.That(order.DomainEvents, Does.Contain(domainEvent));

        order.ClearDomainEvents();
        Assert.That(order.DomainEvents, Is.Empty);
    }
}
