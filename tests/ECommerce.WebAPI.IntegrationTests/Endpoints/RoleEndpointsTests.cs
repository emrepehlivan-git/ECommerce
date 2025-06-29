namespace ECommerce.WebAPI.IntegrationTests.Endpoints;

public class RoleEndpointsTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = default!;

    public RoleEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _client = _factory.CreateClient();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetRoles_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/Role");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task GetRoleById_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync($"/api/Role/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserRoles_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync($"/api/Role/user/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateRole_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var command = new { Name = "TestRole" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Role", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateRole_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var command = new { Name = "UpdatedRole" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Role/{Guid.NewGuid()}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteRole_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/Role/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddUserToRole_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.PostAsJsonAsync($"/api/Role/user/{Guid.NewGuid()}/add-role", new { roleId = Guid.NewGuid()});

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveUserFromRole_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.PostAsJsonAsync($"/api/Role/user/{Guid.NewGuid()}/remove-role", new { roleId = Guid.NewGuid()});

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
} 