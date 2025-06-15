namespace ECommerce.Domain.UnitTests.Entities;

public sealed class UserTests
{
    private const string ValidEmail = "test@example.com";
    private const string ValidFirstName = "John";
    private const string ValidLastName = "Doe";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidEmail_ShouldThrowArgumentException(string? email)
    {
        // Act
#pragma warning disable CS8604 // Possible null reference argument.
        var act = () => User.Create(email, ValidFirstName, ValidLastName);
#pragma warning restore CS8604 // Possible null reference argument.

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot be null or empty.*");
    }

    [Theory]
    [InlineData("invalid-email")]
    public void Create_WithInvalidEmailFormat_ShouldThrowArgumentException(string email)
    {
        // Act
        var act = () => User.Create(email, ValidFirstName, ValidLastName);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("test@example.com", "John", "Doe")]
    [InlineData("another@example.com", "Jane", "Smith")]
    public void Create_WithValidParameters_ShouldCreateUser(string email, string firstName, string lastName)
    {
        // Act
        var user = User.Create(email, firstName, lastName);

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.UserName.Should().Be(email);
        user.FullName.Should().NotBeNull();
        user.FullName.FirstName.Should().Be(firstName);
        user.FullName.LastName.Should().Be(lastName);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = User.Create(ValidEmail, ValidFirstName, ValidLastName);

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = User.Create(ValidEmail, ValidFirstName, ValidLastName);
        user.Deactivate();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("Jane", "Smith")]
    [InlineData("Alice", "Johnson")]
    public void UpdateName_WithValidNames_ShouldUpdateFullName(string firstName, string lastName)
    {
        // Arrange
        var user = User.Create(ValidEmail, ValidFirstName, ValidLastName);

        // Act
        user.UpdateName(firstName, lastName);

        // Assert
        user.FullName.FirstName.Should().Be(firstName);
        user.FullName.LastName.Should().Be(lastName);
    }

    [Theory]
    [InlineData(null, "Smith")]
    [InlineData("", "Smith")]
    [InlineData(" ", "Smith")]
    public void UpdateName_WithInvalidFirstName_ShouldThrowArgumentException(string? firstName, string lastName)
    {
        // Arrange
        var user = User.Create(ValidEmail, ValidFirstName, ValidLastName);

        // Act
#pragma warning disable CS8604 // Possible null reference argument.
        var act = () => user.UpdateName(firstName, lastName);
#pragma warning restore CS8604 // Possible null reference argument.

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("First name cannot be null or empty.*");
    }

    [Theory]
    [InlineData("Jane", null)]
    [InlineData("Jane", "")]
    [InlineData("Jane", " ")]
    public void UpdateName_WithInvalidLastName_ShouldThrowArgumentException(string? firstName, string? lastName)
    {
        // Arrange
        var user = User.Create(ValidEmail, ValidFirstName, ValidLastName);

        // Act
#pragma warning disable CS8604 // Possible null reference argument.
        var act = () => user.UpdateName(firstName, lastName);
#pragma warning restore CS8604 // Possible null reference argument.

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Last name cannot be null or empty.*");
    }
}