using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.UnitTests.Features.Roles.V1.Commands;

public sealed class DeleteRoleCommandTests : RoleTestBase
{
    private readonly DeleteRoleCommandHandler Handler;
    private readonly DeleteRoleCommand Command;
    private readonly DeleteRoleCommandValidator Validator;
    private readonly Guid RoleId;

    public DeleteRoleCommandTests()
    {
        RoleId = Guid.NewGuid();
        Command = new DeleteRoleCommand(RoleId);

        Handler = new DeleteRoleCommandHandler(
            RoleServiceMock.Object,
            CacheManagerMock.Object,
            LazyServiceProviderMock.Object);

        Validator = new DeleteRoleCommandValidator(
            RoleServiceMock.Object,
            LocalizerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteRole()
    {
        // Arrange
        var role = DefaultRole;
        SetupRoleServiceFindByIdAsync(role);
        SetupRoleServiceDeleteAsync(IdentityResult.Success);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        RoleServiceMock.Verify(x => x.FindRoleByIdAsync(RoleId), Times.Once);
        RoleServiceMock.Verify(x => x.DeleteRoleAsync(It.IsAny<Role>()), Times.Once);
        CacheManagerMock.Verify(x => x.RemoveAsync("roles:all:include-permissions:True", It.IsAny<CancellationToken>()), Times.Once);
        CacheManagerMock.Verify(x => x.RemoveAsync("roles:all:include-permissions:False", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithFailedIdentityResult_ShouldReturnError()
    {
        // Arrange
        var role = DefaultRole;
        var errors = new[] { new IdentityError { Description = "Role deletion failed" } };
        var identityResult = IdentityResult.Failed(errors);

        SetupRoleServiceFindByIdAsync(role);
        SetupRoleServiceDeleteAsync(identityResult);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Role deletion failed");
    }

    [Fact]
    public async Task Validate_WithNonExistentRole_ShouldReturnValidationError()
    {
        // Arrange
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
        SetupRoleServiceFindByIdAsync(DefaultRole);

        // Act
        var validationResult = await Validator.ValidateAsync(Command);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyId_ShouldReturnValidationError()
    {
        // Arrange
        var command = new DeleteRoleCommand(Guid.Empty);
        SetupRoleServiceFindByIdAsync(null);

        // Act
        var validationResult = await Validator.ValidateAsync(Command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == LocalizerMock.Object[RoleConsts.RoleNotFound]);
    }
} 