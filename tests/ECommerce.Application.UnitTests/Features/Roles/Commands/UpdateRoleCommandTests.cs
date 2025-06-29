using ECommerce.Application.Features.Roles;
using ECommerce.Application.Features.Roles.Commands;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.UnitTests.Features.Roles.Commands;

public sealed class UpdateRoleCommandTests : RoleTestBase
{
    private readonly UpdateRoleCommandHandler _handler;
    private readonly UpdateRoleCommand _command;
    private readonly UpdateRoleCommandValidator _validator;
    private readonly Guid _roleId;

    public UpdateRoleCommandTests()
    {
        _roleId = Guid.NewGuid();
        _command = new UpdateRoleCommand(_roleId, "UpdatedRole");

        _handler = new UpdateRoleCommandHandler(
            RoleServiceMock.Object,
            CacheManagerMock.Object,
            LazyServiceProviderMock.Object);

        _validator = new UpdateRoleCommandValidator(
            LocalizationServiceMock.Object,
            RoleServiceMock.Object
            );
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateRole()
    {
        // Arrange
        var role = DefaultRole;
        SetupRoleServiceFindByIdAsync(role);
        SetupRoleServiceUpdateAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        RoleServiceMock.Verify(x => x.FindRoleByIdAsync(_roleId), Times.Once);
        RoleServiceMock.Verify(x => x.UpdateRoleAsync(It.IsAny<Role>()), Times.Once);
        CacheManagerMock.Verify(x => x.RemoveByPatternAsync("roles:all:include-permissions:*", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithFailedIdentityResult_ShouldReturnError()
    {
        // Arrange
        var role = DefaultRole;
        var errors = new[] { new IdentityError { Description = "Role update failed" } };
        var identityResult = IdentityResult.Failed(errors);

        SetupRoleServiceFindByIdAsync(role);
        SetupRoleServiceUpdateAsync(identityResult);

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Role update failed");
    }

    [Fact]
    public async Task Validate_WithNonExistentRole_ShouldReturnValidationError()
    {
        // Arrange
        SetupRoleServiceFindByIdAsync(null);
        SetupRoleServiceFindByNameAsync(null);

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
        SetupRoleServiceFindByIdAsync(DefaultRole);
        SetupRoleServiceFindByNameAsync(null); // No existing role with this name

        // Act
        var validationResult = await _validator.ValidateAsync(_command);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithExistingRoleName_ShouldReturnValidationError()
    {
        // Arrange
        var existingRole = Role.Create("ExistingRole");
        SetupRoleServiceFindByIdAsync(DefaultRole);
        SetupRoleServiceFindByNameAsync(existingRole);

        // Act
        var validationResult = await _validator.ValidateAsync(_command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == Localizer[RoleConsts.NameExists]);
    }

    [Fact]
    public async Task Validate_WithSameRoleNameForSameRole_ShouldPassValidation()
    {
        // Arrange
        var role = DefaultRole;
        var command = new UpdateRoleCommand(role.Id, role.Name!);
        
        SetupRoleServiceFindByIdAsync(role);
        SetupRoleServiceFindByNameAsync(role);

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var command = _command with { Name = "" };
        SetupRoleServiceFindByIdAsync(DefaultRole);

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == Localizer[RoleConsts.NameIsRequired]);
    }

    [Fact]
    public async Task Validate_WithShortName_ShouldReturnValidationError()
    {
        // Arrange
        var command = _command with { Name = "A" };
        SetupRoleServiceFindByIdAsync(DefaultRole);

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == Localizer[RoleConsts.NameMustBeAtLeastCharacters, RoleConsts.NameMinLength.ToString()]);
    }
} 