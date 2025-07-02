using ECommerce.Application.Features.Products.V1;
using ECommerce.Application.Helpers;
using ECommerce.Application.Services;

namespace ECommerce.Application.UnitTests.Features.Stock.V1;

public abstract class StockTestBase
{
    protected readonly Mock<IStockRepository> StockRepositoryMock;
    protected readonly Mock<IProductRepository> ProductRepositoryMock;
    protected readonly Mock<ILazyServiceProvider> LazyServiceProviderMock;
    protected readonly Mock<ILocalizationService> LocalizationServiceMock;
    protected readonly LocalizationHelper Localizer;

    protected readonly ProductStock DefaultStock;
    protected readonly Product DefaultProduct;
    protected readonly Guid CategoryId = Guid.Parse("e64db34c-7455-41da-b255-a9a7a46ace54");

    protected StockTestBase()
    {
        DefaultProduct = Product.Create("Original Name", "Original Description", 100m, CategoryId, 10);
        StockRepositoryMock = new Mock<IStockRepository>();
        DefaultStock = ProductStock.Create(DefaultProduct.Id, 10);
        DefaultProduct.Stock = DefaultStock;
        ProductRepositoryMock = new Mock<IProductRepository>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizationServiceMock = new Mock<ILocalizationService>();
        Localizer = new LocalizationHelper(LocalizationServiceMock.Object);
        SetupLocalizationHelper();
        SetupDefaultLocalizationMessages();
    }

    protected void SetupLocalizationHelper()
    {
        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<LocalizationHelper>())
            .Returns(Localizer);
    }

    protected void SetupStockRepositoryReserveStock()
    {
        StockRepositoryMock
            .Setup(x => x.ReserveStockAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    protected void SetupStockRepositoryReleaseStock()
    {
        StockRepositoryMock
            .Setup(x => x.ReleaseStockAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    protected void SetupStockRepositoryGetById(ProductStock? stock = null)
    {
        StockRepositoryMock
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Expression<Func<IQueryable<ProductStock>, IQueryable<ProductStock>>>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock ?? DefaultStock);
    }

    protected void SetupProductRepositoryGetById(Product? product = null)
    {
        ProductRepositoryMock
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Expression<Func<IQueryable<Product>, IQueryable<Product>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
    }

    protected void SetupProductExists(bool exists = true)
    {
        ProductRepositoryMock
            .Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    protected void SetupDefaultLocalizationMessages()
    {
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(ProductConsts.NotFound))
            .Returns("Product not found.");

        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(ProductConsts.StockQuantityMustBeGreaterThanZero))
            .Returns("Stock quantity must be greater than zero.");
    }
}