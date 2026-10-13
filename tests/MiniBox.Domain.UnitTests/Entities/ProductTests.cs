namespace MiniBox.Domain.UnitTests.Entities;

public class ProductTests
{
    [Test]
    public void NewProduct_HasIdentityAndEmptyAttributes()
    {
        var product = new MiniBox.Domain.Entities.Products.Product();

        Assert.That(product.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(product.ProductAttributes, Is.Empty);
        Assert.That(product.CreatedAtUtc.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public void ProductStock_TotalInStock_IncludesAvailableAndReserved()
    {
        var stock = new MiniBox.Domain.Entities.Products.ProductStock
        {
            QuantityAvailable = 7,
            QuantityReserved = 3
        };

        Assert.That(stock.TotalInStock, Is.EqualTo(10));
    }
}
