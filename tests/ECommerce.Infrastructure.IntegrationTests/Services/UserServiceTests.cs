using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Infrastructure.IntegrationTests.Services;

public sealed class UserServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly UserService _userService;

    public UserServiceTests()
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

        _userService = new UserService(_userManagerMock.Object, _signInManagerMock.Object);
    }

    [Fact]
    public async Task FindByEmailAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var email = "test@example.com";
        var user = User.Create(email, "Test", "User");
        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                        .ReturnsAsync(user);

        // Act
        var result = await _userService.FindByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task FindByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                        .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.FindByEmailAsync(email);

        // Assert
        result.Should().BeNull();
        _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User");
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                        .ReturnsAsync(user);

        // Act
        var result = await _userService.FindByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
        _userManagerMock.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                        .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.FindByIdAsync(userId);

        // Assert
        result.Should().BeNull();
        _userManagerMock.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
    }

    [Fact]
    public async Task GetUserByPrincipalAsync_ShouldReturnUser_WhenPrincipalIsValid()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id.ToString()) };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _userManagerMock.Setup(x => x.GetUserAsync(principal))
                        .ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByPrincipalAsync(principal);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
        _userManagerMock.Verify(x => x.GetUserAsync(principal), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess_WhenUserIsCreatedSuccessfully()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var password = "Password123!";
        var identityResult = IdentityResult.Success;

        _userManagerMock.Setup(x => x.CreateAsync(user, password))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await _userService.CreateAsync(user, password);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeTrue();
        _userManagerMock.Verify(x => x.CreateAsync(user, password), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnSuccess_WhenUserIsUpdatedSuccessfully()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var identityResult = IdentityResult.Success;

        _userManagerMock.Setup(x => x.UpdateAsync(user))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await _userService.UpdateAsync(user);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeTrue();
        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task CanSignInAsync_ShouldReturnTrue_WhenUserCanSignIn()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");

        _signInManagerMock.Setup(x => x.CanSignInAsync(user))
                          .ReturnsAsync(true);

        // Act
        var result = await _userService.CanSignInAsync(user);

        // Assert
        result.Should().BeTrue();
        _signInManagerMock.Verify(x => x.CanSignInAsync(user), Times.Once);
    }

    [Fact]
    public async Task CanSignInAsync_ShouldReturnFalse_WhenUserCannotSignIn()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");

        _signInManagerMock.Setup(x => x.CanSignInAsync(user))
                          .ReturnsAsync(false);

        // Act
        var result = await _userService.CanSignInAsync(user);

        // Assert
        result.Should().BeFalse();
        _signInManagerMock.Verify(x => x.CanSignInAsync(user), Times.Once);
    }

    [Fact]
    public void Users_ShouldReturnQueryableUsers()
    {
        // Arrange
        var users = new List<User>
        {
            User.Create("user1@example.com", "User", "One"),
            User.Create("user2@example.com", "User", "Two")
        }.AsQueryable();

        _userManagerMock.Setup(x => x.Users)
                        .Returns(users);

        // Act
        var result = _userService.Users;

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().Be(2);
        _userManagerMock.Verify(x => x.Users, Times.Once);
    }
} 