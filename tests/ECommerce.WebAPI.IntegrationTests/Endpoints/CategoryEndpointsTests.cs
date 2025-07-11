using ECommerce.Persistence.Contexts;
using ECommerce.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.WebAPI.IntegrationTests.Endpoints;

public class CategoryEndpointsTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _unauthenticatedClient = default!;
    private HttpClient _authenticatedClient = default!;

    public CategoryEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _unauthenticatedClient = _factory.CreateUnauthenticatedClient();
        _authenticatedClient = _factory.CreateClient();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetCategories_WithoutAuth_ReturnsOk()
    {
        var response = await _unauthenticatedClient.GetAsync("/api/v1/Category");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCategoryById_WithoutAuth_ReturnsOk()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create("IntegrationCat");
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var response = await _unauthenticatedClient.GetAsync($"/api/v1/Category/{category.Id}");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateCategory_WithAuth_ReturnsCreated()
    {
        var command = new { Name = "New Category" };
        var response = await _authenticatedClient.PostAsJsonAsync("/api/v1/Category", command);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateCategory_WithoutAuth_ReturnsUnauthorized()
    {
        var command = new { Name = "New Category" };
        var response = await _unauthenticatedClient.PostAsJsonAsync("/api/v1/Category", command);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCategory_WithoutAuth_ReturnsUnauthorized()
    {
        var command = new { Id = Guid.NewGuid(), Name = "Updated" };
        var response = await _unauthenticatedClient.PutAsJsonAsync($"/api/v1/Category/{Guid.NewGuid()}", command);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteCategory_WithoutAuth_ReturnsUnauthorized()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create("Category to delete");
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var response = await _unauthenticatedClient.DeleteAsync($"/api/v1/Category/{category.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCategory_WithAuth_ReturnsOk()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create("Category to update");
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var command = new { Name = "Updated Category Name" };
        var response = await _authenticatedClient.PutAsJsonAsync($"/api/v1/Category/{category.Id}", command);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteCategory_WithAuth_ReturnsNoContent()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create("Category to delete with auth");
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var response = await _authenticatedClient.DeleteAsync($"/api/v1/Category/{category.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetCategoryById_WithNonExistentId_ReturnsNotFound()
    {
        var nonExistentId = Guid.NewGuid();
        var response = await _unauthenticatedClient.GetAsync($"/api/v1/Category/{nonExistentId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCategory_WithNonExistentId_ReturnsNotFound()
    {
        var nonExistentId = Guid.NewGuid();
        var command = new { Id = nonExistentId, Name = "Updated Category Name" };
        var response = await _authenticatedClient.PutAsJsonAsync($"/api/v1/Category/{nonExistentId}", command);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteCategory_WithNonExistentId_ReturnsNotFound()
    {
        var nonExistentId = Guid.NewGuid();
        var response = await _authenticatedClient.DeleteAsync($"/api/v1/Category/{nonExistentId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCategory_WithEmptyName_ReturnsBadRequest()
    {
        var command = new { Name = "" };
        var response = await _authenticatedClient.PostAsJsonAsync("/api/v1/Category", command);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCategory_WithEmptyName_ReturnsBadRequest()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create("Category to update with invalid data");
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var command = new { Name = "" };
        var response = await _authenticatedClient.PutAsJsonAsync($"/api/v1/Category/{category.Id}", command);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
