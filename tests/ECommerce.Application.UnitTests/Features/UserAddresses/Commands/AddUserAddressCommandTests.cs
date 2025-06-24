using ECommerce.Application.Features.UserAddresses;
using ECommerce.Application.Features.UserAddresses.Commands;

namespace ECommerce.Application.UnitTests.Features.UserAddresses.Commands;

public sealed class AddUserAddressCommandTests : UserAddressesTestBase
{
    private readonly AddUserAddressCommandHandler Handler;
    private AddUserAddressCommand Command;

    public AddUserAddressCommandTests()
    {
        Command = new AddUserAddressCommand(
            UserId,
            "Home",
            "123 Main St",
            "New York",
            "10001",
            "USA");

        Handler = new AddUserAddressCommandHandler(
            UserAddressRepositoryMock.Object,
            UserServiceMock.Object,
            LazyServiceProviderMock.Object);
    }   

    [Fact]
    public async Task Handle_WithValidCommand_ShouldAddUserAddress()
    {
        // Arrange
        SetupUserExists(true);
        SetupHasDefaultAddress(true);

        var savedAddressId = Guid.NewGuid();
        UserAddress? capturedAddress = null;
        UserAddressRepositoryMock
            .Setup(x => x.Add(It.IsAny<UserAddress>()))
            .Callback<UserAddress>(address => 
            {
                capturedAddress = address;
                // Simulate Entity Framework setting the Id after save
                typeof(UserAddress).GetProperty("Id")?.SetValue(address, savedAddressId);
            });

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(savedAddressId);

        capturedAddress.Should().NotBeNull();
        capturedAddress!.UserId.Should().Be(Command.UserId);
        capturedAddress.Label.Should().Be(Command.Label);
        capturedAddress.Address.Street.Should().Be(Command.Street);
        capturedAddress.Address.City.Should().Be(Command.City);
        capturedAddress.Address.ZipCode.Should().Be(Command.ZipCode);
        capturedAddress.Address.Country.Should().Be(Command.Country);
        capturedAddress.IsDefault.Should().BeFalse();
        capturedAddress.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithUserNotFound_ShouldReturnError()
    {
        // Arrange
        SetupUserExists(false);
        SetupLocalizedMessage(UserAddressConsts.UserNotFound);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(UserAddressConsts.UserNotFound);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoDefaultAddress_ShouldSetAsDefault()
    {
        // Arrange
        SetupUserExists(true);
        SetupHasDefaultAddress(false);

        UserAddress? capturedAddress = null;
        UserAddressRepositoryMock
            .Setup(x => x.Add(It.IsAny<UserAddress>()))
            .Callback<UserAddress>(address => capturedAddress = address);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        capturedAddress.Should().NotBeNull();
        capturedAddress?.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenIsDefaultIsTrue_ShouldCallSetDefaultAddress()
    {
        // Arrange
        var commandWithDefault = Command with { IsDefault = true };
        SetupUserExists(true);
        SetupHasDefaultAddress(true);

        UserAddress? capturedAddress = null;
        UserAddressRepositoryMock
            .Setup(x => x.Add(It.IsAny<UserAddress>()))
            .Callback<UserAddress>(address => capturedAddress = address);

        // Act
        var result = await Handler.Handle(commandWithDefault, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        UserAddressRepositoryMock.Verify(
            x => x.SetDefaultAddressAsync(commandWithDefault.UserId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidAddressData_ShouldCreateCorrectAddress()
    {
        // Arrange
        var street = "456 Oak Avenue";
        var city = "Boston";
        var zipCode = "02101";
        var country = "United States";

        var customCommand = new AddUserAddressCommand(
            UserId,
            "Work",
            street,
            city,
            zipCode,
            country);

        SetupUserExists(true);
        SetupHasDefaultAddress(true);

        UserAddress? capturedAddress = null;
        UserAddressRepositoryMock
            .Setup(x => x.Add(It.IsAny<UserAddress>()))
            .Callback<UserAddress>(address => capturedAddress = address);

        // Act
        var result = await Handler.Handle(customCommand, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        capturedAddress.Should().NotBeNull();
        capturedAddress?.Address.Street.Should().Be(street);
        capturedAddress?.Address.City.Should().Be(city);
        capturedAddress?.Address.ZipCode.Should().Be(zipCode);
        capturedAddress?.Address.Country.Should().Be(country);
    }

    [Theory]
    [InlineData("Home")]
    [InlineData("Work")]
    [InlineData("Parents' House")]
    [InlineData("Vacation Home")]
    public async Task Handle_WithDifferentLabels_ShouldCreateUserAddress(string label)
    {
        // Arrange
        var commandWithLabel = Command with { Label = label };
        SetupUserExists(true);
        SetupHasDefaultAddress(true);

        UserAddress? capturedAddress = null;
        UserAddressRepositoryMock
            .Setup(x => x.Add(It.IsAny<UserAddress>()))
            .Callback<UserAddress>(address => capturedAddress = address);

        // Act
        var result = await Handler.Handle(commandWithLabel, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        capturedAddress.Should().NotBeNull();
        capturedAddress?.Label.Should().Be(label);
    }
} 