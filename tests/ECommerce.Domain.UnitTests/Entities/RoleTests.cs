using ECommerce.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ECommerce.Domain.UnitTests.Entities;

public sealed class RoleTests
{
    private const string ValidName = "Admin";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Act
#pragma warning disable CS8604 // Possible null reference argument.
        var act = () => Role.Create(name);
#pragma warning restore CS8604 // Possible null reference argument.

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name cannot be null or empty.*");
    }

    [Theory]
    [InlineData("A")]
    public void Create_WithShortName_ShouldThrowArgumentException(string name)
    {
        // Act
        var act = () => Role.Create(name);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name cannot be less than 2 characters.*");
    }

    [Fact]
    public void Create_WithLongName_ShouldThrowArgumentException()
    {
        // Arrange
        var longName = new string('a', 101);

        // Act
        var act = () => Role.Create(longName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name cannot be longer than 100 characters.*");
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("User")]
    [InlineData("Manager")]
    public void Create_WithValidName_ShouldCreateRole(string name)
    {
        // Act
        var role = Role.Create(name);

        // Assert
        role.Should().NotBeNull();
        role.Name.Should().Be(name);
    }

    [Theory]
    [InlineData("A")]
    public void UpdateName_WithShortName_ShouldThrowArgumentException(string name)
    {
        // Arrange
        var role = Role.Create(ValidName);

        // Act
        var act = () => role.UpdateName(name);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name cannot be less than 2 characters.*");
    }

    [Fact]
    public void UpdateName_WithLongName_ShouldThrowArgumentException()
    {
        // Arrange
        var role = Role.Create(ValidName);
        var longName = new string('a', 101);

        // Act
        var act = () => role.UpdateName(longName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name cannot be longer than 100 characters.*");
    }

    [Theory]
    [InlineData("SuperAdmin")]
    [InlineData("Editor")]
    [InlineData("Viewer")]
    public void UpdateName_WithValidName_ShouldUpdateName(string name)
    {
        // Arrange
        var role = Role.Create(ValidName);

        // Act
        role.UpdateName(name);

        // Assert
        role.Name.Should().Be(name);
    }
}