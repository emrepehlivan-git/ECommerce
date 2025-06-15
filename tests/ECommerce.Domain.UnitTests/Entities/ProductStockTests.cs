namespace ECommerce.Domain.UnitTests.Entities;

public sealed class ProductStockTests
{
    private readonly Guid _productId = new Guid("123e4567-e89b-12d3-a456-426614174000");
    private const int ValidQuantity = 10;

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Create_WithNegativeQuantity_ShouldThrowArgumentException(int quantity)
    {
        // Act
        var act = () => ProductStock.Create(_productId, quantity);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Stock quantity cannot be negative.*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(100)]
    public void Create_WithValidQuantity_ShouldCreateProductStock(int quantity)
    {
        // Act
        var stock = ProductStock.Create(_productId, quantity);

        // Assert
        stock.Should().NotBeNull();
        stock.ProductId.Should().Be(_productId);
        stock.Quantity.Should().Be(quantity);
    }

    [Fact]
    public void Create_ShouldInheritFromAuditableEntity()
    {
        // Act
        var stock = ProductStock.Create(_productId, ValidQuantity);

        // Assert
        stock.Should().BeAssignableTo<AuditableEntity>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void UpdateQuantity_WithNegativeQuantity_ShouldThrowArgumentException(int quantity)
    {
        // Arrange
        var stock = ProductStock.Create(_productId, ValidQuantity);

        // Act
        var act = () => stock.UpdateQuantity(quantity);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Stock quantity cannot be negative.*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(20)]
    public void UpdateQuantity_WithValidQuantity_ShouldUpdateQuantity(int newQuantity)
    {
        // Arrange
        var stock = ProductStock.Create(_productId, ValidQuantity);

        // Act
        stock.UpdateQuantity(newQuantity);

        // Assert
        stock.Quantity.Should().Be(newQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Reserve_WithInvalidQuantity_ShouldThrowArgumentException(int quantity)
    {
        // Arrange
        var stock = ProductStock.Create(_productId, ValidQuantity);

        // Act
        var act = () => stock.Reserve(quantity);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero.*");
    }

    [Fact]
    public void Reserve_WithInsufficientStock_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var stock = ProductStock.Create(_productId, 5);

        // Act
        var act = () => stock.Reserve(10);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Insufficient stock.");
    }

    [Theory]
    [InlineData(5, 3, 2)]
    [InlineData(10, 10, 0)]
    [InlineData(20, 5, 15)]
    public void Reserve_WithValidQuantity_ShouldReduceStock(int initialStock, int reserveQuantity, int expectedStock)
    {
        // Arrange
        var stock = ProductStock.Create(_productId, initialStock);

        // Act
        stock.Reserve(reserveQuantity);

        // Assert
        stock.Quantity.Should().Be(expectedStock);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Release_WithInvalidQuantity_ShouldThrowArgumentException(int quantity)
    {
        // Arrange
        var stock = ProductStock.Create(_productId, ValidQuantity);

        // Act
        var act = () => stock.Release(quantity);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero.*");
    }

    [Theory]
    [InlineData(5, 3, 8)]
    [InlineData(0, 10, 10)]
    [InlineData(15, 5, 20)]
    public void Release_WithValidQuantity_ShouldIncreaseStock(int initialStock, int releaseQuantity, int expectedStock)
    {
        // Arrange
        var stock = ProductStock.Create(_productId, initialStock);

        // Act
        stock.Release(releaseQuantity);

        // Assert
        stock.Quantity.Should().Be(expectedStock);
    }

    [Fact]
    public void ReserveAndRelease_ShouldMaintainStockBalance()
    {
        // Arrange
        var stock = ProductStock.Create(_productId, 10);

        // Act
        stock.Reserve(5);
        stock.Release(3);

        // Assert
        stock.Quantity.Should().Be(8);
    }

    [Fact]
    public void ProductId_ShouldBeReadOnly()
    {
        // Arrange
        var stock = ProductStock.Create(_productId, ValidQuantity);

        // Assert
        stock.ProductId.Should().Be(_productId);
        // ProductId should not have a public setter
        typeof(ProductStock).GetProperty(nameof(ProductStock.ProductId))?.SetMethod?.IsPublic.Should().BeFalse();
    }

    [Fact]
    public void Quantity_ShouldBeReadOnly()
    {
        // Arrange
        var stock = ProductStock.Create(_productId, ValidQuantity);

        // Assert
        stock.Quantity.Should().Be(ValidQuantity);
        // Quantity should not have a public setter
        typeof(ProductStock).GetProperty(nameof(ProductStock.Quantity))?.SetMethod?.IsPublic.Should().BeFalse();
    }
} 