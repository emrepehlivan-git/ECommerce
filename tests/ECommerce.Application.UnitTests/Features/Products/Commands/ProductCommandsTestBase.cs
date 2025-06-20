using ECommerce.Application.Features.Products;
using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;

namespace ECommerce.Application.UnitTests.Features.Products.Commands;

public class ProductCommandsTestBase
{
    protected Guid CategoryId = Guid.Parse("e64db34c-7455-41da-b255-a9a7a46ace54");
    protected Product DefaultProduct => Product.Create("Original Name", "Original Description", 100m, CategoryId, 10);

    protected Mock<IProductRepository> ProductRepositoryMock;
    protected Mock<ICategoryRepository> CategoryRepositoryMock;
        protected Mock<ILazyServiceProvider> LazyServiceProviderMock;
    protected Mock<ILocalizationService> LocalizationServiceMock;
    protected Mock<ICacheManager> CacheManagerMock;

    protected LocalizationHelper Localizer;

    protected ProductCommandsTestBase()
    {
        ProductRepositoryMock = new Mock<IProductRepository>();
        CategoryRepositoryMock = new Mock<ICategoryRepository>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizationServiceMock = new Mock<ILocalizationService>();
        CacheManagerMock = new Mock<ICacheManager>();
        Localizer = new LocalizationHelper(LocalizationServiceMock.Object);

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<LocalizationHelper>())
            .Returns(Localizer);

        SetupDefaultLocalizationMessages();
    }

    protected void SetupDefaultLocalizationMessages()
    {
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(ProductConsts.NameMustBeAtLeastCharacters))
            .Returns("Product name must be at least 3 characters long");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(ProductConsts.NameMustBeLessThanCharacters))
            .Returns("Product name must be less than 100 characters long");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(ProductConsts.PriceMustBeGreaterThanZero))
            .Returns("Product price must be greater than zero");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(ProductConsts.CategoryNotFound))
            .Returns("Product category not found");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(ProductConsts.NotFound))
            .Returns("Product not found.");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(ProductConsts.NameExists))
            .Returns("Product with this name already exists");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(ProductConsts.StockQuantityMustBeGreaterThanZero))
            .Returns("Stock quantity must be greater than or equal to zero");
    }

    protected void SetupProductRepositoryAdd(Product capturedProduct)
    {
        ProductRepositoryMock
            .Setup(x => x.Add(It.IsAny<Product>()))
            .Callback<Product>(product => capturedProduct = product);
    }

    protected void SetupCategoryExists(bool exists = true)
    {
        CategoryRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    protected void SetupProductExists(bool exists = true)
    {
        ProductRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    protected void SetupProductRepositoryGetByIdAsync(Product product)
    {
        ProductRepositoryMock
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Expression<Func<IQueryable<Product>, IQueryable<Product>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
    }

    protected void SetupLocalizedMessage(string message)
    {
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(message))
            .Returns(message);
    }

    protected CreateProductCommand CreateDefaultCommand(
        string name = "Test Product",
        string description = "Test Description",
        decimal price = 100m,
        int stockQuantity = 10)
    {
        return new CreateProductCommand(
            Name: name,
            Description: description,
            Price: price,
            CategoryId: CategoryId,
            StockQuantity: stockQuantity);
    }
}
