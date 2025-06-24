using ECommerce.Application.Features.UserAddresses;
using ECommerce.Application.Features.UserAddresses.Commands;

namespace ECommerce.Application.UnitTests.Features.UserAddresses.Commands;

public sealed class UpdateUserAddressCommandTests : UserAddressesTestBase
{
    private readonly UpdateUserAddressCommandHandler Handler;
    private UpdateUserAddressCommand Command;

    public UpdateUserAddressCommandTests()
    {
        Command = new UpdateUserAddressCommand(
            AddressId,
            UserId,
            "Updated Home",
            "456 Updated St",
            "Boston",
            "02101",
            "USA");

        Handler = new UpdateUserAddressCommandHandler(
            UserAddressRepositoryMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateUserAddress()
    {
        // Arrange
        var existingAddress = DefaultUserAddress;
        SetupUserAddressExists(existingAddress);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        existingAddress.Label.Should().Be(Command.Label);
        existingAddress.Address.Street.Should().Be(Command.Street);
        existingAddress.Address.City.Should().Be(Command.City);
        existingAddress.Address.ZipCode.Should().Be(Command.ZipCode);
        existingAddress.Address.Country.Should().Be(Command.Country);

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
    public async Task Handle_WithCompleteAddressUpdate_ShouldUpdateAllFields()
    {
        // Arrange
        var existingAddress = DefaultUserAddress;
        SetupUserAddressExists(existingAddress);

        var newLabel = "Work Office";
        var newStreet = "789 Business Blvd";
        var newCity = "Chicago";
        var newZipCode = "60601";
        var newCountry = "United States";

        var updateCommand = new UpdateUserAddressCommand(
            AddressId,
            UserId,
            newLabel,
            newStreet,
            newCity,
            newZipCode,
            newCountry);

        // Act
        var result = await Handler.Handle(updateCommand, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        existingAddress.Label.Should().Be(newLabel);
        existingAddress.Address.Street.Should().Be(newStreet);
        existingAddress.Address.City.Should().Be(newCity);
        existingAddress.Address.ZipCode.Should().Be(newZipCode);
        existingAddress.Address.Country.Should().Be(newCountry);
    }

    [Theory]
    [InlineData("Home")]
    [InlineData("Work")]
    [InlineData("Parents' House")]
    [InlineData("Vacation Home")]
    public async Task Handle_WithDifferentLabels_ShouldUpdateLabel(string newLabel)
    {
        // Arrange
        var existingAddress = DefaultUserAddress;
        SetupUserAddressExists(existingAddress);
        var commandWithNewLabel = Command with { Label = newLabel };

        // Act
        var result = await Handler.Handle(commandWithNewLabel, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        existingAddress.Label.Should().Be(newLabel);
    }
} 