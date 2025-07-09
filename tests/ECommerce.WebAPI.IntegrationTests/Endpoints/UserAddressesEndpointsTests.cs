using ECommerce.Application.Features.UserAddresses.V1.Commands;
using ECommerce.Application.Features.UserAddresses.V1.DTOs;
using ECommerce.Domain.ValueObjects;
using ECommerce.WebAPI.IntegrationTests.Common;

namespace ECommerce.WebAPI.IntegrationTests.Endpoints;

public class UserAddressesEndpointsTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseUrl = "/api/v1/user-addresses";

    public UserAddressesEndpointsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    public async Task InitializeAsync()
    {
        await Factory.InitializeAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
    
    private async Task<UserAddress> CreateUserAddressAsync(bool isDefault = false)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userId = Guid.Parse(TestAuthHandler.TestUserId);

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            user = User.Create("test@test.com", "test", "user");
            user.Id = userId;
            context.Users.Add(user);
        }

        var address = UserAddress.Create(
            userId,
            $"Home-{Guid.NewGuid()}",
            new Address("123 Main St", "Anytown", "12345", "USA"),
            isDefault
        );
        context.UserAddresses.Add(address);
        await context.SaveChangesAsync();
        return address;
    }

    [Fact]
    public async Task GetUserAddresses_ShouldReturnAddresses()
    {
        // Arrange
        await ResetDatabaseAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userAddress = UserAddress.Create(Guid.Parse(TestAuthHandler.TestUserId), "Home", new Address("123 Main St", "Anytown", "12345", "USA"));
        context.UserAddresses.Add(userAddress);
        await context.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var addresses = await response.Content.ReadFromJsonAsync<List<UserAddressDto>>();
        addresses.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetUserAddressById_WithValidId_ShouldReturnAddress()
    {
        // Arrange
        await ResetDatabaseAsync();
        var address = UserAddress.Create(Guid.Parse(TestAuthHandler.TestUserId), "Work", new Address("456 Oak Ave", "Somecity", "67890", "USA"));
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.UserAddresses.Add(address);
            await context.SaveChangesAsync();
        }

        // Act
        var response = await Client.GetAsync($"{BaseUrl}/{address.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserAddressDto>();
        result.Should().NotBeNull();
        result!.Label.Should().Be("Work");
    }

    [Fact]
    public async Task CreateUserAddress_WithValidCommand_ShouldReturnCreated()
    {
        // Arrange
        await ResetDatabaseAsync();
        var command = new AddUserAddressCommand(
            Guid.Parse(TestAuthHandler.TestUserId), 
            "Shipping", 
            "789 Pine Ln", 
            "Otherplace", 
            "54321", 
            "USA");

        // Act
        var response = await Client.PostAsJsonAsync(BaseUrl, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();
    }

    private async Task<(Guid, UpdateUserAddressCommand)> CreateAddressAndCommand()
    {
        await ResetDatabaseAsync();
        var address = UserAddress.Create(Guid.Parse(TestAuthHandler.TestUserId), "Billing", new Address("111 Maple St", "Metropolis", "11223", "USA"));
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.UserAddresses.Add(address);
            await context.SaveChangesAsync();
        }

        var command = new UpdateUserAddressCommand(
            address.Id,
            Guid.Parse(TestAuthHandler.TestUserId), 
            "Billing Updated", 
            "222 Elm St", 
            "Gotham", 
            "11224",
            "USA");
        return (address.Id, command);
    }
    
    private async Task<(Guid, SetDefaultUserAddressCommand)> CreateAddressAndDefaultCommand()
    {
        await ResetDatabaseAsync();
        var address = UserAddress.Create(Guid.Parse(TestAuthHandler.TestUserId), "Default Test", new Address("333 Birch Rd", "Star City", "44556", "USA"));
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.UserAddresses.Add(address);
            await context.SaveChangesAsync();
        }

        var command = new SetDefaultUserAddressCommand(address.Id, Guid.Parse(TestAuthHandler.TestUserId));
        return (address.Id, command);
    }
    
    private async Task<(Guid, object)> CreateAddressAndPatchDocument()
    {
        await ResetDatabaseAsync();
        var address = UserAddress.Create(Guid.Parse(TestAuthHandler.TestUserId), "Patch Test", new Address("555 Spruce Ct", "Coast City", "99001", "USA"));
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.UserAddresses.Add(address);
            await context.SaveChangesAsync();
        }

        var patchDoc = new[]
        {
            new { op = "replace", path = "/label", value = "Patched Label" }
        };

        return (address.Id, patchDoc);
    }
    
    private async Task<(Guid, DeleteUserAddressCommand)> CreateAddressAndDeleteCommand()
    {
        await ResetDatabaseAsync();
        var address = UserAddress.Create(Guid.Parse(TestAuthHandler.TestUserId), "Delete Test", new Address("666 Willow Way", "Fawcett City", "12131", "USA"));
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.UserAddresses.Add(address);
            await context.SaveChangesAsync();
        }

        var command = new DeleteUserAddressCommand(address.Id, Guid.Parse(TestAuthHandler.TestUserId));
        return (address.Id, command);
    }
} 