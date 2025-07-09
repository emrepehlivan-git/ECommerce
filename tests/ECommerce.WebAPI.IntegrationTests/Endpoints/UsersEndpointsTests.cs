namespace ECommerce.WebAPI.IntegrationTests.Endpoints;

public class UsersEndpointsTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = default!;

    public UsersEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    [Fact]
    public async Task GetUsers_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/v1/Users");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUser_ReturnsOk()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = User.Create("john@example.com", "John", "Doe");
        user.SecurityStamp = Guid.NewGuid().ToString();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var response = await _client.GetAsync($"/api/v1/Users/{user.Id}");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ActivateUser_ActivatesUser()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = User.Create("john2@example.com", "John", "Smith");
        user.SecurityStamp = Guid.NewGuid().ToString();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var response = await _client.PostAsync($"/api/v1/Users/activate/{user.Id}", null);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"ActivateUser response: {content}");
        }
        response.EnsureSuccessStatusCode();

        var updated = await context.Users.FindAsync(user.Id);
        updated!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateUser_DeactivatesUser()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = User.Create("jane@example.com", "Jane", "Doe");
        user.SecurityStamp = Guid.NewGuid().ToString();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var response = await _client.PostAsync($"/api/v1/Users/deactivate/{user.Id}", null);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"DeactivateUser response: {content}");
        }
        response.EnsureSuccessStatusCode();

        // Yeni scope/context ile güncel user'ı çek
        using (var newScope = _factory.Services.CreateScope())
        {
            var newContext = newScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updated = await newContext.Users.FindAsync(user.Id);
            updated!.IsActive.Should().BeFalse();
        }
    }
}
