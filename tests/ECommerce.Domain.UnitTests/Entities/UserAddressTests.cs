namespace ECommerce.Domain.UnitTests.Entities;

public sealed class UserAddressTests
{
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private const string ValidLabel = "Home";
    private static readonly Address ValidAddress = new("123 Main St", "New York", "10001", "USA");

    [Fact]
    public void Create_WithValidParameters_ShouldCreateUserAddress()
    {
        // Act
        var userAddress = UserAddress.Create(ValidUserId, ValidLabel, ValidAddress);

        // Assert
        userAddress.Should().NotBeNull();
        userAddress.UserId.Should().Be(ValidUserId);
        userAddress.Label.Should().Be(ValidLabel);
        userAddress.Address.Should().Be(ValidAddress);
        userAddress.IsDefault.Should().BeFalse();
        userAddress.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithIsDefaultTrue_ShouldCreateUserAddressAsDefault()
    {
        // Act
        var userAddress = UserAddress.Create(ValidUserId, ValidLabel, ValidAddress, true);

        // Assert
        userAddress.Should().NotBeNull();
        userAddress.IsDefault.Should().BeTrue();
        userAddress.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidLabel_ShouldThrowArgumentException(string? label)
    {
        // Act
#pragma warning disable CS8604 // Possible null reference argument.
        var act = () => UserAddress.Create(ValidUserId, label, ValidAddress);
#pragma warning restore CS8604 // Possible null reference argument.

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Label cannot be null or empty.*");
    }

    [Fact]
    public void Create_WithLabelTooShort_ShouldThrowArgumentException()
    {
        // Arrange
        var shortLabel = "A";

        // Act
        var act = () => UserAddress.Create(ValidUserId, shortLabel, ValidAddress);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Label cannot be less than 2 characters.*");
    }

    [Fact]
    public void Create_WithLabelTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longLabel = new string('A', 51);

        // Act
        var act = () => UserAddress.Create(ValidUserId, longLabel, ValidAddress);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Label cannot be longer than 50 characters.*");
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateUserAddress()
    {
        // Arrange
        var userAddress = UserAddress.Create(ValidUserId, ValidLabel, ValidAddress);
        var newLabel = "Work";
        var newAddress = new Address("456 Business Ave", "Boston", "02101", "USA");

        // Act
        userAddress.Update(newLabel, newAddress);

        // Assert
        userAddress.Label.Should().Be(newLabel);
        userAddress.Address.Should().Be(newAddress);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WithInvalidLabel_ShouldThrowArgumentException(string? label)
    {
        // Arrange
        var userAddress = UserAddress.Create(ValidUserId, ValidLabel, ValidAddress);
        var newAddress = new Address("456 Business Ave", "Boston", "02101", "USA");

        // Act
#pragma warning disable CS8604 // Possible null reference argument.
        var act = () => userAddress.Update(label, newAddress);
#pragma warning restore CS8604 // Possible null reference argument.

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Label cannot be null or empty.*");
    }

    [Fact]
    public void SetAsDefault_ShouldSetIsDefaultToTrue()
    {
        // Arrange
        var userAddress = UserAddress.Create(ValidUserId, ValidLabel, ValidAddress);

        // Act
        userAddress.SetAsDefault();

        // Assert
        userAddress.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void UnsetAsDefault_ShouldSetIsDefaultToFalse()
    {
        // Arrange
        var userAddress = UserAddress.Create(ValidUserId, ValidLabel, ValidAddress, true);

        // Act
        userAddress.UnsetAsDefault();

        // Assert
        userAddress.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var userAddress = UserAddress.Create(ValidUserId, ValidLabel, ValidAddress);
        userAddress.Deactivate();

        // Act
        userAddress.Activate();

        // Assert
        userAddress.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var userAddress = UserAddress.Create(ValidUserId, ValidLabel, ValidAddress);

        // Act
        userAddress.Deactivate();

        // Assert
        userAddress.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData("Home")]
    [InlineData("Work")]
    [InlineData("Parents' House")]
    public void Create_WithValidLabels_ShouldCreateUserAddress(string label)
    {
        // Act
        var userAddress = UserAddress.Create(ValidUserId, label, ValidAddress);

        // Assert
        userAddress.Should().NotBeNull();
        userAddress.Label.Should().Be(label);
    }

    [Theory]
    [InlineData("AB")] // Minimum valid length
    [InlineData("Business Address for Office Use")] // Some long but valid label
    public void Create_WithBoundaryValidLabels_ShouldCreateUserAddress(string label)
    {
        // Act
        var userAddress = UserAddress.Create(ValidUserId, label, ValidAddress);

        // Assert
        userAddress.Should().NotBeNull();
        userAddress.Label.Should().Be(label);
    }

    [Fact]
    public void Create_WithMaximumValidLabel_ShouldCreateUserAddress()
    {
        // Arrange
        var maxLabel = new string('A', 50);

        // Act
        var userAddress = UserAddress.Create(ValidUserId, maxLabel, ValidAddress);

        // Assert
        userAddress.Should().NotBeNull();
        userAddress.Label.Should().Be(maxLabel);
    }
} 