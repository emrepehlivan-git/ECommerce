namespace ECommerce.Domain.UnitTests.Entities;

public sealed class RolePermissionTests
{
    private readonly Guid _roleId = new Guid("123e4567-e89b-12d3-a456-426614174000");
    private readonly Guid _permissionId = new Guid("987fcdeb-51a2-43d1-9f12-345678901234");

    [Fact]
    public void Create_WithRoleIdAndPermissionId_ShouldCreateRolePermission()
    {
        // Act
        var rolePermission = RolePermission.Create(_roleId, _permissionId);

        // Assert
        rolePermission.Should().NotBeNull();
        rolePermission.RoleId.Should().Be(_roleId);
        rolePermission.PermissionId.Should().Be(_permissionId);
        rolePermission.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithRoleAndPermission_ShouldCreateRolePermission()
    {
        // Arrange
        var role = Role.Create("TestRole");
        var permission = Permission.Create("TestPermission", "Test Permission Description", "TestModule", "TestAction");

        // Act
        var rolePermission = RolePermission.Create(role, permission);

        // Assert
        rolePermission.Should().NotBeNull();
        rolePermission.RoleId.Should().Be(role.Id);
        rolePermission.PermissionId.Should().Be(permission.Id);
        rolePermission.Role.Should().Be(role);
        rolePermission.Permission.Should().Be(permission);
        rolePermission.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldInheritFromBaseEntity()
    {
        // Act
        var rolePermission = RolePermission.Create(_roleId, _permissionId);

        // Assert
        rolePermission.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var rolePermission = RolePermission.Create(_roleId, _permissionId);

        // Act
        rolePermission.Deactivate();

        // Assert
        rolePermission.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var rolePermission = RolePermission.Create(_roleId, _permissionId);
        rolePermission.Deactivate();

        // Act
        rolePermission.Activate();

        // Assert
        rolePermission.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldRemainActive()
    {
        // Arrange
        var rolePermission = RolePermission.Create(_roleId, _permissionId);

        // Act
        rolePermission.Activate();

        // Assert
        rolePermission.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldRemainInactive()
    {
        // Arrange
        var rolePermission = RolePermission.Create(_roleId, _permissionId);
        rolePermission.Deactivate();

        // Act
        rolePermission.Deactivate();

        // Assert
        rolePermission.IsActive.Should().BeFalse();
    }

    [Fact]
    public void RoleId_ShouldBeReadOnly()
    {
        // Arrange
        var rolePermission = RolePermission.Create(_roleId, _permissionId);

        // Assert
        rolePermission.RoleId.Should().Be(_roleId);
        // RoleId should not have a public setter
        typeof(RolePermission).GetProperty(nameof(RolePermission.RoleId))?.SetMethod?.IsPublic.Should().BeFalse();
    }

    [Fact]
    public void PermissionId_ShouldBeReadOnly()
    {
        // Arrange
        var rolePermission = RolePermission.Create(_roleId, _permissionId);

        // Assert
        rolePermission.PermissionId.Should().Be(_permissionId);
        // PermissionId should not have a public setter
        typeof(RolePermission).GetProperty(nameof(RolePermission.PermissionId))?.SetMethod?.IsPublic.Should().BeFalse();
    }

    [Fact]
    public void IsActive_ShouldBeReadOnly()
    {
        // Arrange
        var rolePermission = RolePermission.Create(_roleId, _permissionId);

        // Assert
        rolePermission.IsActive.Should().BeTrue();
        // IsActive should not have a public setter
        typeof(RolePermission).GetProperty(nameof(RolePermission.IsActive))?.SetMethod?.IsPublic.Should().BeFalse();
    }

    [Fact]
    public void ActivateDeactivateCycle_ShouldWorkCorrectly()
    {
        // Arrange
        var rolePermission = RolePermission.Create(_roleId, _permissionId);

        // Act & Assert
        rolePermission.IsActive.Should().BeTrue();

        rolePermission.Deactivate();
        rolePermission.IsActive.Should().BeFalse();

        rolePermission.Activate();
        rolePermission.IsActive.Should().BeTrue();

        rolePermission.Deactivate();
        rolePermission.IsActive.Should().BeFalse();
    }
} 