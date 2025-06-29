using ECommerce.Application.Features.Roles;
using ECommerce.Application.Features.Roles.Commands;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.UnitTests.Features.Roles.Commands;

public sealed class CreateRoleCommandTests : RoleTestBase
{
    private readonly CreateRoleCommandHandler _handler;
    private readonly CreateRoleCommand _command;
    private readonly CreateRoleCommandValidator _validator;

    public CreateRoleCommandTests()
    {
        _command = new CreateRoleCommand("TestRole");

        _handler = new CreateRoleCommandHandler(
            RoleServiceMock.Object,
            CacheManagerMock.Object,
            LazyServiceProviderMock.Object);

        _validator = new CreateRoleCommandValidator(
            RoleServiceMock.Object,
            LocalizationServiceMock.Object
            );
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateRole()
    {
        // Arrange
        SetupRoleServiceCreateAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        RoleServiceMock.Verify(x => x.CreateRoleAsync(It.IsAny<Role>()), Times.Once);
        CacheManagerMock.Verify(x => x.RemoveByPatternAsync("roles:all:include-permissions:*", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithFailedIdentityResult_ShouldReturnError()
    {
        // Arrange
        var errors = new[] { new IdentityError { Description = "Role creation failed" } };
        var identityResult = IdentityResult.Failed(errors);
        SetupRoleServiceCreateAsync(identityResult);

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Role creation failed");
    }

    [Fact]
    public async Task Validate_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var command = _command with { Name = "" };
        SetupRoleServiceRoleExistsAsync(false);

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
        SetupRoleServiceRoleExistsAsync(false);

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == Localizer[RoleConsts.NameMustBeAtLeastCharacters, RoleConsts.NameMinLength.ToString()]);
    }

    [Fact]
    public async Task Validate_WithExistingName_ShouldReturnValidationError()
    {
        // Arrange
        SetupRoleServiceRoleExistsAsync(true);

        // Act
        var validationResult = await _validator.ValidateAsync(_command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == Localizer[RoleConsts.NameExists]);
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPassValidation()
    {
        // Arrange
        SetupRoleServiceRoleExistsAsync(false);

        // Act
        var validationResult = await _validator.ValidateAsync(_command);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithLongName_ShouldReturnValidationError()
    {
        // Arrange
        var longName = new string('A', RoleConsts.NameMaxLength + 1);
        var command = _command with { Name = longName };
        SetupRoleServiceRoleExistsAsync(false);

        // Act
        var validationResult = await _validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == Localizer[RoleConsts.NameMustBeLessThanCharacters, RoleConsts.NameMaxLength.ToString()]);
    }
} 