using Microsoft.EntityFrameworkCore;

namespace MiniBox.IntegrationTests.Controllers;

public class OrdersControllerTests
{
    [Test]
    public async Task OrderDeliveryAddressAndAuditFields_PersistInDatabase()
    {
        using var fixture = new MiniBox.IntegrationTests.Infrastructure.DatabaseFixture();
        await using (var write = fixture.CreateContext())
        {
            write.Orders.Add(new MiniBox.Domain.Entities.Orders.Order
            {
                OrderNumber = "MB-INTEGRATION-1", CustomerName = "Test Customer", CustomerEmail = "test@example.test",
                DeliveryStreet = "Main street 1", DeliveryCity = "Kyiv", DeliveryPostalCode = "01001",
                DeliveryCountry = "Україна", DeliveryNotes = "Call on arrival"
            });
            await write.SaveChangesAsync();
            var saved = await write.Orders.SingleAsync();
            Assert.That(saved.CreatedBy, Is.EqualTo("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
            Assert.That(saved.CreatedAtUtc, Is.EqualTo(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc)));
        }

        await using var read = fixture.CreateContext();
        var order = await read.Orders.SingleAsync();
        Assert.That(order.DeliveryStreet, Is.EqualTo("Main street 1"));
        Assert.That(order.DeliveryCity, Is.EqualTo("Kyiv"));
        Assert.That(order.DeliveryPostalCode, Is.EqualTo("01001"));
        Assert.That(order.DeliveryNotes, Is.EqualTo("Call on arrival"));
    }
}
