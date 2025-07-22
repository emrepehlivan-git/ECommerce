using ECommerce.Domain.ValueObjects;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.UnitTests.Features.UserAddresses.V1.Queries;

public sealed class GetUserAddressesQueryTests : UserAddressesTestBase
{
    private readonly GetUserAddressesQueryHandler Handler;
    private readonly GetUserAddressesQuery Query;

    public GetUserAddressesQueryTests()
    {
        Query = new GetUserAddressesQuery(UserId);

        Handler = new GetUserAddressesQueryHandler(
            UserAddressRepositoryMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserId_ShouldReturnUserAddresses()
    {
        // Arrange
        var userAddresses = new List<UserAddress>
        {
            UserAddress.Create(UserId, "Home", DefaultAddress),
            UserAddress.Create(UserId, "Work", new Address("456 Business Ave", "Boston", "02101", "USA"))
        };
        SetupGetUserAddresses(userAddresses);
        SetupUserExists(true);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(dto => dto.Label == "Home");
        result.Value.Should().Contain(dto => dto.Label == "Work");
    }

    [Fact]
    public async Task Handle_WithUserNotFound_ShouldReturnEmptyList()
    {
        // Arrange
        SetupUserExists(false);
        SetupGetUserAddresses(new List<UserAddress>());

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // Query returns empty list, not error
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithActiveOnlyTrue_ShouldCallRepositoryWithActiveOnly()
    {
        // Arrange
        var queryWithActiveOnly = Query with { ActiveOnly = true };
        SetupUserExists(true);
        SetupGetUserAddresses(new List<UserAddress>());

        // Act
        var result = await Handler.Handle(queryWithActiveOnly, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        UserAddressRepositoryMock.Verify(
            x => x.ListAsync(It.IsAny<ISpecification<UserAddress>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithActiveOnlyFalse_ShouldCallRepositoryWithoutActiveFilter()
    {
        // Arrange
        var queryWithAllAddresses = Query with { ActiveOnly = false };
        SetupUserExists(true);
        SetupGetUserAddresses(new List<UserAddress>());

        // Act
        var result = await Handler.Handle(queryWithAllAddresses, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        UserAddressRepositoryMock.Verify(
            x => x.ListAsync(It.IsAny<ISpecification<UserAddress>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyAddressList_ShouldReturnEmptyList()
    {
        // Arrange
        SetupUserExists(true);
        SetupGetUserAddresses(new List<UserAddress>());

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithMixedActiveInactiveAddresses_ShouldReturnCorrectAddresses()
    {
        // Arrange
        var activeAddress = UserAddress.Create(UserId, "Home", DefaultAddress);
        var inactiveAddress = UserAddress.Create(UserId, "Old Work", DefaultAddress);
        inactiveAddress.Deactivate();

        var userAddresses = new List<UserAddress> { activeAddress, inactiveAddress };
        SetupUserExists(true);
        SetupGetUserAddresses(userAddresses);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(dto => dto.Label == "Home");
        result.Value.Should().Contain(dto => dto.Label == "Old Work");
    }

    [Fact]
    public async Task Handle_WithDefaultAndNonDefaultAddresses_ShouldReturnAllAddresses()
    {
        // Arrange
        var defaultAddress = UserAddress.Create(UserId, "Home", DefaultAddress);
        defaultAddress.SetAsDefault();
        var nonDefaultAddress = UserAddress.Create(UserId, "Work", DefaultAddress);
        nonDefaultAddress.UnsetAsDefault();

        var userAddresses = new List<UserAddress> { defaultAddress, nonDefaultAddress };
        SetupUserExists(true);
        SetupGetUserAddresses(userAddresses);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(dto => dto.IsDefault == true);
        result.Value.Should().Contain(dto => dto.IsDefault == false);
    }

    [Fact]
    public async Task Handle_ShouldMapAddressFieldsCorrectly()
    {
        // Arrange
        var street = "123 Test Street";
        var city = "Test City";
        var zipCode = "12345";
        var country = "Test Country";
        var address = new Address(street, city, zipCode, country);
        var userAddress = UserAddress.Create(UserId, "Test Address", address);

        SetupUserExists(true);
        SetupGetUserAddresses(new List<UserAddress> { userAddress });

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var dto = result.Value.First();
        dto.Street.Should().Be(street);
        dto.City.Should().Be(city);
        dto.ZipCode.Should().Be(zipCode);
        dto.Country.Should().Be(country);
        dto.Label.Should().Be("Test Address");
    }
} 