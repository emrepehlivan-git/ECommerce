using Microsoft.AspNetCore.Identity;
 
namespace ECommerce.Application.UnitTests.Features.Users.V1.Commands;

public sealed class ActivateUserCommandTests : UserCommandsTestBase
{
    private readonly ActivateUserCommandHandler Handler;
    private ActivateUserCommand Command;

    public ActivateUserCommandTests()
    {
        Command = new ActivateUserCommand(UserId);
        Handler = new ActivateUserCommandHandler(
            UserServiceMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingInactiveUser_ShouldActivateUser()
    {
        var inactiveUser = User.Create("test@example.com", "Test User", "Password123!");
        inactiveUser.Deactivate();

        UserServiceMock
            .Setup(x => x.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(inactiveUser);

        UserServiceMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await Handler.Handle(Command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        inactiveUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithExistingActiveUser_ShouldReturnSuccess()
    {
        var activeUser = User.Create("test@example.com", "Test User", "Password123!");
        activeUser.Activate();

        UserServiceMock
            .Setup(x => x.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(activeUser);

        var result = await Handler.Handle(Command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        activeUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistingUser_ShouldReturnNotFound()
    {
        SetupUserExists(false);
        SetupLocalizedMessage(UserConsts.NotFound);

        var result = await Handler.Handle(Command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(LocalizerMock.Object[UserConsts.NotFound]);
    }

    [Fact]
    public async Task Handle_WhenUpdateFails_ShouldReturnError()
    {
        var inactiveUser = User.Create("test@example.com", "Test User", "Password123!");
        inactiveUser.Deactivate();

        UserServiceMock
            .Setup(x => x.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(inactiveUser);

            UserServiceMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed());

        var result = await Handler.Handle(Command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }
}