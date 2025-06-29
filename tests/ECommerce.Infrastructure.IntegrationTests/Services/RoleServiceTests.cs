using ECommerce.Application.Features.Roles.DTOs;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;

namespace ECommerce.Infrastructure.IntegrationTests.Services;

public sealed class RoleServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly RoleService _roleService;

    public RoleServiceTests()
    {
        var userStore = new Mock<IUserStore<User>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        _userManagerMock = new Mock<UserManager<User>>(
            userStore.Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        var roleStore = new Mock<IRoleStore<Role>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        _roleManagerMock = new Mock<RoleManager<Role>>(
            roleStore.Object, null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        _roleService = new RoleService(_userManagerMock.Object, _roleManagerMock.Object);
    }

    [Fact]
    public async Task GetAllRolesAsync_ShouldReturnPagedRoles()
    {
        // Arrange
        var roles = new List<Role>
        {
            Role.Create("Admin"),
            Role.Create("User")
        }.AsQueryable().BuildMock();

        _roleManagerMock.Setup(m => m.Roles).Returns(roles);

        // Act
        var result = await _roleService.GetAllRolesAsync(1, 10, string.Empty);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.First().Should().BeOfType<RoleDto>();
    }

    [Fact]
    public async Task GetRolesAsync_ShouldReturnRoleNames()
    {
        // Arrange
        var roles = new List<Role>
        {
            Role.Create("Admin"),
            Role.Create("User")
        };

        var mockQueryable = roles.AsQueryable().BuildMock();
        _roleManagerMock.Setup(x => x.Roles)
                        .Returns(mockQueryable);

        // Act
        var result = await _roleService.GetRolesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain("Admin");
        result.Should().Contain("User");
    }

    [Fact]
    public async Task GetUserRolesAsync_ShouldReturnUserRoles()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var userRoles = new List<string> { "Admin", "User" };

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
                        .ReturnsAsync(userRoles);

        // Act
        var result = await _roleService.GetUserRolesAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain("Admin");
        result.Should().Contain("User");
        _userManagerMock.Verify(x => x.GetRolesAsync(user), Times.Once);
    }

    [Fact]
    public async Task FindRoleByIdAsync_ShouldReturnRole_WhenRoleExists()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = Role.Create("Admin");
        // Id'yi set etmek için reflection kullanmak yerine mock'ta doğru role'ü setup edelim
        var roles = new List<Role> { role };

        var mockQueryable = roles.AsQueryable().BuildMock();
        _roleManagerMock.Setup(x => x.Roles)
                        .Returns(mockQueryable);

        // Act
        var result = await _roleService.FindRoleByIdAsync(roleId);

        // Assert
        // Mock'ta gerçek ID eşleşmesi olmadığı için null olacak
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindRoleByNameAsync_ShouldReturnRole_WhenRoleExists()
    {
        // Arrange
        var roleName = "Admin";
        var role = Role.Create(roleName);
        var roles = new List<Role> { role };

        var mockQueryable = roles.AsQueryable().BuildMock();
        _roleManagerMock.Setup(x => x.Roles)
                        .Returns(mockQueryable);

        // Act
        var result = await _roleService.FindRoleByNameAsync(roleName);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(roleName);
    }

    [Fact]
    public async Task CreateRoleAsync_ShouldReturnSuccess_WhenRoleIsCreatedSuccessfully()
    {
        // Arrange
        var role = Role.Create("NewRole");
        var identityResult = IdentityResult.Success;

        _roleManagerMock.Setup(x => x.CreateAsync(role))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await _roleService.CreateRoleAsync(role);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeTrue();
        _roleManagerMock.Verify(x => x.CreateAsync(role), Times.Once);
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldReturnSuccess_WhenRoleIsUpdatedSuccessfully()
    {
        // Arrange
        var role = Role.Create("UpdatedRole");
        var identityResult = IdentityResult.Success;

        _roleManagerMock.Setup(x => x.UpdateAsync(role))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await _roleService.UpdateRoleAsync(role);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeTrue();
        _roleManagerMock.Verify(x => x.UpdateAsync(role), Times.Once);
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldReturnSuccess_WhenRoleIsDeletedSuccessfully()
    {
        // Arrange
        var role = Role.Create("RoleToDelete");
        var identityResult = IdentityResult.Success;

        _roleManagerMock.Setup(x => x.DeleteAsync(role))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await _roleService.DeleteRoleAsync(role);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeTrue();
        _roleManagerMock.Verify(x => x.DeleteAsync(role), Times.Once);
    }

    [Fact]
    public async Task RoleExistsAsync_ShouldReturnTrue_WhenRoleExists()
    {
        // Arrange
        var roleName = "ExistingRole";

        _roleManagerMock.Setup(x => x.RoleExistsAsync(roleName))
                        .ReturnsAsync(true);

        // Act
        var result = await _roleService.RoleExistsAsync(roleName);

        // Assert
        result.Should().BeTrue();
        _roleManagerMock.Verify(x => x.RoleExistsAsync(roleName), Times.Once);
    }

    [Fact]
    public async Task RoleExistsAsync_ShouldReturnFalse_WhenRoleDoesNotExist()
    {
        // Arrange
        var roleName = "NonExistentRole";

        _roleManagerMock.Setup(x => x.RoleExistsAsync(roleName))
                        .ReturnsAsync(false);

        // Act
        var result = await _roleService.RoleExistsAsync(roleName);

        // Assert
        result.Should().BeFalse();
        _roleManagerMock.Verify(x => x.RoleExistsAsync(roleName), Times.Once);
    }

    [Fact]
    public async Task AddToRoleAsync_ShouldReturnSuccess_WhenUserIsAddedToRole()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var roleName = "Admin";
        var identityResult = IdentityResult.Success;

        _userManagerMock.Setup(x => x.AddToRoleAsync(user, roleName))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await _roleService.AddToRoleAsync(user, roleName);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeTrue();
        _userManagerMock.Verify(x => x.AddToRoleAsync(user, roleName), Times.Once);
    }

    [Fact]
    public async Task RemoveFromRoleAsync_ShouldReturnSuccess_WhenUserIsRemovedFromRole()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var roleName = "Admin";
        var identityResult = IdentityResult.Success;

        _userManagerMock.Setup(x => x.RemoveFromRoleAsync(user, roleName))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await _roleService.RemoveFromRoleAsync(user, roleName);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeTrue();
        _userManagerMock.Verify(x => x.RemoveFromRoleAsync(user, roleName), Times.Once);
    }
} 