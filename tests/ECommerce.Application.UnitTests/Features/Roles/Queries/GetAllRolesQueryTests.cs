using ECommerce.Application.Behaviors;
using ECommerce.Application.Features.Roles.DTOs;
using ECommerce.Application.Features.Roles.Queries;

namespace ECommerce.Application.UnitTests.Features.Roles.Queries;

public sealed class GetAllRolesQueryTests : RoleTestBase
{
    private readonly GetAllRolesQueryHandler _handler;
    private readonly GetAllRolesQuery _query;
    private readonly List<Role> _roles;

    public GetAllRolesQueryTests()
    {
        _roles = new List<Role>
        {
            Role.Create("Admin"),
            Role.Create("User"),
            Role.Create("Manager")
        };

        _query = new GetAllRolesQuery();
        _handler = new GetAllRolesQueryHandler(
            RoleServiceMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnRoles()
    {
        // Arrange
        SetupRoleServiceGetAllRolesAsync(_roles);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.Should().Contain(r => r.Name == "Admin");
        result.Value.Should().Contain(r => r.Name == "User");
        result.Value.Should().Contain(r => r.Name == "Manager");

        RoleServiceMock.Verify(x => x.GetAllRolesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyRolesList_ShouldReturnEmptyList()
    {
        // Arrange
        SetupRoleServiceGetAllRolesAsync(new List<Role>());

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        RoleServiceMock.Verify(x => x.GetAllRolesAsync(), Times.Once);
    }

    [Fact]
    public void Query_ShouldImplementICacheableRequest()
    {
        // Arrange & Act & Assert
        _query.Should().BeAssignableTo<ICacheableRequest>();
        _query.CacheKey.Should().Be("roles:all");
        _query.CacheDuration.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public async Task Handle_ShouldMapRolesToDtos()
    {
        // Arrange
        var roleWithPermissions = Role.Create("AdminWithPermissions");
        var rolesWithPermissions = new List<Role> { roleWithPermissions };
        SetupRoleServiceGetAllRolesAsync(rolesWithPermissions);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Should().BeOfType<RoleDto>();
        result.Value.First().Name.Should().Be("AdminWithPermissions");
    }
} 