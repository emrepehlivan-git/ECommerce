using ECommerce.Domain.Entities;

namespace ECommerce.Domain.UnitTests.Entities;

public sealed class CartItemTests
{
    [Fact]
    public void Create_ShouldCreateCartItemWithValidParameters()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var unitPrice = 99.99m;
        var quantity = 2;

        // Act
        var cartItem = CartItem.Create(cartId, productId, unitPrice, quantity);

        // Assert
        cartItem.Should().NotBeNull();
        cartItem.Id.Should().NotBe(Guid.Empty);
        cartItem.CartId.Should().Be(cartId);
        cartItem.ProductId.Should().Be(productId);
        cartItem.UnitPrice.Should().Be(unitPrice);
        cartItem.Quantity.Should().Be(quantity);
        cartItem.TotalPrice.Should().Be(199.98m);
    }

    [Fact]
    public void Create_WithInvalidParameters_ShouldThrowArgumentException()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        // Act & Assert
        var actEmptyCartId = () => CartItem.Create(Guid.Empty, productId, 10m, 1);
        actEmptyCartId.Should().Throw<ArgumentException>()
            .WithMessage("Cart ID cannot be empty. (Parameter 'cartId')");

        var actEmptyProductId = () => CartItem.Create(cartId, Guid.Empty, 10m, 1);
        actEmptyProductId.Should().Throw<ArgumentException>()
            .WithMessage("Product ID cannot be empty. (Parameter 'productId')");

        var actNegativePrice = () => CartItem.Create(cartId, productId, -10m, 1);
        actNegativePrice.Should().Throw<ArgumentException>()
            .WithMessage("Unit price cannot be negative. (Parameter 'unitPrice')");

        var actZeroQuantity = () => CartItem.Create(cartId, productId, 10m, 0);
        actZeroQuantity.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero. (Parameter 'quantity')");

        var actNegativeQuantity = () => CartItem.Create(cartId, productId, 10m, -1);
        actNegativeQuantity.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero. (Parameter 'quantity')");
    }

    [Fact]
    public void UpdateQuantity_ShouldUpdateQuantityAndTotalPrice()
    {
        // Arrange
        var cartItem = CartItem.Create(Guid.NewGuid(), Guid.NewGuid(), 25m, 2);

        // Act
        cartItem.UpdateQuantity(5);

        // Assert
        cartItem.Quantity.Should().Be(5);
        cartItem.TotalPrice.Should().Be(125m);
    }

    [Fact]
    public void UpdateQuantity_WithInvalidQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var cartItem = CartItem.Create(Guid.NewGuid(), Guid.NewGuid(), 25m, 2);

        // Act & Assert
        var actZeroQuantity = () => cartItem.UpdateQuantity(0);
        actZeroQuantity.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero. (Parameter 'quantity')");

        var actNegativeQuantity = () => cartItem.UpdateQuantity(-1);
        actNegativeQuantity.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero. (Parameter 'quantity')");
    }

    [Fact]
    public void UpdateUnitPrice_ShouldUpdateUnitPriceAndTotalPrice()
    {
        // Arrange
        var cartItem = CartItem.Create(Guid.NewGuid(), Guid.NewGuid(), 25m, 2);

        // Act
        cartItem.UpdateUnitPrice(30m);

        // Assert
        cartItem.UnitPrice.Should().Be(30m);
        cartItem.TotalPrice.Should().Be(60m);
    }

    [Fact]
    public void UpdateUnitPrice_WithNegativePrice_ShouldThrowArgumentException()
    {
        // Arrange
        var cartItem = CartItem.Create(Guid.NewGuid(), Guid.NewGuid(), 25m, 2);

        // Act & Assert
        var act = () => cartItem.UpdateUnitPrice(-10m);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Unit price cannot be negative. (Parameter 'unitPrice')");
    }

    [Fact]
    public void TotalPrice_ShouldAlwaysBeCalculatedCorrectly()
    {
        // Arrange
        var cartItem = CartItem.Create(Guid.NewGuid(), Guid.NewGuid(), 12.5m, 3);

        // Assert
        cartItem.TotalPrice.Should().Be(37.5m);

        // Act - Update quantity
        cartItem.UpdateQuantity(4);

        // Assert
        cartItem.TotalPrice.Should().Be(50m);

        // Act - Update price
        cartItem.UpdateUnitPrice(15m);

        // Assert
        cartItem.TotalPrice.Should().Be(60m);
    }
} 