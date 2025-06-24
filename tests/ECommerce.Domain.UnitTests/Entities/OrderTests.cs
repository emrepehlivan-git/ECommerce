namespace ECommerce.Domain.UnitTests.Entities;

public sealed class OrderTests
{
    private readonly Guid _userId = new Guid("ef40322b-1946-472c-97b3-7e90e401c872");
    private static readonly Address ValidShippingAddress = new Address("123 Shipping St", "Istanbul", "34000", "Turkey");
    private static readonly Address ValidBillingAddress = new Address("123 Billing St", "Istanbul", "34000", "Turkey");
    private readonly Guid _productId = new Guid("8932467a-3c87-49c9-9726-d9f96ff5c1b2");
    private readonly Price _unitPrice = Price.Create(100m);
    private const int ValidQuantity = 2;

    [Fact]
    public void Create_WithValidParameters_ShouldCreateOrder()
    {
        // Act
        var order = Order.Create(_userId, ValidShippingAddress, ValidBillingAddress);

        // Assert
        order.Should().NotBeNull();
        order.UserId.Should().Be(_userId);
        order.OrderDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        order.Status.Should().Be(OrderStatus.Pending);
        order.TotalAmount.Should().Be(0);
        order.ShippingAddress.Should().Be(ValidShippingAddress);
        order.BillingAddress.Should().Be(ValidBillingAddress);
        order.Items.Should().NotBeNull();
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void AddItem_WithNewItem_ShouldAddItemAndRecalculateTotal()
    {
        // Arrange
        var order = Order.Create(_userId, ValidShippingAddress, ValidBillingAddress);

        // Act
        order.AddItem(_productId, _unitPrice, ValidQuantity);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items.First().ProductId.Should().Be(_productId);
        order.Items.First().Quantity.Should().Be(ValidQuantity);
        order.TotalAmount.Should().Be(200m); // 100 * 2
    }

    [Fact]
    public void AddItem_WithExistingItem_ShouldUpdateQuantityAndRecalculateTotal()
    {
        // Arrange
        var order = Order.Create(_userId, ValidShippingAddress, ValidBillingAddress);
        order.AddItem(_productId, _unitPrice, ValidQuantity);

        // Act
        order.AddItem(_productId, _unitPrice, ValidQuantity);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items.First().Quantity.Should().Be(4); // 2 + 2
        order.TotalAmount.Should().Be(400m); // 100 * 4
    }

    [Fact]
    public void RemoveItem_WithExistingItem_ShouldRemoveItemAndRecalculateTotal()
    {
        // Arrange
        var order = Order.Create(_userId, ValidShippingAddress, ValidBillingAddress);
        order.AddItem(_productId, _unitPrice, ValidQuantity);

        // Act
        order.RemoveItem(_productId);

        // Assert
        order.Items.Should().BeEmpty();
        order.TotalAmount.Should().Be(0);
    }

    [Fact]
    public void RemoveItem_WithNonExistingItem_ShouldNotChangeOrder()
    {
        // Arrange
        var order = Order.Create(_userId, ValidShippingAddress, ValidBillingAddress);
        order.AddItem(_productId, _unitPrice, ValidQuantity);
        var nonExistingProductId = new Guid("bf9e6eff-f59a-4bbb-9007-59755e20dc2d");

        // Act
        order.RemoveItem(nonExistingProductId);

        // Assert
        order.Items.Should().HaveCount(1);
        order.TotalAmount.Should().Be(200m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateItemQuantity_WithInvalidQuantity_ShouldRemoveItem(int quantity)
    {
        // Arrange
        var order = Order.Create(_userId, ValidShippingAddress, ValidBillingAddress);
        order.AddItem(_productId, _unitPrice, ValidQuantity);

        // Act
        order.UpdateItemQuantity(_productId, quantity);

        // Assert
        order.Items.Should().BeEmpty();
        order.TotalAmount.Should().Be(0);
    }

    [Fact]
    public void UpdateItemQuantity_WithValidQuantity_ShouldUpdateQuantityAndRecalculateTotal()
    {
        // Arrange
        var order = Order.Create(_userId, ValidShippingAddress, ValidBillingAddress);
        order.AddItem(_productId, _unitPrice, ValidQuantity);

        // Act
        order.UpdateItemQuantity(_productId, 3);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items.First().Quantity.Should().Be(3);
        order.TotalAmount.Should().Be(300m); // 100 * 3
    }

    [Fact]
    public void UpdateStatus_ShouldUpdateOrderStatus()
    {
        // Arrange
        var order = Order.Create(_userId, ValidShippingAddress, ValidBillingAddress);

        // Act
        order.UpdateStatus(OrderStatus.Processing);

        // Assert
        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void UpdateAddresses_ShouldUpdateAddresses()
    {
        // Arrange
        var order = Order.Create(_userId, ValidShippingAddress, ValidBillingAddress);
        var newShippingAddress = new Address("456 New Shipping St", "Ankara", "06000", "Turkey");
        var newBillingAddress = new Address("456 New Billing St", "Ankara", "06000", "Turkey");

        // Act
        order.UpdateAddresses(newShippingAddress, newBillingAddress);

        // Assert
        order.ShippingAddress.Should().Be(newShippingAddress);
        order.BillingAddress.Should().Be(newBillingAddress);
    }
}