using System.Text;
using System.Text.Json;
using ECommerce.Application.Features.UserAddresses.V1.Commands;
using ECommerce.Domain.ValueObjects;
using ECommerce.WebAPI.Controllers.V1;

namespace ECommerce.WebAPI.IntegrationTests.Endpoints;

public class UserAddressesEndpointsTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory Factory;
    private HttpClient Client = default!;
    private User TestUser = default!;

    public UserAddressesEndpointsTests(CustomWebApplicationFactory factory)
    {
        Factory = factory;
    }

    public async Task InitializeAsync()
    {
        await Factory.InitializeAsync();
        Client = Factory.CreateClient();
        await CreateTestUser();
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    private async Task CreateTestUser()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestUser = User.Create("testuser@example.com", "Test", "User");
        TestUser.SecurityStamp = Guid.NewGuid().ToString();
        context.Users.Add(TestUser);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetUserAddresses_WithValidUserId_ReturnsOk()
    {
        // Act
        var response = await Client.GetAsync($"/api/UserAddresses/user/{TestUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetUserAddresses_WithInvalidUserId_ReturnsOk()
    {
        // Act
        var response = await Client.GetAsync($"/api/UserAddresses/user/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("[]"); // Empty array for non-existent user
    }

    [Fact]
    public async Task AddUserAddress_WithValidData_ReturnsCreated()
    {
        // Arrange
        var command = new AddUserAddressCommand(
            TestUser.Id,
            "Home",
            "123 Main St",
            "New York",
            "10001",
            "USA");

        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/UserAddresses", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeNullOrEmpty();

        // Verify the address was created in database
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userAddress = await context.UserAddresses
            .FirstOrDefaultAsync(ua => ua.UserId == TestUser.Id && ua.Label == "Home");
        
        userAddress.Should().NotBeNull();
        userAddress!.Address.Street.Should().Be("123 Main St");
        userAddress.IsDefault.Should().BeTrue(); // First address should be default
    }

    [Fact]
    public async Task AddUserAddress_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var command = new AddUserAddressCommand(
            TestUser.Id,
            "", // Invalid empty label
            "123 Main St",
            "New York", 
            "10001",
            "USA");

        var json = JsonSerializer.Serialize(command);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/UserAddresses", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUserAddress_WithValidData_ReturnsOk()
    {
        // Arrange - First create an address
        var address = UserAddress.Create(TestUser.Id, "Work", new Address("456 Business Ave", "Boston", "02101", "USA"));
        
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.UserAddresses.Add(address);
            await context.SaveChangesAsync();
        }

        var updateCommand = new UpdateUserAddressCommand(
            address.Id,
            TestUser.Id,
            "Updated Work",
            "789 Updated Ave",
            "Chicago",
            "60601",
            "USA");

        var json = JsonSerializer.Serialize(updateCommand);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync($"/api/UserAddresses/{address.Id}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the address was updated in database
        using var scope2 = Factory.Services.CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedAddress = await context2.UserAddresses.FindAsync(address.Id);
        
        updatedAddress.Should().NotBeNull();
        updatedAddress!.Label.Should().Be("Updated Work");
        updatedAddress.Address.Street.Should().Be("789 Updated Ave");
        updatedAddress.Address.City.Should().Be("Chicago");
    }

    [Fact]
    public async Task UpdateUserAddress_WithNonExistentId_ReturnsBadRequest()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateCommand = new UpdateUserAddressCommand(
            nonExistentId,
            TestUser.Id,
            "Updated Work",
            "789 Updated Ave",
            "Chicago",
            "60601",
            "USA");

        var json = JsonSerializer.Serialize(updateCommand);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync($"/api/UserAddresses/{nonExistentId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetDefaultUserAddress_WithValidData_ReturnsOk()
    {
        // Arrange - Create two addresses
        var address1 = UserAddress.Create(TestUser.Id, "Home", new Address("123 Home St", "New York", "10001", "USA"));
        var address2 = UserAddress.Create(TestUser.Id, "Work", new Address("456 Work Ave", "Boston", "02101", "USA"));
        address1.SetAsDefault(); // Set first as default

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.UserAddresses.AddRange(address1, address2);
            await context.SaveChangesAsync();
        }

        var setDefaultCommand = new SetDefaultUserAddressCommand(address2.Id, TestUser.Id);
        var json = JsonSerializer.Serialize(setDefaultCommand);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PatchAsync($"/api/UserAddresses/{address2.Id}/set-default", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the default address was changed
        using var scope2 = Factory.Services.CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var addresses = await context2.UserAddresses
            .Where(ua => ua.UserId == TestUser.Id)
            .ToListAsync();

        var defaultAddress = addresses.FirstOrDefault(a => a.IsDefault);
        defaultAddress.Should().NotBeNull();
        defaultAddress!.Id.Should().Be(address2.Id);
    }

    [Fact]
    public async Task SetDefaultUserAddress_WithNonExistentId_ReturnsBadRequest()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var setDefaultCommand = new SetDefaultUserAddressCommand(nonExistentId, TestUser.Id);
        var json = JsonSerializer.Serialize(setDefaultCommand);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PatchAsync($"/api/UserAddresses/{nonExistentId}/set-default", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteUserAddress_WithValidData_ReturnsOk()
    {
        // Arrange - Create two addresses (one default, one not)
        var address1 = UserAddress.Create(TestUser.Id, "Home", new Address("123 Home St", "New York", "10001", "USA"));
        var address2 = UserAddress.Create(TestUser.Id, "Work", new Address("456 Work Ave", "Boston", "02101", "USA"));
        address1.SetAsDefault(); // Set first as default

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.UserAddresses.AddRange(address1, address2);
            await context.SaveChangesAsync();
        }

        // Act - Delete the non-default address
        var deleteRequest = new DeleteUserAddressRequest(TestUser.Id);
        var deleteJson = JsonSerializer.Serialize(deleteRequest);
        var deleteContent = new StringContent(deleteJson, Encoding.UTF8);
        deleteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/UserAddresses/{address2.Id}")
        {
            Content = deleteContent
        };
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the address was deactivated
        using var scope2 = Factory.Services.CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deletedAddress = await context2.UserAddresses.FindAsync(address2.Id);
        
        deletedAddress.Should().NotBeNull();
        deletedAddress!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserAddress_WithDefaultAddress_ReturnsBadRequest()
    {
        // Arrange - Create a default address
        var address = UserAddress.Create(TestUser.Id, "Home", new Address("123 Home St", "New York", "10001", "USA"));
        address.SetAsDefault();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.UserAddresses.Add(address);
            await context.SaveChangesAsync();
        }

        // Act - Try to delete the default address
        var deleteRequest = new DeleteUserAddressRequest(TestUser.Id);
        var deleteJson = JsonSerializer.Serialize(deleteRequest);
        var deleteContent = new StringContent(deleteJson, Encoding.UTF8);
        deleteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/UserAddresses/{address.Id}")
        {
            Content = deleteContent
        };
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify the address is still active
        using var scope2 = Factory.Services.CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notDeletedAddress = await context2.UserAddresses.FindAsync(address.Id);
        
        notDeletedAddress.Should().NotBeNull();
        notDeletedAddress!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUserAddress_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var deleteRequest = new DeleteUserAddressRequest(TestUser.Id);
        var deleteJson = JsonSerializer.Serialize(deleteRequest);
        var deleteContent = new StringContent(deleteJson, Encoding.UTF8);
        deleteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/UserAddresses/{nonExistentId}")
        {
            Content = deleteContent
        };
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserAddresses_WithActiveOnlyFilter_ReturnsOnlyActiveAddresses()
    {
        // Arrange - Create one active and one inactive address
        var activeAddress = UserAddress.Create(TestUser.Id, "Home", new Address("123 Home St", "New York", "10001", "USA"));
        var inactiveAddress = UserAddress.Create(TestUser.Id, "Old Work", new Address("456 Old Ave", "Boston", "02101", "USA"));
        inactiveAddress.Deactivate();

        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.UserAddresses.AddRange(activeAddress, inactiveAddress);
            await context.SaveChangesAsync();
        }

        // Act
        var response = await Client.GetAsync($"/api/UserAddresses/user/{TestUser.Id}?activeOnly=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var addresses = JsonSerializer.Deserialize<List<object>>(content);
        
        // Should only return the active address
        addresses.Should().HaveCount(1);
    }
} 