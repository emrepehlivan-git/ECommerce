using System.Reflection;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;

namespace ECommerce.Application.UnitTests.Features.Carts.Queries;

public abstract class CartQueriesTestsBase
{
    protected readonly Mock<ICartRepository> CartRepositoryMock;
    protected readonly Mock<ICurrentUserService> CurrentUserServiceMock;
    protected readonly Mock<ILazyServiceProvider> LazyServiceProviderMock;
    protected readonly Cart DefaultCart;
    protected readonly Product DefaultProduct;
    protected readonly Category DefaultCategory;
    protected readonly User DefaultUser;
    protected readonly Mock<ILocalizationHelper> LocalizerMock;
    protected readonly Guid DefaultUserId = Guid.Parse("047e47c3-1680-4118-9894-76fd3f3bb6c1");

    protected CartQueriesTestsBase()
    {
        CartRepositoryMock = new Mock<ICartRepository>();
        CurrentUserServiceMock = new Mock<ICurrentUserService>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizerMock = new Mock<ILocalizationHelper>();

        LocalizerMock
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string key) => $"Test message for {key}");

        DefaultCategory = Category.Create("Test Category");
        // Set Category Id manually for testing
        var categoryIdProperty = typeof(Category).GetProperty("Id");
        categoryIdProperty?.SetValue(DefaultCategory, Guid.NewGuid());
        
        DefaultProduct = Product.Create("Test Product", "Test Description", 100m, DefaultCategory.Id, 10);
        // Set Product Id manually for testing  
        var productIdProperty = typeof(Product).GetProperty("Id");
        productIdProperty?.SetValue(DefaultProduct, Guid.NewGuid());
        DefaultProduct.Category = DefaultCategory;
        
        DefaultUser = User.Create("test@example.com", "Test User", "Test", new DateTime(1994, 1, 1));
        DefaultCart = Cart.Create(DefaultUserId);

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<ILocalizationHelper>())
            .Returns(LocalizerMock.Object);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(DefaultUserId.ToString());
    }

    protected void SetupCartExists(bool exists = true)
    {
        CartRepositoryMock
            .Setup(x => x.GetByUserIdWithItemsAsync(DefaultUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists ? DefaultCart : null);
    }

    protected void SetupCurrentUser(string? userId = null)
    {
        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId ?? DefaultUserId.ToString());
    }
} 