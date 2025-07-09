using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.UnitTests.Features.Roles.V1.Commands;

public sealed class AddUserToRoleCommandTests : RoleTestBase
{
    private readonly AddUserToRoleCommandHandler Handler;
    private readonly AddUserToRoleCommand Command;
    private readonly AddUserToRoleCommandValidator Validator;
    private readonly Guid UserId;

    public AddUserToRoleCommandTests()
    {
        UserId = Guid.NewGuid();
        Command = new AddUserToRoleCommand(UserId, Guid.NewGuid());

        Handler = new AddUserToRoleCommandHandler(
            RoleServiceMock.Object,
            UserServiceMock.Object,
            CacheManagerMock.Object,
            LazyServiceProviderMock.Object);

        Validator = new AddUserToRoleCommandValidator(
            LocalizerMock.Object,
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
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        UserServiceMock.Verify(x => x.FindByIdAsync(UserId), Times.Once);
        RoleServiceMock.Verify(x => x.FindRoleByIdAsync(Command.RoleId), Times.Once);
        RoleServiceMock.Verify(x => x.GetUserRolesAsync(user), Times.Once);
        RoleServiceMock.Verify(x => x.AddToRoleAsync(user, It.IsAny<string>()), Times.Once);
        CacheManagerMock.Verify(x => x.RemoveByPatternAsync($"user-roles:{UserId}:*", It.IsAny<CancellationToken>()), Times.Once);
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
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(LocalizerMock.Object[RoleConsts.UserAlreadyInRole]);
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
        var result = await Handler.Handle(Command, CancellationToken.None);

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
        var validationResult = await Validator.ValidateAsync(Command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == LocalizerMock.Object[UserConsts.NotFound]);
    }

    [Fact]
    public async Task Validate_WithNonExistentRole_ShouldReturnValidationError()
    {
        // Arrange
        SetupUserServiceFindByIdAsync(DefaultUser);
        SetupRoleServiceFindByIdAsync(null);

        // Act
        var validationResult = await Validator.ValidateAsync(Command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == LocalizerMock.Object[RoleConsts.RoleNotFound]);
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPassValidation()
    {
        // Arrange
        SetupUserServiceFindByIdAsync(DefaultUser);
        SetupRoleServiceFindByIdAsync(DefaultRole);

        // Act
        var validationResult = await Validator.ValidateAsync(Command);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithInvalidRoleId_ShouldReturnValidationError()
    {
        // Arrange
        var command = Command with { RoleId = Guid.Empty };
        SetupUserServiceFindByIdAsync(DefaultUser);
        SetupRoleServiceFindByIdAsync(null);

        // Act
        var validationResult = await Validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == LocalizerMock.Object[RoleConsts.RoleNotFound]);
    }
} 