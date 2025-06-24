namespace ECommerce.Infrastructure.IntegrationTests.Repositories;

public class UserAddressRepositoryTests : RepositoryTestBase
{
    private readonly UserAddressRepository _repository;

    public UserAddressRepositoryTests()
    {
        _repository = new UserAddressRepository(Context);
    }

    [Fact]
    public async Task GetDefaultAddressAsync_ShouldReturnDefaultAddress_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User");
        user.Id = userId;
        Context.Users.Add(user);

        var address = UserAddress.Create(
            userId,
            "Home",
            new Address("123 Main St", "New York", "10001", "USA")
        );
        address.SetAsDefault();
        Context.UserAddresses.Add(address);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetDefaultAddressAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.IsDefault.Should().BeTrue();
        result.Label.Should().Be("Home");
    }

    [Fact]
    public async Task GetDefaultAddressAsync_ShouldReturnNull_WhenNoDefaultAddress()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.GetDefaultAddressAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserAddressesAsync_ShouldReturnActiveAddresses_WhenActiveOnlyTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User");
        user.Id = userId;
        Context.Users.Add(user);

        var activeAddress = UserAddress.Create(userId, "Home", new Address("123 Main St", "New York", "10001", "USA"));
        var inactiveAddress = UserAddress.Create(userId, "Work", new Address("456 Work St", "New York", "10002", "USA"));
        inactiveAddress.Deactivate();

        Context.UserAddresses.AddRange(activeAddress, inactiveAddress);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserAddressesAsync(userId, activeOnly: true);

        // Assert
        result.Should().HaveCount(1);
        result.First().Label.Should().Be("Home");
        result.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserAddressesAsync_ShouldReturnAllAddresses_WhenActiveOnlyFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User");
        user.Id = userId;
        Context.Users.Add(user);

        var activeAddress = UserAddress.Create(userId, "Home", new Address("123 Main St", "New York", "10001", "USA"));
        var inactiveAddress = UserAddress.Create(userId, "Work", new Address("456 Work St", "New York", "10002", "USA"));
        inactiveAddress.Deactivate();

        Context.UserAddresses.AddRange(activeAddress, inactiveAddress);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserAddressesAsync(userId, activeOnly: false);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task HasDefaultAddressAsync_ShouldReturnTrue_WhenDefaultAddressExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User");
        user.Id = userId;
        Context.Users.Add(user);

        var address = UserAddress.Create(userId, "Home", new Address("123 Main St", "New York", "10001", "USA"));
        address.SetAsDefault();
        Context.UserAddresses.Add(address);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.HasDefaultAddressAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasDefaultAddressAsync_ShouldReturnFalse_WhenNoDefaultAddress()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.HasDefaultAddressAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SetDefaultAddressAsync_ShouldSetNewDefaultAddress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User");
        user.Id = userId;
        Context.Users.Add(user);

        var oldDefaultAddress = UserAddress.Create(userId, "Home", new Address("123 Main St", "New York", "10001", "USA"));
        oldDefaultAddress.SetAsDefault();
        
        var newAddress = UserAddress.Create(userId, "Work", new Address("456 Work St", "New York", "10002", "USA"));

        Context.UserAddresses.AddRange(oldDefaultAddress, newAddress);
        await Context.SaveChangesAsync();

        // Act
        await _repository.SetDefaultAddressAsync(userId, newAddress.Id);
        await Context.SaveChangesAsync();

        // Assert
        var oldDefault = await Context.UserAddresses.FindAsync(oldDefaultAddress.Id);
        var newDefault = await Context.UserAddresses.FindAsync(newAddress.Id);

        oldDefault!.IsDefault.Should().BeFalse();
        newDefault!.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task SetDefaultAddressAsync_ShouldNotSetDefault_WhenAddressNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "Test", "User");
        user.Id = userId;
        Context.Users.Add(user);

        var address = UserAddress.Create(userId, "Home", new Address("123 Main St", "New York", "10001", "USA"));
        address.SetAsDefault();
        Context.UserAddresses.Add(address);
        await Context.SaveChangesAsync();

        // Act
        await _repository.SetDefaultAddressAsync(userId, Guid.NewGuid());
        await Context.SaveChangesAsync();

        // Assert
        var currentDefault = await Context.UserAddresses.FindAsync(address.Id);
        currentDefault!.IsDefault.Should().BeFalse(); // Should be unset but no new default set
    }
} 