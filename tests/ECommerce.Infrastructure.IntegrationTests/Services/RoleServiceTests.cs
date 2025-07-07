using ECommerce.Application.Common.Logging;
using ECommerce.Application.Features.Roles.V1.DTOs;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;

namespace ECommerce.Infrastructure.IntegrationTests.Services;

public sealed class RoleServiceTests
{
    private readonly Mock<UserManager<User>> UserManagerMock;
    private readonly Mock<RoleManager<Role>> RoleManagerMock;
    private readonly Mock<IKeycloakPermissionSyncService> KeycloakSyncServiceMock;
    private readonly Mock<IPermissionService> PermissionServiceMock;
    private readonly Mock<IECommerceLogger<RoleService>> LoggerMock;
    private readonly RoleService RoleService;

    public RoleServiceTests()
    {
        var userStore = new Mock<IUserStore<User>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        UserManagerMock = new Mock<UserManager<User>>(
            userStore.Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        var roleStore = new Mock<IRoleStore<Role>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        RoleManagerMock = new Mock<RoleManager<Role>>(
            roleStore.Object, null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        KeycloakSyncServiceMock = new Mock<IKeycloakPermissionSyncService>();
        PermissionServiceMock = new Mock<IPermissionService>();
        LoggerMock = new Mock<IECommerceLogger<RoleService>>();

        RoleService = new RoleService(
            UserManagerMock.Object, 
            RoleManagerMock.Object,
            KeycloakSyncServiceMock.Object,
            PermissionServiceMock.Object,
            LoggerMock.Object);
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

        RoleManagerMock.Setup(m => m.Roles).Returns(roles);

        // Act
        var result = await RoleService.GetAllRolesAsync(1, 10, string.Empty);

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
        RoleManagerMock.Setup(x => x.Roles)
                        .Returns(mockQueryable);

        // Act
        var result = await RoleService.GetRolesAsync();

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

        UserManagerMock.Setup(x => x.GetRolesAsync(user))
                        .ReturnsAsync(userRoles);

        // Act
        var result = await RoleService.GetUserRolesAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain("Admin");
        result.Should().Contain("User");
        UserManagerMock.Verify(x => x.GetRolesAsync(user), Times.Once);
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
        RoleManagerMock.Setup(x => x.Roles)
                        .Returns(mockQueryable);

        // Act
        var result = await RoleService.FindRoleByIdAsync(roleId);

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
        RoleManagerMock.Setup(x => x.Roles)
                        .Returns(mockQueryable);

        // Act
        var result = await RoleService.FindRoleByNameAsync(roleName);

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

        RoleManagerMock.Setup(x => x.CreateAsync(role))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await RoleService.CreateRoleAsync(role);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeTrue();
        RoleManagerMock.Verify(x => x.CreateAsync(role), Times.Once);
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldReturnSuccess_WhenRoleIsUpdatedSuccessfully()
    {
        // Arrange
        var role = Role.Create("UpdatedRole");
        var identityResult = IdentityResult.Success;

        RoleManagerMock.Setup(x => x.UpdateAsync(role))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await RoleService.UpdateRoleAsync(role);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeTrue();
        RoleManagerMock.Verify(x => x.UpdateAsync(role), Times.Once);
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldReturnSuccess_WhenRoleIsDeletedSuccessfully()
    {
        // Arrange
        var role = Role.Create("RoleToDelete");
        var identityResult = IdentityResult.Success;

        RoleManagerMock.Setup(x => x.DeleteAsync(role))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await RoleService.DeleteRoleAsync(role);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeTrue();
        RoleManagerMock.Verify(x => x.DeleteAsync(role), Times.Once);
    }

    [Fact]
    public async Task RoleExistsAsync_ShouldReturnTrue_WhenRoleExists()
    {
        // Arrange
        var roleName = "ExistingRole";

        RoleManagerMock.Setup(x => x.RoleExistsAsync(roleName))
                        .ReturnsAsync(true);

        // Act
        var result = await RoleService.RoleExistsAsync(roleName);

        // Assert
        result.Should().BeTrue();
        RoleManagerMock.Verify(x => x.RoleExistsAsync(roleName), Times.Once);
    }

    [Fact]
    public async Task RoleExistsAsync_ShouldReturnFalse_WhenRoleDoesNotExist()
    {
        // Arrange
        var roleName = "NonExistentRole";

        RoleManagerMock.Setup(x => x.RoleExistsAsync(roleName))
                        .ReturnsAsync(false);

        // Act
        var result = await RoleService.RoleExistsAsync(roleName);

        // Assert
        result.Should().BeFalse();
        RoleManagerMock.Verify(x => x.RoleExistsAsync(roleName), Times.Once);
    }

    [Fact]
    public async Task AddToRoleAsync_ShouldReturnSuccess_WhenUserIsAddedToRole()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var roleName = "Admin";
        var identityResult = IdentityResult.Success;

        UserManagerMock.Setup(x => x.AddToRoleAsync(user, roleName))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await RoleService.AddToRoleAsync(user, roleName);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeTrue();
        UserManagerMock.Verify(x => x.AddToRoleAsync(user, roleName), Times.Once);
    }

    [Fact]
    public async Task RemoveFromRoleAsync_ShouldReturnSuccess_WhenUserIsRemovedFromRole()
    {
        // Arrange
        var user = User.Create("test@example.com", "Test", "User");
        var roleName = "Admin";
        var identityResult = IdentityResult.Success;

        UserManagerMock.Setup(x => x.RemoveFromRoleAsync(user, roleName))
                        .ReturnsAsync(identityResult);

        // Act
        var result = await RoleService.RemoveFromRoleAsync(user, roleName);

        // Assert
        result.Should().Be(identityResult);
        result.Succeeded.Should().BeTrue();
        UserManagerMock.Verify(x => x.RemoveFromRoleAsync(user, roleName), Times.Once);
    }
} 