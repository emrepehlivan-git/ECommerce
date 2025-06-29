using ECommerce.Application.Features.Roles;
using ECommerce.Application.Features.Roles.Commands;
using ECommerce.Application.Features.Users;
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
        _command = new RemoveUserFromRoleCommand(_userId, Guid.NewGuid());

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
        var role = DefaultRole;
        SetupUserServiceFindByIdAsync(user);
        SetupRoleServiceFindByIdAsync(role);
        SetupRoleServiceGetUserRolesAsync(new List<string> { role.Name! });
        SetupRoleServiceRemoveFromRoleAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        UserServiceMock.Verify(x => x.FindByIdAsync(_userId), Times.Once);
        RoleServiceMock.Verify(x => x.FindRoleByIdAsync(_command.RoleId), Times.Once);
        RoleServiceMock.Verify(x => x.GetUserRolesAsync(user), Times.Once);
        RoleServiceMock.Verify(x => x.RemoveFromRoleAsync(user, It.IsAny<string>()), Times.Once);
        CacheManagerMock.Verify(x => x.RemoveByPatternAsync($"user-roles:{_userId}:*", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUserNotInRole_ShouldReturnError()
    {
        // Arrange
        var user = DefaultUser;
        var role = DefaultRole;
        SetupUserServiceFindByIdAsync(user);
        SetupRoleServiceFindByIdAsync(role);
        SetupRoleServiceGetUserRolesAsync(new List<string>()); // User not in role

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(Localizer[RoleConsts.UserNotInRole]);
    }

    [Fact]
    public async Task Handle_WithFailedIdentityResult_ShouldReturnError()
    {
        // Arrange
        var user = DefaultUser;
        var role = DefaultRole;
        var errors = new[] { new IdentityError { Description = "Remove from role failed" } };
        var identityResult = IdentityResult.Failed(errors);

        SetupUserServiceFindByIdAsync(user);
        SetupRoleServiceFindByIdAsync(role);
        SetupRoleServiceGetUserRolesAsync(new List<string> { role.Name! });
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
        SetupRoleServiceFindByIdAsync(DefaultRole);

        // Act
        var validationResult = await _validator.ValidateAsync(_command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == Localizer[UserConsts.NotFound]);
    }

    [Fact]
    public async Task Validate_WithNonExistentRole_ShouldReturnValidationError()
    {
        // Arrange
        SetupUserServiceFindByIdAsync(DefaultUser);
        SetupRoleServiceFindByIdAsync(null);

        // Act
        var validationResult = await _validator.ValidateAsync(_command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == Localizer[RoleConsts.RoleNotFound]);
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPassValidation()
    {
        // Arrange
        SetupUserServiceFindByIdAsync(DefaultUser);
        SetupRoleServiceFindByIdAsync(DefaultRole);

        // Act
        var validationResult = await _validator.ValidateAsync(_command);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithInvalidRoleId_ShouldReturnValidationError()
    {
        // Arrange
        var command = _command with { RoleId = Guid.Empty };
        SetupUserServiceFindByIdAsync(DefaultUser);
        SetupRoleServiceFindByIdAsync(null);

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == Localizer[RoleConsts.RoleNotFound]);
    }
} 