namespace ECommerce.Domain.ValueObjects;

public sealed record Address
{
    public string Street { get; init; }
    public string City { get; init; }
    public string ZipCode { get; init; }
    public string Country { get; init; }

    public Address(string street, string city, string zipCode, string country)
    {
        Validate(street, city, zipCode, country);
        Street = street;
        City = city;
        ZipCode = zipCode;
        Country = country;
    }

    private static void Validate(string street, string city, string zipCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be null or empty.", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty.", nameof(city));
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("ZipCode cannot be null or empty.", nameof(zipCode));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be null or empty.", nameof(country));
        if (street.Length > 200)
            throw new ArgumentException("Street cannot be longer than 200 characters.", nameof(street));
        if (city.Length > 100)
            throw new ArgumentException("City cannot be longer than 100 characters.", nameof(city));
        if (zipCode.Length > 20)
            throw new ArgumentException("ZipCode cannot be longer than 20 characters.", nameof(zipCode));
        if (country.Length > 100)
            throw new ArgumentException("Country cannot be longer than 100 characters.", nameof(country));
    }

    public override string ToString() => $"{Street}, {City}, {ZipCode}, {Country}";
}
