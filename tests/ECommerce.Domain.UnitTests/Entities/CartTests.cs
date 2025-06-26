using ECommerce.Domain.Events.Cart;

namespace ECommerce.Domain.UnitTests.Entities;

public sealed class CartTests
{
    [Fact]
    public void Create_ShouldCreateCartWithValidUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var cart = Cart.Create(userId);

        // Assert
        cart.Should().NotBeNull();
        cart.Id.Should().NotBe(Guid.Empty);
        cart.UserId.Should().Be(userId);
        cart.Items.Should().BeEmpty();
        cart.TotalAmount.Should().Be(0);
        cart.TotalItems.Should().Be(0);
        cart.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act = () => Cart.Create(Guid.Empty);
        act.Should().Throw<ArgumentException>()
            .WithMessage("User ID cannot be empty. (Parameter 'userId')");
    }

    [Fact]
    public void AddItem_ShouldAddNewItemToCart()
    {
        // Arrange
        var cart = Cart.Create(Guid.NewGuid());
        var productId = Guid.NewGuid();

        // Act
        cart.AddItem(productId, 99.99m, 2);

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.TotalItems.Should().Be(1);
        cart.TotalAmount.Should().Be(199.98m);
        cart.IsEmpty.Should().BeFalse();
        cart.HasItem(productId).Should().BeTrue();

        var item = cart.GetItem(productId);
        item.Should().NotBeNull();
        item!.ProductId.Should().Be(productId);
        item.UnitPrice.Should().Be(99.99m);
        item.Quantity.Should().Be(2);
        item.TotalPrice.Should().Be(199.98m);

        cart.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CartItemAddedEvent>();
    }

    [Fact]
    public void AddItem_WithExistingProduct_ShouldUpdateQuantity()
    {
        // Arrange
        var cart = Cart.Create(Guid.NewGuid());
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 1);

        // Act
        cart.AddItem(productId, 50m, 2);

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.TotalItems.Should().Be(1);
        cart.TotalAmount.Should().Be(150m);

        var item = cart.GetItem(productId);
        item!.Quantity.Should().Be(3);
        item.TotalPrice.Should().Be(150m);

        // Only one event should be raised (from first AddItem)
        cart.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_WithInvalidParameters_ShouldThrowArgumentException()
    {
        // Arrange
        var cart = Cart.Create(Guid.NewGuid());

        // Act & Assert
        var actEmptyProductId = () => cart.AddItem(Guid.Empty, 10m, 1);
        actEmptyProductId.Should().Throw<ArgumentException>()
            .WithMessage("Product ID cannot be empty. (Parameter 'productId')");

        var actNegativePrice = () => cart.AddItem(Guid.NewGuid(), -10m, 1);
        actNegativePrice.Should().Throw<ArgumentException>()
            .WithMessage("Unit price cannot be negative. (Parameter 'unitPrice')");

        var actZeroQuantity = () => cart.AddItem(Guid.NewGuid(), 10m, 0);
        actZeroQuantity.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero. (Parameter 'quantity')");
    }

    [Fact]
    public void RemoveItem_ShouldRemoveItemFromCart()
    {
        // Arrange
        var cart = Cart.Create(Guid.NewGuid());
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);

        // Act
        cart.RemoveItem(productId);

        // Assert
        cart.Items.Should().BeEmpty();
        cart.TotalItems.Should().Be(0);
        cart.TotalAmount.Should().Be(0);
        cart.IsEmpty.Should().BeTrue();
        cart.HasItem(productId).Should().BeFalse();
        cart.GetItem(productId).Should().BeNull();

        cart.DomainEvents.Should().HaveCount(2);
        cart.DomainEvents.Should().Contain(e => e is CartItemRemovedEvent);
    }

    [Fact]
    public void RemoveItem_WithNonExistentProduct_ShouldNotThrow()
    {
        // Arrange
        var cart = Cart.Create(Guid.NewGuid());
        var productId = Guid.NewGuid();

        // Act & Assert
        var act = () => cart.RemoveItem(productId);
        act.Should().NotThrow();

        cart.Items.Should().BeEmpty();
        cart.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateItemQuantity_ShouldUpdateQuantity()
    {
        // Arrange
        var cart = Cart.Create(Guid.NewGuid());
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 25m, 2);

        // Act
        cart.UpdateItemQuantity(productId, 5);

        // Assert
        cart.TotalItems.Should().Be(1);
        cart.TotalAmount.Should().Be(125m);

        var item = cart.GetItem(productId);
        item!.Quantity.Should().Be(5);
        item.TotalPrice.Should().Be(125m);
    }

    [Fact]
    public void UpdateItemQuantity_WithNonExistentProduct_ShouldNotThrow()
    {
        // Arrange
        var cart = Cart.Create(Guid.NewGuid());

        // Act & Assert
        var act = () => cart.UpdateItemQuantity(Guid.NewGuid(), 5);
        act.Should().NotThrow();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItemsAndRaiseDomainEvent()
    {
        // Arrange
        var cart = Cart.Create(Guid.NewGuid());
        cart.AddItem(Guid.NewGuid(), 10m, 1);
        cart.AddItem(Guid.NewGuid(), 20m, 2);

        // Act
        cart.Clear();

        // Assert
        cart.Items.Should().BeEmpty();
        cart.TotalItems.Should().Be(0);
        cart.TotalAmount.Should().Be(0);
        cart.IsEmpty.Should().BeTrue();

        cart.DomainEvents.Should().Contain(e => e is CartClearedEvent);
    }

    [Fact]
    public void Clear_WithEmptyCart_ShouldNotRaiseDomainEvent()
    {
        // Arrange
        var cart = Cart.Create(Guid.NewGuid());

        // Act
        cart.Clear();

        // Assert
        cart.Items.Should().BeEmpty();
        cart.DomainEvents.Should().BeEmpty();
    }
} 