
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.UnitTests.Features.Roles.V1.Commands;

public sealed class CreateRoleCommandTests : RoleTestBase
{
    private readonly CreateRoleCommandHandler Handler;
    private readonly CreateRoleCommand Command;
    private readonly CreateRoleCommandValidator Validator;

    public CreateRoleCommandTests()
    {
        Command = new CreateRoleCommand("TestRole");

        Handler = new CreateRoleCommandHandler(
            RoleServiceMock.Object,
            CacheManagerMock.Object,
            LazyServiceProviderMock.Object);

        Validator = new CreateRoleCommandValidator(
            RoleServiceMock.Object,
            LocalizerMock.Object
            );
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateRole()
    {
        // Arrange
        SetupRoleServiceCreateAsync(IdentityResult.Success);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        RoleServiceMock.Verify(x => x.CreateRoleAsync(It.IsAny<Role>()), Times.Once);
        CacheManagerMock.Verify(x => x.RemoveByPatternAsync("roles:*", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithFailedIdentityResult_ShouldReturnError()
    {
        // Arrange
        var errors = new[] { new IdentityError { Description = "Role creation failed" } };
        var identityResult = IdentityResult.Failed(errors);
        SetupRoleServiceCreateAsync(identityResult);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Role creation failed");
    }

    [Fact]
    public async Task Validate_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var command = Command with { Name = "" };
        SetupRoleServiceRoleExistsAsync(false);

        // Act
        var validationResult = await Validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == LocalizerMock.Object[RoleConsts.NameIsRequired]);
    }

    [Fact]
    public async Task Validate_WithShortName_ShouldReturnValidationError()
    {
        // Arrange
        var command = Command with { Name = "A" };
        SetupRoleServiceRoleExistsAsync(false);

        // Act
        var validationResult = await Validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == LocalizerMock.Object[RoleConsts.NameMustBeAtLeastCharacters, RoleConsts.NameMinLength.ToString()]);
    }

    [Fact]
    public async Task Validate_WithExistingName_ShouldReturnValidationError()
    {
        // Arrange
        SetupRoleServiceRoleExistsAsync(true);

        // Act
        var validationResult = await Validator.ValidateAsync(Command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == LocalizerMock.Object[RoleConsts.NameExists]);
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPassValidation()
    {
        // Arrange
        SetupRoleServiceRoleExistsAsync(false);

        // Act
        var validationResult = await Validator.ValidateAsync(Command);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithLongName_ShouldReturnValidationError()
    {
        // Arrange
        var longName = new string('A', RoleConsts.NameMaxLength + 1);
        var command = Command with { Name = longName };
        SetupRoleServiceRoleExistsAsync(false);

        // Act
        var validationResult = await Validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == LocalizerMock.Object[RoleConsts.NameMustBeLessThanCharacters, RoleConsts.NameMaxLength.ToString()]);
    }
} 