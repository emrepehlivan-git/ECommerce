using ECommerce.Application.Features.Orders.V1;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Application.UnitTests.Features.Orders.Commands;

public abstract class OrderCommandsTestBase
{
    protected readonly Mock<IOrderRepository> OrderRepositoryMock;
    protected readonly Mock<IProductRepository> ProductRepositoryMock;
    protected readonly Mock<IOrderItemRepository> OrderItemRepositoryMock;
    protected readonly Mock<IStockRepository> StockRepositoryMock;
    protected readonly Mock<IUserAddressRepository> UserAddressRepositoryMock;
    protected readonly Mock<IUserService> UserServiceMock;
    protected readonly Mock<ILazyServiceProvider> LazyServiceProviderMock;
    protected readonly Mock<ILocalizationHelper> LocalizerMock;

    protected readonly Guid UserId = Guid.Parse("e64db34c-7455-41da-b255-a9a7a46ace54");
    protected readonly Order DefaultOrder;
    protected readonly Product DefaultProduct;
    protected readonly Category DefaultCategory;

    protected OrderCommandsTestBase()
    {
        OrderRepositoryMock = new Mock<IOrderRepository>();
        ProductRepositoryMock = new Mock<IProductRepository>();
        OrderItemRepositoryMock = new Mock<IOrderItemRepository>();
        StockRepositoryMock = new Mock<IStockRepository>();
        UserAddressRepositoryMock = new Mock<IUserAddressRepository>();
        UserServiceMock = new Mock<IUserService>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizerMock = new Mock<ILocalizationHelper>();

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<ILocalizationHelper>())
            .Returns(LocalizerMock.Object);

        SetupDefaultLocalizationMessages();

        DefaultCategory = Category.Create("Test Category");
        DefaultProduct = Product.Create("Test Product", "Test Description", 100m, DefaultCategory.Id, 10);
        DefaultProduct.Category = DefaultCategory;

        DefaultOrder = Order.Create(UserId, new Address("Test Shipping", "Istanbul", "34000", "Turkey"), new Address("Test Billing", "Istanbul", "34000", "Turkey"));
    }

    protected void SetupOrderRepositoryGetByIdAsync(Order? order = null)
    {
        OrderRepositoryMock
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Expression<Func<IQueryable<Order>, IQueryable<Order>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order ?? DefaultOrder);
    }

    protected void SetupDefaultLocalizationMessages()
    {
        LocalizerMock.Setup(x => x[OrderConsts.NotFound]).Returns("Order not found");
        LocalizerMock.Setup(x => x[OrderConsts.ProductNotFound]).Returns("Product not found");
        LocalizerMock.Setup(x => x[OrderConsts.OrderCannotBeModified]).Returns("Order cannot be modified");
        LocalizerMock.Setup(x => x[OrderConsts.OrderCannotBeCancelled]).Returns("Order cannot be cancelled");
        LocalizerMock.Setup(x => x[OrderConsts.QuantityMustBeGreaterThanZero]).Returns("Quantity must be greater than zero");
        LocalizerMock.Setup(x => x[OrderConsts.UserNotFound]).Returns("User not found");
        LocalizerMock.Setup(x => x[OrderConsts.OrderItemNotFound]).Returns("Order item not found");
        LocalizerMock.Setup(x => x[OrderConsts.ShippingAddressRequired]).Returns("Shipping address is required");
        LocalizerMock.Setup(x => x[OrderConsts.BillingAddressRequired]).Returns("Billing address is required");
        LocalizerMock.Setup(x => x[OrderConsts.EmptyOrder]).Returns("Order cannot be empty");
        LocalizerMock.Setup(x => x[OrderConsts.InsufficientStock]).Returns("Insufficient stock");
        LocalizerMock.Setup(x => x[OrderConsts.ProductNotActive]).Returns("Product not active");
        LocalizerMock.Setup(x => x[OrderConsts.ShippingAddressNotFound]).Returns("Shipping address not found");
        LocalizerMock.Setup(x => x[OrderConsts.BillingAddressNotFound]).Returns("Billing address not found");
    }
}
