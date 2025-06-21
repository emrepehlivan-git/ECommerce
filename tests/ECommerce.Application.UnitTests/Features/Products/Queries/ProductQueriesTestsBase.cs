using System;
using ECommerce.Application.Helpers;
using ECommerce.Application.Services;

namespace ECommerce.Application.UnitTests.Features.Products.Queries;

public abstract class ProductQueriesTestsBase
{
    protected readonly Mock<IProductRepository> ProductRepositoryMock;
    protected readonly Mock<ICategoryRepository> CategoryRepositoryMock;
    protected readonly Mock<ILazyServiceProvider> LazyServiceProviderMock;
    protected readonly Mock<ILocalizationService> LocalizationServiceMock;
    protected readonly Product DefaultProduct;
    protected readonly Category DefaultCategory;
    protected readonly LocalizationHelper Localizer;

    protected ProductQueriesTestsBase()
    {
        ProductRepositoryMock = new Mock<IProductRepository>();
        CategoryRepositoryMock = new Mock<ICategoryRepository>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizationServiceMock = new Mock<ILocalizationService>();

        Localizer = new LocalizationHelper(LocalizationServiceMock.Object);

        DefaultCategory = Category.Create("Test Category");
        DefaultProduct = Product.Create("Test Product", "Test Description", 100m, DefaultCategory.Id, 10);
        DefaultProduct.Category = DefaultCategory;

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<LocalizationHelper>())
            .Returns(Localizer);
    }

    protected void SetupProductExists(bool exists = true)
    {
        ProductRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(),
            It.IsAny<Expression<Func<IQueryable<Product>, IQueryable<Product>>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists ? DefaultProduct : null);
    }

    protected void SetupCategoryExists(bool exists = true)
    {
        CategoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(),
            It.IsAny<Expression<Func<IQueryable<Category>, IQueryable<Category>>>?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists ? DefaultCategory : null);
    }
}