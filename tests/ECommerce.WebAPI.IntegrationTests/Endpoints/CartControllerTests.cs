using ECommerce.Application.Features.Carts.V1.Commands;
using ECommerce.WebAPI.IntegrationTests.Common;

namespace ECommerce.WebAPI.IntegrationTests.Endpoints;

public class CartControllerTests : BaseIntegrationTest, IAsyncLifetime
{
    public CartControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    public async Task InitializeAsync()
    {
        await Factory.InitializeAsync();
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    [Fact]
    public async Task GetCart_WhenUserHasNoCart_ReturnsOkWithEmptyCart()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var cart = await GetCartAsync();

        // Assert
        cart.Should().NotBeNull();
        cart!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task AddToCart_WithValidData_ReturnsOkWithCartSummary()
    {
        await ResetDatabaseAsync();
        var product = await CreateProductAsync();
        var summary = await AddToCartAsync(product.Id, 1);

        summary.Should().NotBeNull();
        summary.TotalItems.Should().Be(1);
        summary.TotalAmount.Should().Be(100m);
    }

    [Fact]
    public async Task GetCart_ReturnsOk()
    {
        await ResetDatabaseAsync();
        var product = await CreateProductAsync();
        await AddToCartAsync(product.Id, 2);

        var cart = await GetCartAsync();

        cart.Should().NotBeNull();
        cart.Items.Should().ContainSingle();
        cart.Items.First().Quantity.Should().Be(2);
        cart.Items.First().ProductName.Should().Be("Test Product");
    }

    [Fact]
    public async Task AddToCart_ExceedsStock_ReturnsUnprocessableEntity()
    {
        await ResetDatabaseAsync();
        var product = await CreateProductAsync("Test Product", 100m, 1);
        var command = new AddToCartCommand(product.Id, 2);

        var response = await Client.PostAsJsonAsync("/api/v1/Cart/add", command);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task RemoveFromCart_WithExistingProduct_ReturnsOkWithCartSummary()
    {
        await ResetDatabaseAsync();
        var product = await CreateProductAsync();
        await AddToCartAsync(product.Id, 1);

        var summary = await RemoveFromCartAsync(product.Id);

        summary.Should().NotBeNull();
        summary.TotalItems.Should().Be(0);
        summary.TotalAmount.Should().Be(0);
    }
    
    [Fact]
    public async Task UpdateQuantity_WithValidData_ReturnsOkWithCartSummary()
    {
        await ResetDatabaseAsync();
        var product = await CreateProductAsync();
        await AddToCartAsync(product.Id, 1);
        var command = new UpdateCartItemQuantityCommand(product.Id, 5);

        var summary = await UpdateQuantityAsync(command.ProductId, command.Quantity);

        summary.Should().NotBeNull();
        summary.TotalItems.Should().Be(1);
        summary.TotalAmount.Should().Be(500m);
    }
    
    [Fact]
    public async Task ClearCart_WhenCartIsNotEmpty_ReturnsNoContent()
    {
        await ResetDatabaseAsync();
        var product1 = await CreateProductAsync("Product 1", 10m, 5);
        await AddToCartAsync(product1.Id, 2);
        var product2 = await CreateProductAsync("Product 2", 20m, 3);
        await AddToCartAsync(product2.Id, 3);

        var response = await Client.DeleteAsync("/api/v1/Cart/clear");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var cart = await GetCartAsync();
        cart.Items.Should().BeEmpty();
    }


    private async Task<Product> CreateProductAsync(string name = "Test Product", decimal price = 100m, int stock = 10)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Test Category");
        if (category is null)
        {
            category = Category.Create("Test Category");
            context.Categories.Add(category);
        }

        var product = Product.Create(name, "description", price, category.Id, stock);
        context.Products.Add(product);
        context.ProductStocks.Add(product.Stock);

        await context.SaveChangesAsync();
        return product;
    }

    private async Task<CartSummaryDto> AddToCartAsync(Guid productId, int quantity)
    {
        var command = new AddToCartCommand(productId, quantity);
        var response = await Client.PostAsJsonAsync("/api/v1/Cart/add", command);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CartSummaryDto>())!;
    }
    
    private async Task<CartDto> GetCartAsync()
    {
        var response = await Client.GetAsync("/api/v1/Cart");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CartDto>())!;
    }

    private async Task<CartSummaryDto> RemoveFromCartAsync(Guid productId)
    {
        var response = await Client.DeleteAsync($"/api/v1/Cart/remove/{productId}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CartSummaryDto>())!;
    }

    private async Task<CartSummaryDto> UpdateQuantityAsync(Guid productId, int quantity)
    {
        var command = new UpdateCartItemQuantityCommand(productId, quantity);
        var response = await Client.PutAsJsonAsync("/api/v1/Cart/update-quantity", command);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CartSummaryDto>())!;
    }
}

// Minimal DTOs for deserialization in tests
public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
}

public class CartItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
}

public class CartSummaryDto
{
    public int TotalItems { get; set; }
    public decimal TotalAmount { get; set; }
} 