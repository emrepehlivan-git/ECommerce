using ECommerce.Application.Features.Orders;
using ECommerce.Application.Helpers;
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
    protected readonly Mock<ILocalizationService> LocalizationServiceMock;
    protected readonly LocalizationHelper Localizer;

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
        LocalizationServiceMock = new Mock<ILocalizationService>();
        Localizer = new LocalizationHelper(LocalizationServiceMock.Object);

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<LocalizationHelper>())
            .Returns(Localizer);

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
        LocalizationServiceMock.Setup(x => x.GetLocalizedString(OrderConsts.NotFound)).Returns("Order not found");
        LocalizationServiceMock.Setup(x => x.GetLocalizedString(OrderConsts.ProductNotFound)).Returns("Product not found");
        LocalizationServiceMock.Setup(x => x.GetLocalizedString(OrderConsts.OrderCannotBeModified)).Returns("Order cannot be modified");
        LocalizationServiceMock.Setup(x => x.GetLocalizedString(OrderConsts.OrderCannotBeCancelled)).Returns("Order cannot be cancelled");
        LocalizationServiceMock.Setup(x => x.GetLocalizedString(OrderConsts.QuantityMustBeGreaterThanZero)).Returns("Quantity must be greater than zero");
        LocalizationServiceMock.Setup(x => x.GetLocalizedString(OrderConsts.UserNotFound)).Returns("User not found");
        LocalizationServiceMock.Setup(x => x.GetLocalizedString(OrderConsts.OrderItemNotFound)).Returns("Order item not found");
        LocalizationServiceMock.Setup(x => x.GetLocalizedString(OrderConsts.ShippingAddressRequired)).Returns("Shipping address is required");
        LocalizationServiceMock.Setup(x => x.GetLocalizedString(OrderConsts.BillingAddressRequired)).Returns("Billing address is required");
        LocalizationServiceMock.Setup(x => x.GetLocalizedString(OrderConsts.EmptyOrder)).Returns("Order cannot be empty");
        LocalizationServiceMock.Setup(x => x.GetLocalizedString(OrderConsts.InsufficientStock)).Returns("Insufficient stock");
        LocalizationServiceMock.Setup(x => x.GetLocalizedString(OrderConsts.ProductNotActive)).Returns("Product not active");
        LocalizationServiceMock.Setup(x => x.GetLocalizedString(OrderConsts.ShippingAddressNotFound)).Returns("Shipping address not found");
        LocalizationServiceMock.Setup(x => x.GetLocalizedString(OrderConsts.BillingAddressNotFound)).Returns("Billing address not found");
    }
}
