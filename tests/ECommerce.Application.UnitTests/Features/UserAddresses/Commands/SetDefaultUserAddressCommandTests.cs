using ECommerce.Application.Features.UserAddresses;
using ECommerce.Application.Features.UserAddresses.Commands;

namespace ECommerce.Application.UnitTests.Features.UserAddresses.Commands;

public sealed class SetDefaultUserAddressCommandTests : UserAddressesTestBase
{
    private readonly SetDefaultUserAddressCommandHandler Handler;
    private SetDefaultUserAddressCommand Command;

    public SetDefaultUserAddressCommandTests()
    {
        Command = new SetDefaultUserAddressCommand(AddressId, UserId);

        Handler = new SetDefaultUserAddressCommandHandler(
            UserAddressRepositoryMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSetAddressAsDefault()
    {
        // Arrange
        var existingAddress = DefaultUserAddress;
        SetupUserAddressExists(existingAddress);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        UserAddressRepositoryMock.Verify(
            x => x.SetDefaultAddressAsync(Command.UserId, Command.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentAddress_ShouldReturnNotFound()
    {
        // Arrange
        UserAddressRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Expression<Func<IQueryable<UserAddress>, IQueryable<UserAddress>>>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAddress?)null);
        SetupLocalizedMessage(UserAddressConsts.NotFound);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(UserAddressConsts.NotFound);
    }

    [Fact]
    public async Task Handle_WithDifferentUserId_ShouldReturnNotFound()
    {
        // Arrange
        var addressWithDifferentUser = UserAddress.Create(Guid.NewGuid(), "Home", DefaultAddress);
        SetupUserAddressExists(addressWithDifferentUser);
        SetupLocalizedMessage(UserAddressConsts.NotFound);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(UserAddressConsts.NotFound);
    }

    [Fact]
    public async Task Handle_WithInactiveAddress_ShouldReturnNotFound()
    {
        // Arrange
        var inactiveAddress = DefaultUserAddress;
        inactiveAddress.Deactivate();
        SetupUserAddressExists(inactiveAddress);
        SetupLocalizedMessage(UserAddressConsts.NotFound);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(UserAddressConsts.NotFound);
    }

    [Fact]
    public async Task Handle_WithAlreadyDefaultAddress_ShouldReturnError()
    {
        // Arrange
        var defaultAddress = DefaultUserAddress;
        defaultAddress.SetAsDefault();
        SetupUserAddressExists(defaultAddress);
        SetupLocalizedMessage(UserAddressConsts.AddressAlreadyDefault);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(UserAddressConsts.AddressAlreadyDefault);

        UserAddressRepositoryMock.Verify(
            x => x.SetDefaultAddressAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonDefaultAddress_ShouldCallSetDefaultAsync()
    {
        // Arrange
        var nonDefaultAddress = DefaultUserAddress;
        nonDefaultAddress.UnsetAsDefault(); // Ensure it's not default
        SetupUserAddressExists(nonDefaultAddress);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        UserAddressRepositoryMock.Verify(
            x => x.SetDefaultAddressAsync(Command.UserId, Command.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }
} 