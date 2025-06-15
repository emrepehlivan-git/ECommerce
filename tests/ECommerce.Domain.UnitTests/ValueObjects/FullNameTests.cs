namespace ECommerce.Domain.UnitTests.ValueObjects;

public sealed class FullNameTests
{
    [Theory]
    [InlineData(null, "Doe")]
    [InlineData("", "Doe")]
    [InlineData(" ", "Doe")]
    public void Create_WithInvalidFirstName_ShouldThrowArgumentException(string? firstName, string lastName)
    {
        // Act
#pragma warning disable CS8604 // Possible null reference argument.
        var act = () => FullName.Create(firstName, lastName);
#pragma warning restore CS8604 // Possible null reference argument.

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("First name cannot be null or empty.*");
    }

    [Theory]
    [InlineData("John", null)]
    [InlineData("John", "")]
    [InlineData("John", " ")]
    public void Create_WithInvalidLastName_ShouldThrowArgumentException(string? firstName, string? lastName)
    {
        // Act
#pragma warning disable CS8604 // Possible null reference argument.
        var act = () => FullName.Create(firstName, lastName);
#pragma warning restore CS8604 // Possible null reference argument.

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Last name cannot be null or empty.*");
    }

    [Theory]
    [InlineData("Jo", "Doe")]
    [InlineData("J", "Doe")]
    public void Create_WithShortFirstName_ShouldThrowArgumentException(string firstName, string lastName)
    {
        // Act
        var act = () => FullName.Create(firstName, lastName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("First name cannot be less than 3 characters.*");
    }

    [Theory]
    [InlineData("John", "Do")]
    [InlineData("John", "D")]
    public void Create_WithShortLastName_ShouldThrowArgumentException(string firstName, string lastName)
    {
        // Act
        var act = () => FullName.Create(firstName, lastName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Last name cannot be less than 3 characters.*");
    }

    [Theory]
    [InlineData("John", "Doe")]
    [InlineData("John", "Smith")]
    [InlineData("Mary", "Johnson")]
    public void Create_WithValidNames_ShouldCreateFullName(string firstName, string lastName)
    {
        // Act
        var fullName = FullName.Create(firstName, lastName);

        // Assert
        fullName.Should().NotBeNull();
        fullName.FirstName.Should().Be(firstName.Trim());
        fullName.LastName.Should().Be(lastName.Trim());
    }

    [Theory]
    [InlineData(" John ", " Doe ")]
    public void Create_WithWhitespace_ShouldTrimNames(string firstName, string lastName)
    {
        // Act
        var fullName = FullName.Create(firstName, lastName);

        // Assert
        fullName.FirstName.Should().Be("John");
        fullName.LastName.Should().Be("Doe");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedFullName()
    {
        // Arrange
        var fullName = FullName.Create("John", "Doe");

        // Act
        var result = fullName.ToString();

        // Assert
        result.Should().Be("John Doe");
    }

    [Fact]
    public void Equals_WithSameNames_ShouldReturnTrue()
    {
        // Arrange
        var fullName1 = FullName.Create("John", "Doe");
        var fullName2 = FullName.Create("John", "Doe");

        // Act & Assert
        fullName1.Equals(fullName2).Should().BeTrue();
        (fullName1 == fullName2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentNames_ShouldReturnFalse()
    {
        // Arrange
        var fullName1 = FullName.Create("John", "Doe");
        var fullName2 = FullName.Create("Jane", "Doe");

        // Act & Assert
        fullName1.Equals(fullName2).Should().BeFalse();
        (fullName1 == fullName2).Should().BeFalse();
    }
}