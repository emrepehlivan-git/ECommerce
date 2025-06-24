using ECommerce.Application.Features.Orders.Commands;
using ECommerce.Domain.ValueObjects;
using ECommerce.SharedKernel.Specifications;
using FluentAssertions;

namespace ECommerce.Application.UnitTests.Features.Orders.Commands;

public sealed class OrderPlaceCommandTests : OrderCommandsTestBase
{
    private readonly OrderPlaceCommandHandler _handler;
    private readonly Address _defaultShippingAddress;
    private readonly Address _defaultBillingAddress;

    public OrderPlaceCommandTests()
    {
        _defaultShippingAddress = new Address("123 Main St", "New York", "10001", "USA");
        _defaultBillingAddress = new Address("456 Oak Ave", "New York", "10002", "USA");

        _handler = new OrderPlaceCommandHandler(
            OrderRepositoryMock.Object,
            ProductRepositoryMock.Object,
            UserAddressRepositoryMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithDirectAddresses_ShouldCreateOrder()
    {
        // Arrange
        var command = new OrderPlaceCommand(
            UserId,
            _defaultShippingAddress,
            _defaultBillingAddress,
            null,
            null,
            false,
            new List<OrderItemRequest>
            {
                new(DefaultProduct.Id, 2)
            });

        SetupProductRepository();
        SetupOrderRepository();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        OrderRepositoryMock.Verify(x => x.Add(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUserAddressIds_ShouldCreateOrder()
    {
        // Arrange
        var shippingAddressId = Guid.NewGuid();
        var billingAddressId = Guid.NewGuid();

        var command = new OrderPlaceCommand(
            UserId,
            null,
            null,
            shippingAddressId,
            billingAddressId,
            false,
            new List<OrderItemRequest>
            {
                new(DefaultProduct.Id, 1)
            });

        SetupProductRepository();
        SetupOrderRepository();
        SetupUserAddressRepository(shippingAddressId, billingAddressId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        OrderRepositoryMock.Verify(x => x.Add(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUseSameForBilling_ShouldCreateOrder()
    {
        // Arrange
        var shippingAddressId = Guid.NewGuid();

        var command = new OrderPlaceCommand(
            UserId,
            null,
            null,
            shippingAddressId,
            null,
            true,
            new List<OrderItemRequest>
            {
                new(DefaultProduct.Id, 1)
            });

        SetupProductRepository();
        SetupOrderRepository();
        SetupUserAddressRepository(shippingAddressId, shippingAddressId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        UserAddressRepositoryMock.Verify(x => x.GetByIdAsync(shippingAddressId, It.IsAny<Expression<Func<IQueryable<UserAddress>, IQueryable<UserAddress>>>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WithInsufficientStock_ShouldReturnError()
    {
        // Arrange
        var productWithLowStock = Product.Create("Low Stock Product", "Description", 100m, DefaultCategory.Id, 1);
        productWithLowStock.Stock.Reserve(1);

        var command = new OrderPlaceCommand(
            UserId,
            _defaultShippingAddress,
            _defaultBillingAddress,
            null,
            null,
            false,
            new List<OrderItemRequest>
            {
                new(productWithLowStock.Id, 2)
            });

        ProductRepositoryMock
            .Setup(x => x.ListAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { productWithLowStock });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Insufficient stock");
    }

    [Fact]
    public async Task Handle_WithInactiveProduct_ShouldReturnError()
    {
        // Arrange
        var inactiveProduct = Product.Create("Inactive Product", "Description", 100m, DefaultCategory.Id, 10);
        inactiveProduct.Deactivate();

        var command = new OrderPlaceCommand(
            UserId,
            _defaultShippingAddress,
            _defaultBillingAddress,
            null,
            null,
            false,
            new List<OrderItemRequest>
            {
                new(inactiveProduct.Id, 1)
            });

        ProductRepositoryMock
            .Setup(x => x.ListAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Product not found");
    }

    [Fact]
    public async Task Handle_WithNonExistentShippingAddress_ShouldReturnError()
    {
        // Arrange
        var command = new OrderPlaceCommand(
            UserId,
            null,
            null,
                Guid.NewGuid(), 
            Guid.NewGuid(),
            false,
            new List<OrderItemRequest>
            {
                new(DefaultProduct.Id, 1)
            });

        UserAddressRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Expression<Func<IQueryable<UserAddress>, IQueryable<UserAddress>>>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAddress?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Shipping address not found");
    }

    private void SetupProductRepository()
    {
        ProductRepositoryMock
            .Setup(x => x.ListAsync(It.IsAny<ISpecification<Product>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { DefaultProduct });
    }

    private void SetupOrderRepository()
    {
        OrderRepositoryMock
            .Setup(x => x.Add(It.IsAny<Order>()))
            .Verifiable();
    }

    private void SetupUserAddressRepository(Guid shippingAddressId, Guid billingAddressId)
    {
        var shippingUserAddress = UserAddress.Create(UserId, "Home", _defaultShippingAddress);
        var billingUserAddress = UserAddress.Create(UserId, "Work", _defaultBillingAddress);

        UserAddressRepositoryMock
            .Setup(x => x.GetByIdAsync(shippingAddressId, It.IsAny<Expression<Func<IQueryable<UserAddress>, IQueryable<UserAddress>>>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(shippingUserAddress);

        UserAddressRepositoryMock
            .Setup(x => x.GetByIdAsync(billingAddressId, It.IsAny<Expression<Func<IQueryable<UserAddress>, IQueryable<UserAddress>>>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(billingUserAddress);
    }
} 