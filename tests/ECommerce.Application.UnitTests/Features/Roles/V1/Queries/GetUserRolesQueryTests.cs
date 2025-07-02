using ECommerce.Application.Behaviors;

namespace ECommerce.Application.UnitTests.Features.Roles.V1.Queries;

public sealed class GetUserRolesQueryTests : RoleTestBase
{
    private readonly GetUserRolesQueryHandler Handler;
    private readonly GetUserRolesQuery Query;
    private readonly GetUserRolesQueryValidator Validator;
    private readonly Guid UserId;

    public GetUserRolesQueryTests()
    {
        UserId = Guid.NewGuid();
        Query = new GetUserRolesQuery(UserId);

        Handler = new GetUserRolesQueryHandler(
            RoleServiceMock.Object,
            UserServiceMock.Object,
            LazyServiceProviderMock.Object);

        Validator = new GetUserRolesQueryValidator(Localizer);
    }

    [Fact]
    public async Task Handle_WithExistingUser_ShouldReturnUserRoles()
    {
        // Arrange
        var user = DefaultUser;
        var userRoles = new List<string> { "Admin", "User" };
        
        SetupUserServiceFindByIdAsync(user);
        SetupRoleServiceGetUserRolesAsync(userRoles);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UserId.Should().Be(user.Id);
        result.Value.UserName.Should().Be(user.UserName);
        result.Value.Roles.Should().HaveCount(2);
        result.Value.Roles.Should().Contain("Admin");
        result.Value.Roles.Should().Contain("User");

        UserServiceMock.Verify(x => x.FindByIdAsync(UserId), Times.Once);
        RoleServiceMock.Verify(x => x.GetUserRolesAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUserWithoutRoles_ShouldReturnEmptyRolesList()
    {
        // Arrange
        var user = DefaultUser;
        var userRoles = new List<string>();
        
        SetupUserServiceFindByIdAsync(user);
        SetupRoleServiceGetUserRolesAsync(userRoles);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UserId.Should().Be(user.Id);
        result.Value.Roles.Should().BeEmpty();

        UserServiceMock.Verify(x => x.FindByIdAsync(UserId), Times.Once);
        RoleServiceMock.Verify(x => x.GetUserRolesAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnError()
    {
        // Arrange
        SetupUserServiceFindByIdAsync(null);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(Localizer[RoleConsts.UserNotFound]);

        UserServiceMock.Verify(x => x.FindByIdAsync(UserId), Times.Once);
    }

    [Fact]
    public void Query_ShouldImplementICacheableRequest()
    {
        // Arrange & Act & Assert
        Query.Should().BeAssignableTo<ICacheableRequest>();
        Query.CacheKey.Should().Be($"user-roles:{UserId}");
        Query.CacheDuration.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public async Task Handle_ShouldMapToUserRoleDto()
    {
        // Arrange
        var user = DefaultUser;
        var userRoles = new List<string> { "Manager" };
        
        SetupUserServiceFindByIdAsync(user);
        SetupRoleServiceGetUserRolesAsync(userRoles);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<UserRoleDto>();
        result.Value.UserId.Should().Be(user.Id);
        result.Value.UserName.Should().Be(user.UserName);
        result.Value.Roles.Should().BeEquivalentTo(userRoles);
    }

    [Fact]
    public async Task Validate_WithEmptyUserId_ShouldReturnValidationError()
    {
        // Arrange
        var query = new GetUserRolesQuery(Guid.Empty);

        // Act
        var validationResult = await Validator.ValidateAsync(query);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == Localizer[RoleConsts.UserNotFound]);
    }

    [Fact]
    public async Task Validate_WithValidUserId_ShouldPassValidation()
    {
        // Arrange & Act
        var validationResult = await Validator.ValidateAsync(Query);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }
} 