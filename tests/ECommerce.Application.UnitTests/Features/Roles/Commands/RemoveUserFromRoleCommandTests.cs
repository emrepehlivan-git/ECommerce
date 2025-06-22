using ECommerce.Application.Features.Roles;
using ECommerce.Application.Features.Roles.Commands;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.UnitTests.Features.Roles.Commands;

public sealed class RemoveUserFromRoleCommandTests : RoleTestBase
{
    private readonly RemoveUserFromRoleCommandHandler _handler;
    private readonly RemoveUserFromRoleCommand _command;
    private readonly RemoveUserFromRoleCommandValidator _validator;
    private readonly Guid _userId;

    public RemoveUserFromRoleCommandTests()
    {
        _userId = Guid.NewGuid();
        _command = new RemoveUserFromRoleCommand(_userId, "TestRole");

        _handler = new RemoveUserFromRoleCommandHandler(
            RoleServiceMock.Object,
            UserServiceMock.Object,
            CacheManagerMock.Object,
            LazyServiceProviderMock.Object);

        _validator = new RemoveUserFromRoleCommandValidator(
            Localizer,
            UserServiceMock.Object,
            RoleServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRemoveUserFromRole()
    {
        // Arrange
        var user = DefaultUser;
        SetupUserServiceFindByIdAsync(user);
        SetupRoleServiceRoleExistsAsync(true);
        SetupRoleServiceGetUserRolesAsync(new List<string> { "TestRole" });
        SetupRoleServiceRemoveFromRoleAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        UserServiceMock.Verify(x => x.FindByIdAsync(_userId), Times.Once);
        RoleServiceMock.Verify(x => x.GetUserRolesAsync(user), Times.Once);
        RoleServiceMock.Verify(x => x.RemoveFromRoleAsync(user, "TestRole"), Times.Once);
        CacheManagerMock.Verify(x => x.RemoveByPatternAsync($"user-roles:{_userId}:*", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUserNotInRole_ShouldReturnError()
    {
        // Arrange
        var user = DefaultUser;
        SetupUserServiceFindByIdAsync(user);
        SetupRoleServiceRoleExistsAsync(true);
        SetupRoleServiceGetUserRolesAsync(new List<string>()); // User not in role

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User does not have this role.");
    }

    [Fact]
    public async Task Handle_WithFailedIdentityResult_ShouldReturnError()
    {
        // Arrange
        var user = DefaultUser;
        var errors = new[] { new IdentityError { Description = "Remove from role failed" } };
        var identityResult = IdentityResult.Failed(errors);

        SetupUserServiceFindByIdAsync(user);
        SetupRoleServiceRoleExistsAsync(true);
        SetupRoleServiceGetUserRolesAsync(new List<string> { "TestRole" });
        SetupRoleServiceRemoveFromRoleAsync(identityResult);

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Remove from role failed");
    }

    [Fact]
    public async Task Validate_WithNonExistentUser_ShouldReturnValidationError()
    {
        // Arrange
        SetupUserServiceFindByIdAsync(null);
        SetupRoleServiceRoleExistsAsync(true);

        // Act
        var validationResult = await _validator.ValidateAsync(_command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == "UserId is required");
    }

    [Fact]
    public async Task Validate_WithNonExistentRole_ShouldReturnValidationError()
    {
        // Arrange
        SetupUserServiceFindByIdAsync(DefaultUser);
        SetupRoleServiceRoleExistsAsync(false);

        // Act
        var validationResult = await _validator.ValidateAsync(_command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == "Role name is required.");
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPassValidation()
    {
        // Arrange
        SetupUserServiceFindByIdAsync(DefaultUser);
        SetupRoleServiceRoleExistsAsync(true);

        // Act
        var validationResult = await _validator.ValidateAsync(_command);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WithInvalidRoleName_ShouldReturnValidationError(string roleName)
    {
        // Arrange
        var command = _command with { RoleName = roleName };
        SetupUserServiceFindByIdAsync(DefaultUser);
        SetupRoleServiceRoleExistsAsync(false);

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == "Role name is required.");
    }
} 