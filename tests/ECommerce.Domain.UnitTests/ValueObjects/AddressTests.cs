namespace ECommerce.Domain.UnitTests.ValueObjects;

public sealed class AddressTests
{
    private const string ValidStreet = "123 Main Street";
    private const string ValidCity = "New York";
    private const string ValidState = "NY";
    private const string ValidZipCode = "10001";
    private const string ValidCountry = "USA";

    [Theory]
    [InlineData(null, ValidCity, ValidState, ValidZipCode, ValidCountry)]
    [InlineData("", ValidCity, ValidState, ValidZipCode, ValidCountry)]
    [InlineData(" ", ValidCity, ValidState, ValidZipCode, ValidCountry)]
    public void Constructor_WithInvalidStreet_ShouldThrowArgumentException(string? street, string city, string state, string zipCode, string country)
    {
        // Act
#pragma warning disable CS8604 // Possible null reference argument.
        var act = () => new Address(street, city, state, zipCode, country);
#pragma warning restore CS8604 // Possible null reference argument.

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Street cannot be null or empty.*");
    }

    [Theory]
    [InlineData(ValidStreet, null, ValidState, ValidZipCode, ValidCountry)]
    [InlineData(ValidStreet, "", ValidState, ValidZipCode, ValidCountry)]
    [InlineData(ValidStreet, " ", ValidState, ValidZipCode, ValidCountry)]
    public void Constructor_WithInvalidCity_ShouldThrowArgumentException(string street, string? city, string state, string zipCode, string country)
    {
        // Act
#pragma warning disable CS8604 // Possible null reference argument.
        var act = () => new Address(street, city, state, zipCode, country);
#pragma warning restore CS8604 // Possible null reference argument.

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("City cannot be null or empty.*");
    }

    [Theory]
    [InlineData(ValidStreet, ValidCity, null, ValidZipCode, ValidCountry)]
    [InlineData(ValidStreet, ValidCity, "", ValidZipCode, ValidCountry)]
    [InlineData(ValidStreet, ValidCity, " ", ValidZipCode, ValidCountry)]
    public void Constructor_WithInvalidState_ShouldThrowArgumentException(string street, string city, string? state, string zipCode, string country)
    {
        // Act
#pragma warning disable CS8604 // Possible null reference argument.
        var act = () => new Address(street, city, state, zipCode, country);
#pragma warning restore CS8604 // Possible null reference argument.

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("State cannot be null or empty.*");
    }

    [Theory]
    [InlineData(ValidStreet, ValidCity, ValidState, null, ValidCountry)]
    [InlineData(ValidStreet, ValidCity, ValidState, "", ValidCountry)]
    [InlineData(ValidStreet, ValidCity, ValidState, " ", ValidCountry)]
    public void Constructor_WithInvalidZipCode_ShouldThrowArgumentException(string street, string city, string state, string? zipCode, string country)
    {
        // Act
#pragma warning disable CS8604 // Possible null reference argument.
        var act = () => new Address(street, city, state, zipCode, country);
#pragma warning restore CS8604 // Possible null reference argument.

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("ZipCode cannot be null or empty.*");
    }

    [Theory]
    [InlineData(ValidStreet, ValidCity, ValidState, ValidZipCode, null)]
    [InlineData(ValidStreet, ValidCity, ValidState, ValidZipCode, "")]
    [InlineData(ValidStreet, ValidCity, ValidState, ValidZipCode, " ")]
    public void Constructor_WithInvalidCountry_ShouldThrowArgumentException(string street, string city, string state, string zipCode, string? country)
    {
        // Act
#pragma warning disable CS8604 // Possible null reference argument.
        var act = () => new Address(street, city, state, zipCode, country);
#pragma warning restore CS8604 // Possible null reference argument.

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Country cannot be null or empty.*");
    }

    [Fact]
    public void Constructor_WithTooLongStreet_ShouldThrowArgumentException()
    {
        // Arrange
        var longStreet = new string('a', 201);

        // Act
        var act = () => new Address(longStreet, ValidCity, ValidState, ValidZipCode, ValidCountry);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Street cannot be longer than 200 characters.*");
    }

    [Fact]
    public void Constructor_WithTooLongCity_ShouldThrowArgumentException()
    {
        // Arrange
        var longCity = new string('a', 101);

        // Act
        var act = () => new Address(ValidStreet, longCity, ValidState, ValidZipCode, ValidCountry);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("City cannot be longer than 100 characters.*");
    }

    [Fact]
    public void Constructor_WithTooLongState_ShouldThrowArgumentException()
    {
        // Arrange
        var longState = new string('a', 101);

        // Act
        var act = () => new Address(ValidStreet, ValidCity, longState, ValidZipCode, ValidCountry);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("State cannot be longer than 100 characters.*");
    }

    [Fact]
    public void Constructor_WithTooLongZipCode_ShouldThrowArgumentException()
    {
        // Arrange
        var longZipCode = new string('a', 21);

        // Act
        var act = () => new Address(ValidStreet, ValidCity, ValidState, longZipCode, ValidCountry);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("ZipCode cannot be longer than 20 characters.*");
    }

    [Fact]
    public void Constructor_WithTooLongCountry_ShouldThrowArgumentException()
    {
        // Arrange
        var longCountry = new string('a', 101);

        // Act
        var act = () => new Address(ValidStreet, ValidCity, ValidState, ValidZipCode, longCountry);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Country cannot be longer than 100 characters.*");
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateAddress()
    {
        // Act
        var address = new Address(ValidStreet, ValidCity, ValidState, ValidZipCode, ValidCountry);

        // Assert
        address.Should().NotBeNull();
        address.Street.Should().Be(ValidStreet);
        address.City.Should().Be(ValidCity);
        address.State.Should().Be(ValidState);
        address.ZipCode.Should().Be(ValidZipCode);
        address.Country.Should().Be(ValidCountry);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedAddress()
    {
        // Arrange
        var address = new Address(ValidStreet, ValidCity, ValidState, ValidZipCode, ValidCountry);

        // Act
        var result = address.ToString();

        // Assert
        result.Should().Be($"{ValidStreet}, {ValidCity}, {ValidState}, {ValidZipCode}, {ValidCountry}");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var address1 = new Address(ValidStreet, ValidCity, ValidState, ValidZipCode, ValidCountry);
        var address2 = new Address(ValidStreet, ValidCity, ValidState, ValidZipCode, ValidCountry);

        // Act & Assert
        address1.Equals(address2).Should().BeTrue();
        (address1 == address2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var address1 = new Address(ValidStreet, ValidCity, ValidState, ValidZipCode, ValidCountry);
        var address2 = new Address("456 Oak Street", ValidCity, ValidState, ValidZipCode, ValidCountry);

        // Act & Assert
        address1.Equals(address2).Should().BeFalse();
        (address1 == address2).Should().BeFalse();
    }

    [Theory]
    [InlineData("123 Main Street", "New York", "NY", "10001", "USA")]
    [InlineData("456 Oak Avenue", "Los Angeles", "CA", "90210", "USA")]
    [InlineData("789 Pine Road", "Chicago", "IL", "60601", "USA")]
    public void Constructor_WithVariousValidInputs_ShouldCreateAddress(string street, string city, string state, string zipCode, string country)
    {
        // Act
        var address = new Address(street, city, state, zipCode, country);

        // Assert
        address.Should().NotBeNull();
        address.Street.Should().Be(street);
        address.City.Should().Be(city);
        address.State.Should().Be(state);
        address.ZipCode.Should().Be(zipCode);
        address.Country.Should().Be(country);
    }
} 