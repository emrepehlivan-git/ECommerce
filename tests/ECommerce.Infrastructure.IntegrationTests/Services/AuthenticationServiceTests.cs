using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Infrastructure.IntegrationTests.Services;

public sealed class AuthenticationServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly AuthenticationService _authenticationService;

    public AuthenticationServiceTests()
    {
        var store = new Mock<IUserStore<User>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        _userManagerMock = new Mock<UserManager<User>>(
            store.Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        _signInManagerMock = new Mock<SignInManager<User>>(
            _userManagerMock.Object, contextAccessor.Object, 
            claimsFactory.Object, null, null, null, null);

        _authenticationService = new AuthenticationService(_userManagerMock.Object, _signInManagerMock.Object);
    }

    [Fact]
    public async Task PasswordSignInAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";
        var isPersistent = false;
        var lockoutOnFailure = false;
        var signInResult = SignInResult.Success;

        _signInManagerMock.Setup(x => x.PasswordSignInAsync(email, password, isPersistent, lockoutOnFailure))
                          .ReturnsAsync(signInResult);

        // Act
        var result = await _authenticationService.PasswordSignInAsync(email, password, isPersistent, lockoutOnFailure);

        // Assert
        result.Should().Be(signInResult);
        result.Succeeded.Should().BeTrue();
        _signInManagerMock.Verify(x => x.PasswordSignInAsync(email, password, isPersistent, lockoutOnFailure), Times.Once);
    }

    [Fact]
    public async Task PasswordSignInAsync_ShouldReturnFailed_WhenCredentialsAreInvalid()
    {
        // Arrange
        var email = "test@example.com";
        var password = "WrongPassword";
        var isPersistent = false;
        var lockoutOnFailure = false;
        var signInResult = SignInResult.Failed;

        _signInManagerMock.Setup(x => x.PasswordSignInAsync(email, password, isPersistent, lockoutOnFailure))
                          .ReturnsAsync(signInResult);

        // Act
        var result = await _authenticationService.PasswordSignInAsync(email, password, isPersistent, lockoutOnFailure);

        // Assert
        result.Should().Be(signInResult);
        result.Succeeded.Should().BeFalse();
        _signInManagerMock.Verify(x => x.PasswordSignInAsync(email, password, isPersistent, lockoutOnFailure), Times.Once);
    }

    [Fact]
    public async Task PasswordSignInAsync_ShouldReturnLockedOut_WhenAccountIsLockedOut()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";
        var isPersistent = false;
        var lockoutOnFailure = true;
        var signInResult = SignInResult.LockedOut;

        _signInManagerMock.Setup(x => x.PasswordSignInAsync(email, password, isPersistent, lockoutOnFailure))
                          .ReturnsAsync(signInResult);

        // Act
        var result = await _authenticationService.PasswordSignInAsync(email, password, isPersistent, lockoutOnFailure);

        // Assert
        result.Should().Be(signInResult);
        result.IsLockedOut.Should().BeTrue();
        _signInManagerMock.Verify(x => x.PasswordSignInAsync(email, password, isPersistent, lockoutOnFailure), Times.Once);
    }

    [Fact]
    public async Task SignOutAsync_ShouldCallSignInManagerSignOut()
    {
        // Arrange
        _signInManagerMock.Setup(x => x.SignOutAsync())
                          .Returns(Task.CompletedTask);

        // Act
        await _authenticationService.SignOutAsync();

        // Assert
        _signInManagerMock.Verify(x => x.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async Task CheckPasswordAsync_ShouldReturnTrue_WhenPasswordIsCorrect()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var password = "Password123!";

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, password))
                        .ReturnsAsync(true);

        // Act
        var result = await _authenticationService.CheckPasswordAsync(user, password);

        // Assert
        result.Should().BeTrue();
        _userManagerMock.Verify(x => x.CheckPasswordAsync(user, password), Times.Once);
    }

    [Fact]
    public async Task CheckPasswordAsync_ShouldReturnFalse_WhenPasswordIsIncorrect()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var password = "WrongPassword";

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, password))
                        .ReturnsAsync(false);

        // Act
        var result = await _authenticationService.CheckPasswordAsync(user, password);

        // Assert
        result.Should().BeFalse();
        _userManagerMock.Verify(x => x.CheckPasswordAsync(user, password), Times.Once);
    }
} 