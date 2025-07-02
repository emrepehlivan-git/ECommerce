namespace ECommerce.Application.UnitTests.Features.UserAddresses.V1.Commands;

public sealed class DeleteUserAddressCommandTests : UserAddressesTestBase
{
    private readonly DeleteUserAddressCommandHandler Handler;
    private DeleteUserAddressCommand Command;

    public DeleteUserAddressCommandTests()
    {
        Command = new DeleteUserAddressCommand(AddressId, UserId);

        Handler = new DeleteUserAddressCommandHandler(
            UserAddressRepositoryMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeactivateUserAddress()
    {
        // Arrange
        var existingAddress = DefaultUserAddress;
        existingAddress.UnsetAsDefault(); // Ensure it's not default
        SetupUserAddressExists(existingAddress);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        existingAddress.IsActive.Should().BeFalse();

        UserAddressRepositoryMock.Verify(
            x => x.Update(existingAddress),
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
    public async Task Handle_WithDefaultAddress_ShouldReturnError()
    {
        // Arrange
        var defaultAddress = DefaultUserAddress;
        defaultAddress.SetAsDefault();
        SetupUserAddressExists(defaultAddress);
        SetupLocalizedMessage(UserAddressConsts.DefaultAddressCannotBeDeleted);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(UserAddressConsts.DefaultAddressCannotBeDeleted);

        defaultAddress.IsActive.Should().BeTrue(); // Should remain active
        UserAddressRepositoryMock.Verify(
            x => x.Update(It.IsAny<UserAddress>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonDefaultActiveAddress_ShouldSucceed()
    {
        // Arrange
        var nonDefaultAddress = DefaultUserAddress;
        nonDefaultAddress.UnsetAsDefault();
        SetupUserAddressExists(nonDefaultAddress);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        nonDefaultAddress.IsActive.Should().BeFalse();
        UserAddressRepositoryMock.Verify(
            x => x.Update(nonDefaultAddress),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAddressIsActiveButNotDefault_ShouldDeactivateSuccessfully()
    {
        // Arrange
        var activeNonDefaultAddress = UserAddress.Create(UserId, "Work", DefaultAddress);
        activeNonDefaultAddress.Activate();
        activeNonDefaultAddress.UnsetAsDefault();
        SetupUserAddressExists(activeNonDefaultAddress);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        activeNonDefaultAddress.IsActive.Should().BeFalse();
        activeNonDefaultAddress.IsDefault.Should().BeFalse();
    }
} 