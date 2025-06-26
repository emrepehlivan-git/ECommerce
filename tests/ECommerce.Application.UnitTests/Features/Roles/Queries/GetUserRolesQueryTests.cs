using ECommerce.Application.Behaviors;
using ECommerce.Application.Features.Roles.DTOs;
using ECommerce.Application.Features.Roles.Queries;
using ECommerce.Application.Features.Users;

namespace ECommerce.Application.UnitTests.Features.Roles.Queries;

public sealed class GetUserRolesQueryTests : RoleTestBase
{
    private readonly GetUserRolesQueryHandler _handler;
    private readonly GetUserRolesQuery _query;
    private readonly GetUserRolesQueryValidator _validator;
    private readonly Guid _userId;

    public GetUserRolesQueryTests()
    {
        _userId = Guid.NewGuid();
        _query = new GetUserRolesQuery(_userId);

        _handler = new GetUserRolesQueryHandler(
            RoleServiceMock.Object,
            UserServiceMock.Object,
            LazyServiceProviderMock.Object);

        _validator = new GetUserRolesQueryValidator(Localizer);
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
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UserId.Should().Be(user.Id);
        result.Value.UserName.Should().Be(user.UserName);
        result.Value.Roles.Should().HaveCount(2);
        result.Value.Roles.Should().Contain("Admin");
        result.Value.Roles.Should().Contain("User");

        UserServiceMock.Verify(x => x.FindByIdAsync(_userId), Times.Once);
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
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UserId.Should().Be(user.Id);
        result.Value.Roles.Should().BeEmpty();

        UserServiceMock.Verify(x => x.FindByIdAsync(_userId), Times.Once);
        RoleServiceMock.Verify(x => x.GetUserRolesAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnError()
    {
        // Arrange
        SetupUserServiceFindByIdAsync(null);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User not found.");

        UserServiceMock.Verify(x => x.FindByIdAsync(_userId), Times.Once);
    }

    [Fact]
    public void Query_ShouldImplementICacheableRequest()
    {
        // Arrange & Act & Assert
        _query.Should().BeAssignableTo<ICacheableRequest>();
        _query.CacheKey.Should().Be($"user-roles:{_userId}");
        _query.CacheDuration.Should().Be(TimeSpan.FromMinutes(15));
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
        var result = await _handler.Handle(_query, CancellationToken.None);

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
        var validationResult = await _validator.ValidateAsync(query);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == "User not found.");
    }

    [Fact]
    public async Task Validate_WithValidUserId_ShouldPassValidation()
    {
        // Arrange & Act
        var validationResult = await _validator.ValidateAsync(_query);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }
} 