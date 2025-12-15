using ECommerce.Application.Services;
using ECommerce.Domain.ValueObjects;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Helpers;

namespace ECommerce.Application.UnitTests.Features.Orders.Queries;

public abstract class OrderQueriesTestBase
{
    protected readonly Mock<IOrderRepository> OrderRepositoryMock;
    protected readonly Mock<ICurrentUserService> CurrentUserServiceMock;
    protected readonly Mock<ILazyServiceProvider> LazyServiceProviderMock;
    protected readonly Mock<ILocalizationService> LocalizationServiceMock;
    protected readonly ILocalizationHelper Localizer;
    protected readonly Order DefaultOrder;
    protected readonly User DefaultUser;
    protected readonly Category DefaultCategory;
    protected readonly Product DefaultProduct;
    protected readonly Guid UserId = Guid.NewGuid();

    protected OrderQueriesTestBase()
    {
        OrderRepositoryMock = new Mock<IOrderRepository>();
        CurrentUserServiceMock = new Mock<ICurrentUserService>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizationServiceMock = new Mock<ILocalizationService>();

        Localizer = new LocalizationHelper(LocalizationServiceMock.Object);

        CurrentUserServiceMock.Setup(x => x.UserId).Returns(UserId.ToString());

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<ILocalizationHelper>())
            .Returns(Localizer);

        DefaultUser = User.Create("test@example.com", "Test User", "Password123!");
        DefaultCategory = Category.Create("Test Category");
        DefaultProduct = Product.Create("Test Product", "Test Description", 100m, DefaultCategory.Id, 10);
        DefaultProduct.Category = DefaultCategory;

        var shippingAddress = new Address("Test Shipping", "Istanbul", "34000", "Turkey");
        var billingAddress = new Address("Test Billing", "Istanbul", "34000", "Turkey");
        DefaultOrder = Order.Create(UserId, shippingAddress, billingAddress);

        SetupDefaultLocalizationMessages();
    }

    protected void SetupDefaultLocalizationMessages()
    {
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(OrderConsts.NotFound))
            .Returns("Order not found");
    }

    protected void SetupOrderRepositoryGetByIdAsync(Order? order = null)
    {
        OrderRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Expression<Func<IQueryable<Order>, IQueryable<Order>>>?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
    }
} 