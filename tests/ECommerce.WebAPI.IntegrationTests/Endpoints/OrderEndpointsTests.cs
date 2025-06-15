using ECommerce.Domain.ValueObjects;

namespace ECommerce.WebAPI.IntegrationTests.Endpoints;

public class OrderEndpointsTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = default!;

    public OrderEndpointsTests(CustomWebApplicationFactory factory)
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
    public async Task GetOrders_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/Order");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<(Guid orderId, User user, Product product)> CreateOrderAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Her test için benzersiz veriler oluştur
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var user = User.Create($"order{uniqueId}@example.com", "Order", "User");
        var uniqueCategoryName = $"Orders_{uniqueId}";
        var category = Category.Create(uniqueCategoryName);
        category.Id = Guid.NewGuid();
        
        context.Users.Add(user);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product = Product.Create($"Item_{uniqueId}", "desc", 10m, category.Id, 5);
        context.Products.Add(product);
        context.ProductStocks.Add(product.Stock);
        await context.SaveChangesAsync();

        var address = new
        {
            Street = "Test Street",
            City = "Test City",
            State = "Test State",
            ZipCode = "12345",
            Country = "Test Country"
        };
        var command = new
        {
            UserId = user.Id,
            ShippingAddress = address,
            BillingAddress = address,
            Items = new[] { new { ProductId = product.Id, Quantity = 1 } }
        };

        var response = await _client.PostAsJsonAsync("/api/Order", command);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"CreateOrderAsync response: {content}");
        }
        response.EnsureSuccessStatusCode();

        // Oluşturulan order'ı bul - user ID'ye göre
        var order = await context.Orders
            .Where(o => o.UserId == user.Id)
            .FirstAsync();
        return (order.Id, user, product);
    }

    [Fact]
    public async Task PlaceOrder_PersistsOrder()
    {
        var (orderId, user, product) = await CreateOrderAsync();
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        order.Should().NotBeNull();
        order!.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task GetOrderById_ReturnsOk()
    {
        var (orderId, _, _) = await CreateOrderAsync();
        var response = await _client.GetAsync($"/api/Order/{orderId}");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetOrdersByUser_ReturnsOk()
    {
        var (orderId, user, _) = await CreateOrderAsync();
        var response = await _client.GetAsync($"/api/Order/user/{user.Id}");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddOrderItem_AddsItem()
    {
        var (orderId, _, product) = await CreateOrderAsync();
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var categoryId = product.CategoryId;
        var secondProduct = Product.Create("Second", "desc", 5m, categoryId, 5);
        context.Products.Add(secondProduct);
        context.ProductStocks.Add(secondProduct.Stock);
        await context.SaveChangesAsync();

        var response = await _client.PostAsJsonAsync($"/api/Order/{orderId}/items", new { ProductId = secondProduct.Id, Quantity = 2 });
        response.EnsureSuccessStatusCode();

        var order = await context.Orders.Include(o => o.Items).FirstAsync(o => o.Id == orderId);
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task RemoveOrderItem_RemovesItem()
    {
        var (orderId, _, product) = await CreateOrderAsync();
        var response = await _client.DeleteAsync($"/api/Order/{orderId}/items/{product.Id}");
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"RemoveOrderItem response: {content}");
        }
        response.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await context.Orders.Include(o => o.Items).FirstAsync(o => o.Id == orderId);
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CancelOrder_ChangesStatus()
    {
        var (orderId, _, _) = await CreateOrderAsync();
        var response = await _client.GetAsync($"/api/Order/cancel/{orderId}");
        response.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await context.Orders.FindAsync(orderId);
        order!.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task UpdateOrderStatus_UpdatesStatus()
    {
        var (orderId, _, _) = await CreateOrderAsync();
        var response = await _client.PostAsJsonAsync($"/api/Order/status/{orderId}", new { NewStatus = (byte)OrderStatus.Processing });
        response.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await context.Orders.FindAsync(orderId);
        order!.Status.Should().Be(OrderStatus.Processing);
    }
}
