using ECommerce.WebAPI.IntegrationTests.Common;

namespace ECommerce.WebAPI.IntegrationTests.Endpoints;

public class ProductEndpointsTests : BaseIntegrationTest, IAsyncLifetime
{
    public ProductEndpointsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    public async Task InitializeAsync()
    {
        await Factory.InitializeAsync();
    }

    public async Task DisposeAsync() => await Task.CompletedTask;

    private static string CreateUniqueCategoryName(string baseName) => $"{baseName}_{Guid.NewGuid():N}";

    [Fact]
    public async Task GetProducts_ReturnsOk()
    {
        await ResetDatabaseAsync();
        var response = await Client.GetAsync("/api/Product");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProducts_WithPagination_ReturnsOk()
    {
        await ResetDatabaseAsync();
        var response = await Client.GetAsync("/api/Product?page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProducts_WithIncludeCategory_ReturnsOk()
    {
        await ResetDatabaseAsync();
        var response = await Client.GetAsync("/api/Product?includeCategory=true");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProducts_WithOrderBy_ReturnsOk()
    {
        await ResetDatabaseAsync();
        var response = await Client.GetAsync("/api/Product?orderBy=name");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProductById_WithValidId_ReturnsOk()
    {
        await ResetDatabaseAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create(CreateUniqueCategoryName("Electronics"));
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product = Product.Create("Laptop", "Gaming laptop", 1500m, category.Id, 10);
        context.Products.Add(product);
        context.ProductStocks.Add(product.Stock);
        await context.SaveChangesAsync();

        var response = await Client.GetAsync($"/api/Product/{product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Laptop");
        content.Should().Contain("Gaming laptop");
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_ReturnsNotFound()
    {
        await ResetDatabaseAsync();
        var response = await Client.GetAsync($"/api/Product/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_PersistsProduct()
    {
        await ResetDatabaseAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create(CreateUniqueCategoryName("Integration"));
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var command = new
        {
            Name = "Phone",
            Description = "Smart phone",
            Price = 100m,
            CategoryId = category.Id,
            StockQuantity = 3
        };

        var response = await Client.PostAsJsonAsync("/api/Product", command);
        response.EnsureSuccessStatusCode();

        var product = await context.Products.Include(p => p.Stock).FirstOrDefaultAsync(p => p.Name == "Phone");
        product.Should().NotBeNull();
        product!.Name.Should().Be("Phone");
        product.Stock.Quantity.Should().Be(3);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ReturnsBadRequest()
    {
        await ResetDatabaseAsync();
        var command = new
        {
            Name = "",
            Description = "Test description",
            Price = 100m,
            CategoryId = Guid.NewGuid(),
            StockQuantity = 3
        };

        var response = await Client.PostAsJsonAsync("/api/Product", command);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_WithNegativePrice_ReturnsBadRequest()
    {
        await ResetDatabaseAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create(CreateUniqueCategoryName("TestCategory"));
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var command = new
        {
            Name = "Test Product",
            Description = "Test description",
            Price = -10m,
            CategoryId = category.Id,
            StockQuantity = 3
        };

        var response = await Client.PostAsJsonAsync("/api/Product", command);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_WithNonExistentCategory_ReturnsBadRequest()
    {
        await ResetDatabaseAsync();
        var command = new
        {
            Name = "Test Product",
            Description = "Test description",
            Price = 100m,
            CategoryId = Guid.NewGuid(),
            StockQuantity = 3
        };

        var response = await Client.PostAsJsonAsync("/api/Product", command);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsOk()
    {
        await ResetDatabaseAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create(CreateUniqueCategoryName("Electronics"));
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product = Product.Create("Old Name", "Old description", 100m, category.Id, 5);
        context.Products.Add(product);
        context.ProductStocks.Add(product.Stock);
        await context.SaveChangesAsync();

        var updateCommand = new
        {
            Name = "Updated Name",
            Description = "Updated description",
            Price = 150m,
            CategoryId = category.Id
        };

        var response = await Client.PutAsJsonAsync($"/api/Product/{product.Id}", updateCommand);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var verifyScope = Factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedProduct = await verifyContext.Products.FindAsync(product.Id);
        updatedProduct.Should().NotBeNull();
        updatedProduct!.Name.Should().Be("Updated Name");
        updatedProduct.Description.Should().Be("Updated description");
        updatedProduct.Price.Value.Should().Be(150m);
    }

    [Fact]
    public async Task UpdateProduct_WithNonExistentId_ReturnsNotFound()
    {
        await ResetDatabaseAsync();
        var command = new
        {
            Name = "Test Product",
            Description = "Test description",
            Price = 100m,
            CategoryId = Guid.NewGuid()
        };

        var response = await Client.PutAsJsonAsync($"/api/Product/{Guid.NewGuid()}", command);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidData_ReturnsBadRequest()
    {
        await ResetDatabaseAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create(CreateUniqueCategoryName("Electronics"));
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product = Product.Create("Test Product", "Test description", 100m, category.Id, 5);
        context.Products.Add(product);
        context.ProductStocks.Add(product.Stock);
        await context.SaveChangesAsync();

        var command = new
        {
            Name = "",
            Description = "Test description",
            Price = 100m,
            CategoryId = category.Id
        };

        var response = await Client.PutAsJsonAsync($"/api/Product/{product.Id}", command);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ReturnsNoContent()
    {
        await ResetDatabaseAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create(CreateUniqueCategoryName("Electronics"));
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product = Product.Create("To Delete", "Product to delete", 100m, category.Id, 5);
        context.Products.Add(product);
        context.ProductStocks.Add(product.Stock);
        await context.SaveChangesAsync();

        var response = await Client.DeleteAsync($"/api/Product/{product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var verifyScope = Factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deletedProduct = await verifyContext.Products.FindAsync(product.Id);
        deletedProduct.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProduct_WithNonExistentId_ReturnsNotFound()
    {
        await ResetDatabaseAsync();
        var response = await Client.DeleteAsync($"/api/Product/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProductStockInfo_WithValidId_ReturnsOk()
    {
        await ResetDatabaseAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create(CreateUniqueCategoryName("Electronics"));
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product = Product.Create("Stock Test", "Product for stock test", 100m, category.Id, 15);
        context.Products.Add(product);
        context.ProductStocks.Add(product.Stock);
        await context.SaveChangesAsync();

        var response = await Client.GetAsync($"/api/Product/{product.Id}/stock");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("15");
    }

    [Fact]
    public async Task GetProductStockInfo_WithNonExistentId_ReturnsNotFound()
    {
        await ResetDatabaseAsync();
        var response = await Client.GetAsync($"/api/Product/{Guid.NewGuid()}/stock");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProductStock_WithValidData_ReturnsOk()
    {
        await ResetDatabaseAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create(CreateUniqueCategoryName("Electronics"));
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product = Product.Create("Stock Update", "Product for stock update", 100m, category.Id, 10);
        context.Products.Add(product);
        context.ProductStocks.Add(product.Stock);
        await context.SaveChangesAsync();

        var updateCommand = new
        {
            Quantity = 25
        };

        var response = await Client.PutAsJsonAsync($"/api/Product/{product.Id}/stock", updateCommand);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var verifyScope = Factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedProduct = await verifyContext.Products.Include(p => p.Stock).FirstAsync(p => p.Id == product.Id);
        updatedProduct.Stock.Quantity.Should().Be(25);
    }

    [Fact]
    public async Task UpdateProductStock_WithNonExistentId_ReturnsNotFound()
    {
        await ResetDatabaseAsync();
        var command = new
        {
            Quantity = 10
        };

        var response = await Client.PutAsJsonAsync($"/api/Product/{Guid.NewGuid()}/stock", command);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProductStock_WithNegativeQuantity_ReturnsBadRequest()
    {
        await ResetDatabaseAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create(CreateUniqueCategoryName("Electronics"));
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product = Product.Create("Stock Test", "Product for negative stock test", 100m, category.Id, 10);
        context.Products.Add(product);
        context.ProductStocks.Add(product.Stock);
        await context.SaveChangesAsync();

        var command = new
        {
            Quantity = -5
        };

        var response = await Client.PutAsJsonAsync($"/api/Product/{product.Id}/stock", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateName_ReturnsBadRequest()
    {
        await ResetDatabaseAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create(CreateUniqueCategoryName("Electronics"));
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var existingProduct = Product.Create("Duplicate Name", "Existing product", 100m, category.Id, 5);
        context.Products.Add(existingProduct);
        context.ProductStocks.Add(existingProduct.Stock);
        await context.SaveChangesAsync();

        var command = new
        {
            Name = "Duplicate Name",
            Description = "New product with duplicate name",
            Price = 150m,
            CategoryId = category.Id,
            StockQuantity = 3
        };

        var response = await Client.PostAsJsonAsync("/api/Product", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProduct_WithDuplicateName_ReturnsBadRequest()
    {
        await ResetDatabaseAsync();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = Category.Create(CreateUniqueCategoryName("Electronics"));
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product1 = Product.Create("First Product", "First product", 100m, category.Id, 5);
        var product2 = Product.Create("Second Product", "Second product", 150m, category.Id, 3);
        context.Products.AddRange(product1, product2);
        context.ProductStocks.AddRange(product1.Stock, product2.Stock);
        await context.SaveChangesAsync();

        var updateCommand = new
        {
            Name = "First Product",
            Description = "Updated description",
            Price = 200m,
            CategoryId = category.Id
        };

        var response = await Client.PutAsJsonAsync($"/api/Product/{product2.Id}", updateCommand);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
