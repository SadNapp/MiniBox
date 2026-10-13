using MiniBox.Domain.Exceptions;

namespace MiniBox.Domain.ValueObjects;

public class Address
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;

    public Address(string street, string city, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Вулиця є обов'язковим полем.");

        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("Місто є обов'язковим полем.");

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new DomainException("Поштовий індекс є обов'язковим полем.");

        if (string.IsNullOrWhiteSpace(country))
            throw new DomainException("Країна є обов'язковим полем.");

        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }
}
