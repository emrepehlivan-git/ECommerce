using ECommerce.Domain.ValueObjects;
using ECommerce.WebAPI.IntegrationTests.Common;

namespace ECommerce.WebAPI.IntegrationTests.Endpoints;

public class OrderEndpointsTests : BaseIntegrationTest, IAsyncLifetime
{
    public OrderEndpointsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    public async Task InitializeAsync()
    {
        await Factory.InitializeAsync();
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    [Fact]
    public async Task GetOrders_ReturnsOk()
    {
        await ResetDatabaseAsync();
        var response = await Client.GetAsync("/api/v1/Order");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<(Guid orderId, Guid productId)> CreateOrderAsync()
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

        var uniqueCategoryName = $"Category-{Guid.NewGuid()}";
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == uniqueCategoryName);
        if (category == null)
        {
            category = Category.Create(uniqueCategoryName);
            context.Categories.Add(category);
        }

        var product = Product.Create($"Product-{Guid.NewGuid()}", "desc", 10m, category.Id, 5);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var address = new
        {
            Street = "Test Street",
            City = "Test City",
            ZipCode = "12345",
            Country = "Test Country"
        };
        var command = new
        {
            UserId = userId,
            ShippingAddress = address,
            BillingAddress = address,
            Items = new[] { new { ProductId = product.Id, Quantity = 1 } }
        };

        var response = await Client.PostAsJsonAsync("/api/v1/Order", command);
        response.EnsureSuccessStatusCode();

        var createdOrderId = await response.Content.ReadFromJsonAsync<Guid>();
        
        return (createdOrderId, product.Id);
    }

    [Fact]
    public async Task PlaceOrder_PersistsOrder()
    {
        await ResetDatabaseAsync();
        var (orderId, _) = await CreateOrderAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        order.Should().NotBeNull();
        order!.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task GetOrderById_ReturnsOk()
    {
        await ResetDatabaseAsync();
        var (orderId, _) = await CreateOrderAsync();
        var response = await Client.GetAsync($"/api/v1/Order/{orderId}");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetOrdersByUser_ReturnsOk()
    {
        await ResetDatabaseAsync();
        await CreateOrderAsync();
        var response = await Client.GetAsync($"/api/v1/Order/user/{TestAuthHandler.TestUserId}");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddOrderItem_AddsItem()
    {
        await ResetDatabaseAsync();
        var (orderId, _) = await CreateOrderAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var category = await context.Categories.FirstAsync();
        var secondProduct = Product.Create($"Second-{Guid.NewGuid()}", "desc", 5m, category.Id, 5);
        context.Products.Add(secondProduct);
        await context.SaveChangesAsync();

        var response = await Client.PostAsJsonAsync($"/api/v1/Order/{orderId}/items", new { ProductId = secondProduct.Id, Quantity = 2 });
        response.EnsureSuccessStatusCode();

        var order = await context.Orders.Include(o => o.Items).FirstAsync(o => o.Id == orderId);
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task RemoveOrderItem_RemovesItem()
    {
        await ResetDatabaseAsync();
        var (orderId, productId) = await CreateOrderAsync();
        var response = await Client.DeleteAsync($"/api/v1/Order/{orderId}/items/{productId}");
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"RemoveOrderItem response: {content}");
        }
        response.EnsureSuccessStatusCode();

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await context.Orders.Include(o => o.Items).FirstAsync(o => o.Id == orderId);
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CancelOrder_ChangesStatus()
    {
        await ResetDatabaseAsync();
        var (orderId, _) = await CreateOrderAsync();
        var response = await Client.GetAsync($"/api/v1/Order/cancel/{orderId}");
        response.EnsureSuccessStatusCode();

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await context.Orders.FindAsync(orderId);
        order!.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task UpdateOrderStatus_UpdatesStatus()
    {
        await ResetDatabaseAsync();
        var (orderId, _) = await CreateOrderAsync();
        var response = await Client.PostAsJsonAsync($"/api/v1/Order/status/{orderId}", new { NewStatus = (byte)OrderStatus.Processing });
        response.EnsureSuccessStatusCode();

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await context.Orders.FindAsync(orderId);
        order!.Status.Should().Be(OrderStatus.Processing);
    }
}
