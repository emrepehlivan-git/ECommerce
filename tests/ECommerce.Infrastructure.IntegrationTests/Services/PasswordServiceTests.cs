using Microsoft.AspNetCore.Identity;

namespace ECommerce.Infrastructure.IntegrationTests.Services;

public sealed class PasswordServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly PasswordService _passwordService;

    public PasswordServiceTests()
    {
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            store.Object, null, null, null, null, null, null, null, null);
        
        _passwordService = new PasswordService(_userManagerMock.Object);
    }

    [Fact]
    public async Task GenerateEmailConfirmationTokenAsync_ShouldReturnToken()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var expectedToken = "email-confirmation-token";

        _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                        .ReturnsAsync(expectedToken);

        // Act
        var result = await _passwordService.GenerateEmailConfirmationTokenAsync(user);

        // Assert
        result.Should().Be(expectedToken);
        _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnSuccess_WhenTokenIsValid()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var token = "valid-token";
        var identityResult = IdentityResult.Success;

        _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, token))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await _passwordService.ConfirmEmailAsync(user, token);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeTrue();
        _userManagerMock.Verify(x => x.ConfirmEmailAsync(user, token), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnFailed_WhenTokenIsInvalid()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var token = "invalid-token";
        var identityError = new IdentityError { Code = "InvalidToken", Description = "Invalid token" };
        var identityResult = IdentityResult.Failed(identityError);

        _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, token))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await _passwordService.ConfirmEmailAsync(user, token);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Code.Should().Be("InvalidToken");
        _userManagerMock.Verify(x => x.ConfirmEmailAsync(user, token), Times.Once);
    }

    [Fact]
    public async Task GeneratePasswordResetTokenAsync_ShouldReturnToken()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var expectedToken = "password-reset-token";

        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                        .ReturnsAsync(expectedToken);

        // Act
        var result = await _passwordService.GeneratePasswordResetTokenAsync(user);

        // Assert
        result.Should().Be(expectedToken);
        _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnSuccess_WhenTokenIsValid()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var token = "valid-reset-token";
        var newPassword = "NewPassword123!";
        var identityResult = IdentityResult.Success;

        _userManagerMock.Setup(x => x.ResetPasswordAsync(user, token, newPassword))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await _passwordService.ResetPasswordAsync(user, token, newPassword);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeTrue();
        _userManagerMock.Verify(x => x.ResetPasswordAsync(user, token, newPassword), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnFailed_WhenTokenIsInvalid()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var token = "invalid-reset-token";
        var newPassword = "NewPassword123!";
        var identityError = new IdentityError { Code = "InvalidToken", Description = "Invalid token" };
        var identityResult = IdentityResult.Failed(identityError);

        _userManagerMock.Setup(x => x.ResetPasswordAsync(user, token, newPassword))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await _passwordService.ResetPasswordAsync(user, token, newPassword);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Code.Should().Be("InvalidToken");
        _userManagerMock.Verify(x => x.ResetPasswordAsync(user, token, newPassword), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldReturnFailed_WhenPasswordIsWeak()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var token = "valid-reset-token";
        var newPassword = "weak";
        var identityError = new IdentityError { Code = "PasswordTooShort", Description = "Password is too short" };
        var identityResult = IdentityResult.Failed(identityError);

        _userManagerMock.Setup(x => x.ResetPasswordAsync(user, token, newPassword))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await _passwordService.ResetPasswordAsync(user, token, newPassword);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Code.Should().Be("PasswordTooShort");
        _userManagerMock.Verify(x => x.ResetPasswordAsync(user, token, newPassword), Times.Once);
    }
} 