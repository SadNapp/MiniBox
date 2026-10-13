using MiniBox.Domain.Exceptions;
using MiniBox.Domain.ValueObjects;

namespace MiniBox.Domain.UnitTests.ValueObjects;

public class AddressTests
{
    [Test]
    public void Constructor_StoresAllAddressParts()
    {
        var address = new Address("Khreshchatyk 1", "Kyiv", "01001", "Ukraine");

        Assert.That(address.Street, Is.EqualTo("Khreshchatyk 1"));
        Assert.That(address.City, Is.EqualTo("Kyiv"));
        Assert.That(address.PostalCode, Is.EqualTo("01001"));
        Assert.That(address.Country, Is.EqualTo("Ukraine"));
    }

    [TestCase("", "Kyiv", "01001", "Ukraine")]
    [TestCase("Street 1", " ", "01001", "Ukraine")]
    [TestCase("Street 1", "Kyiv", "", "Ukraine")]
    [TestCase("Street 1", "Kyiv", "01001", " ")]
    public void Constructor_RejectsMissingRequiredParts(string street, string city, string postalCode, string country)
    {
        Assert.Throws<DomainException>(() => new Address(street, city, postalCode, country));
    }
}
