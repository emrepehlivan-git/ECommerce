using ECommerce.Application.Features.Roles;
using ECommerce.Application.Features.Roles.Commands;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.UnitTests.Features.Roles.Commands;

public sealed class DeleteRolesCommandTests : RoleTestBase
{
    private readonly DeleteRolesCommandHandler _handler;

    public DeleteRolesCommandTests()
    {
        _handler = new DeleteRolesCommandHandler(
            RoleServiceMock.Object,
            CacheManagerMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidIds_ShouldDeleteAllRoles()
    {
        // Arrange
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var roles = ids.Select(id => { var r = Role.Create($"Role_{id}"); r.Id = id; return r; }).ToList();
        RoleServiceMock.Setup(x => x.FindRolesByIdsAsync(ids, It.IsAny<CancellationToken>())).ReturnsAsync(roles);
        RoleServiceMock.Setup(x => x.DeleteRolesAsync(roles)).ReturnsAsync(IdentityResult.Success);

        var command = new DeleteRolesCommand(ids);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        RoleServiceMock.Verify(x => x.FindRolesByIdsAsync(ids, It.IsAny<CancellationToken>()), Times.Once);
        RoleServiceMock.Verify(x => x.DeleteRolesAsync(roles), Times.Once);
        CacheManagerMock.Verify(x => x.RemoveAsync("roles:all:include-permissions:True", It.IsAny<CancellationToken>()), Times.Once);
        CacheManagerMock.Verify(x => x.RemoveAsync("roles:all:include-permissions:False", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSomeNonExistentIds_ShouldReturnError()
    {
        // Arrange
        var validId = Guid.NewGuid();
        var invalidId = Guid.NewGuid();
        var role = Role.Create($"Role_{validId}");
        role.Id = validId;
        var roles = new List<Role> { role };
        var ids = new List<Guid> { validId, invalidId };
        RoleServiceMock.Setup(x => x.FindRolesByIdsAsync(ids, It.IsAny<CancellationToken>())).ReturnsAsync(roles);
        RoleServiceMock.Setup(x => x.DeleteRolesAsync(It.IsAny<List<Role>>())).ReturnsAsync(IdentityResult.Success);

        var command = new DeleteRolesCommand(ids);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(x => x.Contains("Role not found."));
    }

    [Fact]
    public async Task Handle_WithFailedIdentityResult_ShouldReturnError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var role = Role.Create($"Role_{id}");
        role.Id = id;
        var roles = new List<Role> { role };
        var ids = new List<Guid> { id };
        var errors = new[] { new IdentityError { Description = "Role deletion failed" } };
        var identityResult = IdentityResult.Failed(errors);
        RoleServiceMock.Setup(x => x.FindRolesByIdsAsync(ids, It.IsAny<CancellationToken>())).ReturnsAsync(roles);
        RoleServiceMock.Setup(x => x.DeleteRolesAsync(roles)).ReturnsAsync(identityResult);

        var command = new DeleteRolesCommand(ids);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Role deletion failed");
    }
} 