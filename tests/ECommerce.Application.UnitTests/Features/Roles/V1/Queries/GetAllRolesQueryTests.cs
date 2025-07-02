using ECommerce.Application.Behaviors;

namespace ECommerce.Application.UnitTests.Features.Roles.V1.Queries;

public sealed class GetAllRolesQueryTests : RoleTestBase
{
    private readonly GetAllRolesQueryHandler Handler;
    private readonly GetAllRolesQuery Query;
    private readonly List<Role> Roles;
    private readonly PageableRequestParams PageableRequestParams;

    public GetAllRolesQueryTests()
    {
        Roles =
        [
            Role.Create("Admin"),
            Role.Create("User"),
            Role.Create("Manager")
        ];

        PageableRequestParams = new PageableRequestParams(1, 10);
        Query = new GetAllRolesQuery(PageableRequestParams, false);
        Handler = new GetAllRolesQueryHandler(
            RoleServiceMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnRoles()
    {
        // Arrange
        SetupRoleServiceGetAllRolesAsync(Roles);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.Should().Contain(r => r.Name == "Admin");
        result.Value.Should().Contain(r => r.Name == "User");
        result.Value.Should().Contain(r => r.Name == "Manager");

        RoleServiceMock.Verify(x => x.GetAllRolesAsync(PageableRequestParams.Page, PageableRequestParams.PageSize, PageableRequestParams.Search ?? string.Empty, false), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyRolesList_ShouldReturnEmptyList()
    {
        // Arrange
        SetupRoleServiceGetAllRolesAsync(new List<Role>());

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        RoleServiceMock.Verify(x => x.GetAllRolesAsync(PageableRequestParams.Page, PageableRequestParams.PageSize, PageableRequestParams.Search ?? string.Empty, false), Times.Once);
    }

    [Fact]
    public void CacheKey_WithIncludePermissionsFalse_ShouldBeCorrect()
    {
        // Arrange
        var query = new GetAllRolesQuery(PageableRequestParams, false);

        // Act & Assert
        query.Should().BeAssignableTo<ICacheableRequest>();
        query.CacheKey.Should().Be("roles:all:include-permissions:False");
        query.CacheDuration.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public void CacheKey_WithIncludePermissionsTrue_ShouldBeCorrect()
    {
        // Arrange
        var query = new GetAllRolesQuery(PageableRequestParams, true);

        // Act & Assert
        query.Should().BeAssignableTo<ICacheableRequest>();
        query.CacheKey.Should().Be("roles:all:include-permissions:True");
        query.CacheDuration.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public async Task Handle_ShouldMapRolesToDtos()
    {
        // Arrange
        var roleWithPermissions = Role.Create("AdminWithPermissions");
        var rolesWithPermissions = new List<Role> { roleWithPermissions };
        SetupRoleServiceGetAllRolesAsync(rolesWithPermissions);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Should().BeOfType<RoleDto>();
        result.Value.First().Name.Should().Be("AdminWithPermissions");
    }
} 