using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.Specifications;
 
namespace ECommerce.Application.UnitTests.Features.Carts.V1.Commands;
 
public abstract class CartCommandsTestBase
{
    protected readonly Guid DefaultUserId = Guid.Parse("047e47c3-1680-4118-9894-76fd3f3bb6c1");
    protected readonly Guid DefaultProductId = Guid.Parse("1fee6529-aed5-4808-b9b0-4e02d84f0f39");

    protected Mock<ICartRepository> CartRepositoryMock;
    protected Mock<IProductRepository> ProductRepositoryMock;
    protected Mock<ICurrentUserService> CurrentUserServiceMock;
    protected Mock<ILazyServiceProvider> LazyServiceProviderMock;
    protected Mock<ICacheManager> CacheManagerMock;

    protected Mock<ILocalizationHelper> LocalizerMock;

    protected CartCommandsTestBase()
    {
        CartRepositoryMock = new Mock<ICartRepository>();
        ProductRepositoryMock = new Mock<IProductRepository>();
        CurrentUserServiceMock = new Mock<ICurrentUserService>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        CacheManagerMock = new Mock<ICacheManager>();
        LocalizerMock = new Mock<ILocalizationHelper>();

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<ILocalizationHelper>())
            .Returns(LocalizerMock.Object);
            
        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<ICurrentUserService>())
            .Returns(CurrentUserServiceMock.Object);

        CurrentUserServiceMock.Setup(s => s.UserId).Returns(DefaultUserId.ToString());

        SetupDefaultLocalizationMessages();
    }

    private void SetupDefaultLocalizationMessages()
    {
        LocalizerMock.Setup(s => s[It.IsAny<string>()]).Returns<string>(key => key);
        LocalizerMock.Setup(s => s[It.IsAny<string>(), It.IsAny<string>()]).Returns<string, string>((key, lang) => key);
    }

    protected void SetupProductRepositoryGet(Product? product)
    {
        ProductRepositoryMock
            .Setup(r => r.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Expression<Func<IQueryable<Product>, IQueryable<Product>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
    }

    protected void SetupProductRepositoryListAsync(List<Product> products)
    {
        ProductRepositoryMock
            .Setup(r => r.ListAsync(
                It.IsAny<ISpecification<Product>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
    }

    protected void SetupProductRepositoryAnyAsync(bool exists)
    {
        ProductRepositoryMock
            .Setup(r => r.AnyAsync(
                It.IsAny<ISpecification<Product>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    protected void SetupCartRepositoryGet(Cart? cart)
    {
        CartRepositoryMock
            .Setup(r => r.GetByUserIdWithItemsAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);
    }

    protected static Product CreateTestProduct(int stock = 10, bool isActive = true, Guid? productId = null)
    {
        var product = Product.Create("Test Product", "Test Desc", 100m, Guid.NewGuid(), stock);
        if (productId.HasValue)
        {
            typeof(Product).BaseType!.GetProperty("Id")!.SetValue(product, productId.Value);
        }
        if (!isActive)
        {
            product.Deactivate();
        }
        return product;
    }
} 