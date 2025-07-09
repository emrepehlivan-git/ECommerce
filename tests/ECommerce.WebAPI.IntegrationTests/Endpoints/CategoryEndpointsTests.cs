using ECommerce.WebAPI.IntegrationTests.Common;

namespace ECommerce.WebAPI.IntegrationTests.Endpoints;

public class CategoryEndpointsTests : BaseIntegrationTest, IAsyncLifetime
{
    public CategoryEndpointsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    public async Task InitializeAsync()
    {
        await Factory.InitializeAsync();
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    [Fact]
    public async Task GetCategories_ReturnsOk()
    {
        await ResetDatabaseAsync();
        var response = await Client.GetAsync("/api/v1/Category");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCategoryById_ReturnsOk()
    {
        await ResetDatabaseAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create("IntegrationCat");
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var response = await Client.GetAsync($"/api/v1/Category/{category.Id}");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateCategory_RequiresAuthorization()
    {
        await ResetDatabaseAsync();
        var command = new { Name = "New Category" };
        var response = await Client.PostAsJsonAsync("/api/v1/Category", command);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCategory_RequiresAuthorization()
    {
        await ResetDatabaseAsync();
        var command = new { Name = "Updated" };
        var response = await Client.PutAsJsonAsync($"/api/v1/Category/{Guid.NewGuid()}", command);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteCategory_RequiresAuthorization()
    {
        await ResetDatabaseAsync();
        var response = await Client.DeleteAsync($"/api/v1/Category/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
