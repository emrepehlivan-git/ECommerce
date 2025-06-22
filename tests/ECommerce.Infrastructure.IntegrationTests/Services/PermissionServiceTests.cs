using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Services;
using ECommerce.Persistence.Contexts;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Infrastructure.IntegrationTests.Services;

public class PermissionServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PermissionService _permissionService;
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<ILogger<PermissionService>> _loggerMock;

    public PermissionServiceTests()
    {
        // In-memory database için DbContext oluştur
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        
        // RoleManager mock'ı
        var roleStore = new Mock<IRoleStore<Role>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        _roleManagerMock = new Mock<RoleManager<Role>>(roleStore.Object, null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // UserManager mock'ı
        var userStore = new Mock<IUserStore<User>>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        _userManagerMock = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Logger mock'ı
        _loggerMock = new Mock<ILogger<PermissionService>>();
        
        _permissionService = new PermissionService(_context, _roleManagerMock.Object, _userManagerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task EnsureAdminHasAllPermissionsAsync_ShouldAddMissingPermissions_WhenAdminMissesPermissions()
    {
        // Arrange
        var adminRole = Role.Create("Admin");
        var permission1 = Permission.Create("Users.View", "View users", "Users", "View");
        var permission2 = Permission.Create("Users.Create", "Create users", "Users", "Create");
        var permission3 = Permission.Create("Products.View", "View products", "Products", "View");

        // Entity'leri database'e ekle ve ID'leri ata
        await _context.Roles.AddAsync(adminRole);
        await _context.Permissions.AddRangeAsync(permission1, permission2, permission3);
        await _context.SaveChangesAsync();

        // Admin role'ü sadece bir permission'a sahip
        var existingRolePermission = RolePermission.Create(adminRole.Id, permission1.Id);
        await _context.RolePermissions.AddAsync(existingRolePermission);
        await _context.SaveChangesAsync();

        _roleManagerMock.Setup(x => x.FindByNameAsync("ADMIN"))
            .ReturnsAsync(adminRole);

        // Act
        await _permissionService.EnsureAdminHasAllPermissionsAsync();

        // Assert
        var adminPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == adminRole.Id && rp.IsActive)
            .CountAsync();

        adminPermissions.Should().Be(3); // Tüm permission'lar eklenmiş olmalı
        
        // Logger'ın çağrıldığından emin ol
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Added 2 missing permissions")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task EnsureAdminHasAllPermissionsAsync_ShouldNotAddPermissions_WhenAdminHasAllPermissions()
    {
        // Arrange
        var adminRole = Role.Create("Admin");
        var permission1 = Permission.Create("Users.View", "View users", "Users", "View");
        var permission2 = Permission.Create("Users.Create", "Create users", "Users", "Create");

        await _context.Roles.AddAsync(adminRole);
        await _context.Permissions.AddRangeAsync(permission1, permission2);
        await _context.SaveChangesAsync();
        
        // Admin'e tüm permission'ları ekle
        var rolePermission1 = RolePermission.Create(adminRole.Id, permission1.Id);
        var rolePermission2 = RolePermission.Create(adminRole.Id, permission2.Id);
        await _context.RolePermissions.AddRangeAsync(rolePermission1, rolePermission2);
        await _context.SaveChangesAsync();

        _roleManagerMock.Setup(x => x.FindByNameAsync("ADMIN"))
            .ReturnsAsync(adminRole);

        // Act
        await _permissionService.EnsureAdminHasAllPermissionsAsync();

        // Assert
        var adminPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == adminRole.Id && rp.IsActive)
            .CountAsync();

        adminPermissions.Should().Be(2); // Değişmemiş olmalı
        
        // "Already has all permissions" log'unun çağrıldığından emin ol
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Admin already has all")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task EnsureAdminHasAllPermissionsAsync_ShouldLogWarning_WhenAdminRoleNotFound()
    {
        // Arrange
        _roleManagerMock.Setup(x => x.FindByNameAsync("ADMIN"))
            .ReturnsAsync((Role?)null);
        _roleManagerMock.Setup(x => x.FindByNameAsync("Admin"))
            .ReturnsAsync((Role?)null);

        // Act
        await _permissionService.EnsureAdminHasAllPermissionsAsync();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Admin role not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllPermissionConstantsAsync_ShouldReturnCachedPermissions()
    {
        // Act
        var result = await _permissionService.GetAllPermissionConstantsAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(p => p.PermissionName.Contains("Users."));
        result.Should().Contain(p => p.PermissionName.Contains("Products."));
        result.Should().Contain(p => p.PermissionName.Contains("Categories."));
        result.Should().Contain(p => p.PermissionName.Contains("Orders."));
        result.Should().Contain(p => p.PermissionName.Contains("Roles."));
        
        // Yeni eklenen Roles.Read'ın da bulunduğundan emin ol
        result.Should().Contain(p => p.PermissionName == "Roles.Read");
    }

    [Fact]
    public async Task AssignPermissionsToRoleAsync_ShouldAssignNewPermissions_WhenRoleExists()
    {
        // Arrange
        var managerRole = Role.Create("Manager");
        var permission1 = Permission.Create("Products.View", "View products", "Products", "View");
        var permission2 = Permission.Create("Products.Create", "Create products", "Products", "Create");
        var permission3 = Permission.Create("Products.Update", "Update products", "Products", "Update");

        await _context.Permissions.AddRangeAsync(permission1, permission2, permission3);
        await _context.SaveChangesAsync();

        // Manager role'ü database'e ekle
        await _context.Roles.AddAsync(managerRole);
        await _context.SaveChangesAsync();

        // Manager'a sadece bir permission ver
        var existingRolePermission = RolePermission.Create(managerRole.Id, permission1.Id);
        await _context.RolePermissions.AddAsync(existingRolePermission);
        await _context.SaveChangesAsync();

        _roleManagerMock.Setup(x => x.FindByNameAsync("Manager"))
            .ReturnsAsync(managerRole);

        var permissionsToAssign = new[] { "Products.View", "Products.Create", "Products.Update" };

        // Act
        await _permissionService.AssignPermissionsToRoleAsync("Manager", permissionsToAssign);

        // Assert
        var managerPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == managerRole.Id && rp.IsActive)
            .CountAsync();

        managerPermissions.Should().Be(3); // Tüm permission'lar eklenmiş olmalı
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Added 2 permissions to role Manager")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SyncPermissionsAsync_ShouldAddNewPermissions_AndEnsureAdminHasAll()
    {
        // Arrange
        var adminRole = Role.Create("Admin");
        await _context.Roles.AddAsync(adminRole);
        await _context.SaveChangesAsync();
        
        _roleManagerMock.Setup(x => x.FindByNameAsync("ADMIN"))
            .ReturnsAsync(adminRole);

        // Act
        await _permissionService.SyncPermissionsAsync();

        // Assert
        var allPermissions = await _context.Permissions.CountAsync();
        allPermissions.Should().BeGreaterThan(0);

        // Roles.Read'ın eklendiğinden emin ol
        var rolesReadPermission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == "Roles.Read");
        rolesReadPermission.Should().NotBeNull();

        // Admin'in tüm permission'lara sahip olduğundan emin ol
        var adminPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == adminRole.Id && rp.IsActive)
            .CountAsync();
        adminPermissions.Should().Be(allPermissions);
    }

    [Fact]
    public async Task HasPermissionAsync_ShouldReturnTrue_WhenUserHasPermission()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User");
        var adminRole = Role.Create("Admin");
        var permission = Permission.Create("Users.View", "View users", "Users", "View");

        await _context.Roles.AddAsync(adminRole);
        await _context.Permissions.AddAsync(permission);
        await _context.SaveChangesAsync();
        
        var rolePermission = RolePermission.Create(adminRole.Id, permission.Id);
        await _context.RolePermissions.AddAsync(rolePermission);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Admin" });

        // Act
        var result = await _permissionService.HasPermissionAsync(userId, "Users.View");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_ShouldReturnFalse_WhenUserDoesNotHavePermission()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User");
        var customerRole = Role.Create("Customer");

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Customer" });

        // Act
        var result = await _permissionService.HasPermissionAsync(userId, "Users.Delete");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ShouldReturnUserPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User");
        var managerRole = Role.Create("Manager");
        
        var permission1 = Permission.Create("Products.View", "View products", "Products", "View");
        var permission2 = Permission.Create("Products.Create", "Create products", "Products", "Create");
        
        await _context.Roles.AddAsync(managerRole);
        await _context.Permissions.AddRangeAsync(permission1, permission2);
        await _context.SaveChangesAsync();

        var rolePermission1 = RolePermission.Create(managerRole.Id, permission1.Id);
        var rolePermission2 = RolePermission.Create(managerRole.Id, permission2.Id);
        await _context.RolePermissions.AddRangeAsync(rolePermission1, rolePermission2);
        await _context.SaveChangesAsync();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Manager" });

        // Act
        var result = await _permissionService.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("Products.View");
        result.Should().Contain("Products.Create");
    }

    [Fact]
    public async Task AssignPermissionToRoleAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        var role = Role.Create("TestRole");
        var permission = Permission.Create("Test.Permission", "Test permission", "Test", "Permission");

        await _context.Roles.AddAsync(role);
        await _context.Permissions.AddAsync(permission);
        await _context.SaveChangesAsync();

        _roleManagerMock.Setup(x => x.FindByIdAsync(role.Id.ToString()))
            .ReturnsAsync(role);

        // Act
        var result = await _permissionService.AssignPermissionToRoleAsync(role.Id, "Test.Permission");

        // Assert
        result.Should().BeTrue();
        
        var rolePermissionExists = await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id && rp.IsActive);
        rolePermissionExists.Should().BeTrue();
    }

    [Fact]
    public async Task RemovePermissionFromRoleAsync_ShouldDeactivatePermission_WhenExists()
    {
        // Arrange
        var role = Role.Create("TestRole");
        var permission = Permission.Create("Test.Permission", "Test permission", "Test", "Permission");

        await _context.Roles.AddAsync(role);
        await _context.Permissions.AddAsync(permission);
        await _context.SaveChangesAsync();

        var rolePermission = RolePermission.Create(role.Id, permission.Id);
        await _context.RolePermissions.AddAsync(rolePermission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionService.RemovePermissionFromRoleAsync(role.Id, "Test.Permission");

        // Assert
        result.Should().BeTrue();
        
        var updatedRolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);
        updatedRolePermission.Should().NotBeNull();
        updatedRolePermission!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetRolePermissionsAsync_ShouldReturnActivePermissions()
    {
        // Arrange
        var role = Role.Create("TestRole");
        
        var permission1 = Permission.Create("Test.Permission1", "Test permission 1", "Test", "Permission");
        var permission2 = Permission.Create("Test.Permission2", "Test permission 2", "Test", "Permission");
        
        await _context.Roles.AddAsync(role);
        await _context.Permissions.AddRangeAsync(permission1, permission2);
        await _context.SaveChangesAsync();

        var rolePermission1 = RolePermission.Create(role.Id, permission1.Id);
        var rolePermission2 = RolePermission.Create(role.Id, permission2.Id);
        
        // İkinci permission'ı deactivate et
        rolePermission2.Deactivate();

        await _context.RolePermissions.AddRangeAsync(rolePermission1, rolePermission2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _permissionService.GetRolePermissionsAsync(role.Id);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain("Test.Permission1");
        result.Should().NotContain("Test.Permission2"); // Deactive permission dahil edilmemeli
    }

    public void Dispose()
    {
        _context.Dispose();
    }
} 