using ECommerce.Application.Features.Roles.Commands;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.UnitTests.Features.Roles.Commands;

public sealed class AddUserToRoleCommandTests : RoleTestBase
{
    private readonly AddUserToRoleCommandHandler _handler;
    private readonly AddUserToRoleCommand _command;
    private readonly AddUserToRoleCommandValidator _validator;
    private readonly Guid _userId;

    public AddUserToRoleCommandTests()
    {
        _userId = Guid.NewGuid();
        _command = new AddUserToRoleCommand(_userId, Guid.NewGuid());

        _handler = new AddUserToRoleCommandHandler(
            RoleServiceMock.Object,
            UserServiceMock.Object,
            CacheManagerMock.Object,
            LazyServiceProviderMock.Object);

        _validator = new AddUserToRoleCommandValidator(
            Localizer,
            UserServiceMock.Object,
            RoleServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldAddUserToRole()
    {
        // Arrange
        var user = DefaultUser;
        var role = DefaultRole;
        SetupUserServiceFindByIdAsync(user);
        SetupRoleServiceFindByIdAsync(role);
        SetupRoleServiceGetUserRolesAsync(new List<string>());
        SetupRoleServiceAddToRoleAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        UserServiceMock.Verify(x => x.FindByIdAsync(_userId), Times.Once);
        RoleServiceMock.Verify(x => x.FindRoleByIdAsync(_command.RoleId), Times.Once);
        RoleServiceMock.Verify(x => x.GetUserRolesAsync(user), Times.Once);
        RoleServiceMock.Verify(x => x.AddToRoleAsync(user, It.IsAny<string>()), Times.Once);
        CacheManagerMock.Verify(x => x.RemoveByPatternAsync($"user-roles:{_userId}:*", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUserAlreadyInRole_ShouldReturnError()
    {
        // Arrange
        var user = DefaultUser;
        var role = DefaultRole;
        SetupUserServiceFindByIdAsync(user);
        SetupRoleServiceFindByIdAsync(role);
        SetupRoleServiceGetUserRolesAsync(new List<string> { role.Name! });

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User already has this role.");
    }

    [Fact]
    public async Task Handle_WithFailedIdentityResult_ShouldReturnError()
    {
        // Arrange
        var user = DefaultUser;
        var role = DefaultRole;
        var errors = new[] { new IdentityError { Description = "Add to role failed" } };
        var identityResult = IdentityResult.Failed(errors);

        SetupUserServiceFindByIdAsync(user);
        SetupRoleServiceFindByIdAsync(role);
        SetupRoleServiceGetUserRolesAsync(new List<string>());
        SetupRoleServiceAddToRoleAsync(identityResult);

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Add to role failed");
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
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == "User not found.");
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
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == "Role not found.");
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

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Validate_WithInvalidRoleId_ShouldReturnValidationError(string roleIdString)
    {
        // Arrange
        var roleId = Guid.Parse(roleIdString);
        var command = _command with { RoleId = roleId };
        SetupUserServiceFindByIdAsync(DefaultUser);
        SetupRoleServiceFindByIdAsync(null);

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == "Role not found.");
    }
} 