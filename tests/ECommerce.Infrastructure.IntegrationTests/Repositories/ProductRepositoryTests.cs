namespace ECommerce.Infrastructure.IntegrationTests.Repositories;

public class ProductRepositoryTests : RepositoryTestBase
{
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        _repository = new ProductRepository(Context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddProductToDatabase()
    {
        // Arrange
        var category = Category.Create("Electronics");
        category.Id = Guid.NewGuid();
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        var product = Product.Create("Smartphone", "Latest smartphone", 999.99m, category.Id, 10);

        // Act
        await _repository.AddAsync(product);
        await Context.SaveChangesAsync();

        // Assert
        var savedProduct = await Context.Products.FindAsync(product.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be("Smartphone");
        savedProduct.Description.Should().Be("Latest smartphone");
        savedProduct.Price.Value.Should().Be(999.99m);
        savedProduct.CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenExists()
    {
        // Arrange
        var category = Category.Create("Books");
        category.Id = Guid.NewGuid();
        Context.Categories.Add(category);

        var product = Product.Create("C# Programming", "Learn C# programming", 49.99m, category.Id, 5);
        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("C# Programming");
        result.Description.Should().Be("Learn C# programming");
        result.Price.Value.Should().Be(49.99m);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Query_ShouldReturnAllProducts()
    {
        // Arrange
        var category = Category.Create("Technology");
        category.Id = Guid.NewGuid();
        Context.Categories.Add(category);

        var product1 = Product.Create("Laptop", "Gaming laptop", 1299.99m, category.Id, 3);
        var product2 = Product.Create("Mouse", "Gaming mouse", 59.99m, category.Id, 15);
        
        Context.Products.AddRange(product1, product2);
        await Context.SaveChangesAsync();

        // Act
        var result = _repository.Query().ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Laptop");
        result.Should().Contain(p => p.Name == "Mouse");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProduct()
    {
        // Arrange
        var category = Category.Create("Clothing");
        category.Id = Guid.NewGuid();
        Context.Categories.Add(category);

        var product = Product.Create("T-Shirt", "Cotton T-shirt", 19.99m, category.Id, 20);
        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        // Act
        product.Update("Premium T-Shirt", 29.99m, category.Id, "Premium cotton T-shirt");
        _repository.Update(product);
        await Context.SaveChangesAsync();

        // Assert
        var updatedProduct = await Context.Products.FindAsync(product.Id);
        updatedProduct.Should().NotBeNull();
        updatedProduct!.Name.Should().Be("Premium T-Shirt");
        updatedProduct.Description.Should().Be("Premium cotton T-shirt");
        updatedProduct.Price.Value.Should().Be(29.99m);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProduct()
    {
        // Arrange
        var category = Category.Create("Home");
        category.Id = Guid.NewGuid();
        Context.Categories.Add(category);

        var product = Product.Create("Table", "Wooden table", 199.99m, category.Id, 2);
        Context.Products.Add(product);
        await Context.SaveChangesAsync();

        // Act
        _repository.Delete(product);
        await Context.SaveChangesAsync();

        // Assert
        var deletedProduct = await Context.Products.FindAsync(product.Id);
        deletedProduct.Should().BeNull();
    }
} 