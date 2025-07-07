using ECommerce.Application.Helpers;
using ECommerce.Domain.Enums;
using FluentValidation.TestHelper;

namespace ECommerce.Application.UnitTests.Features.Products.V1.Queries;

public class GetProductImagesQueryTests
{
    private readonly Mock<IProductRepository> ProductRepositoryMock;
    private readonly Mock<IProductImageRepository> ProductImageRepositoryMock;
    private readonly Mock<ILazyServiceProvider> LazyServiceProviderMock;
    private readonly Mock<LocalizationHelper> LocalizerMock;
    private readonly GetProductImagesQueryHandler Handler;
    private readonly GetProductImagesQueryValidator Validator;

    public GetProductImagesQueryTests()
    {
        ProductRepositoryMock = new Mock<IProductRepository>();
        ProductImageRepositoryMock = new Mock<IProductImageRepository>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizerMock = new Mock<LocalizationHelper>();

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<LocalizationHelper>())
            .Returns(LocalizerMock.Object);

        Handler = new GetProductImagesQueryHandler(
            ProductRepositoryMock.Object,
            ProductImageRepositoryMock.Object,
            LazyServiceProviderMock.Object);

        Validator = new GetProductImagesQueryValidator(
            ProductRepositoryMock.Object,
            LocalizerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnProductImages()
    {
        var productId = Guid.NewGuid();
        var query = new GetProductImagesQuery(productId, null, true);

        ProductRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), default))
            .ReturnsAsync(true);

        var productImages = new List<ProductImage>
        {
            ProductImage.Create(
                productId,
                "public-id-1",
                "https://test.com/image1.jpg",
                "https://test.com/thumb1.jpg",
                "https://test.com/large1.jpg",
                1,
                ImageType.Main,
                1024000,
                "Main image")
        };

        ProductImageRepositoryMock
            .Setup(x => x.GetActiveByProductIdAsync(productId, default))
            .ReturnsAsync(productImages);

        var result = await Handler.Handle(query, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Count.Should().Be(1);
        result.Value.First().CloudinaryPublicId.Should().Be("public-id-1");
    }

    [Fact]
    public async Task Handle_QueryWithImageTypeFilter_ShouldReturnFilteredImages()
    {
        var productId = Guid.NewGuid();
        var query = new GetProductImagesQuery(productId, ImageType.Gallery, true);

        ProductRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), default))
            .ReturnsAsync(true);

        var filteredImages = new List<ProductImage>
        {
            ProductImage.Create(
                productId,
                "public-id-gallery",
                "https://test.com/gallery.jpg",
                null,
                null,
                1,
                ImageType.Gallery,
                1024000,
                "Gallery image")
        };

        ProductImageRepositoryMock
            .Setup(x => x.GetByImageTypeAsync(productId, ImageType.Gallery, default))
            .ReturnsAsync(filteredImages);

        var result = await Handler.Handle(query, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Count.Should().Be(1);
        result.Value.First().ImageType.Should().Be(ImageType.Gallery);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldReturnNotFoundResult()
    {
        var productId = Guid.NewGuid();
        var query = new GetProductImagesQuery(productId, null, true);

        ProductRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), default))
            .ReturnsAsync(false);

        LocalizerMock
            .Setup(x => x[ProductConsts.NotFound])
            .Returns("Product not found");

        var result = await Handler.Handle(query, default);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public void Validator_ValidQuery_ShouldNotHaveValidationErrors()
    {
        var query = new GetProductImagesQuery(Guid.NewGuid(), null, true);

        ProductRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), default))
            .ReturnsAsync(true);

        var result = Validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_EmptyProductId_ShouldHaveValidationError()
    {
        var query = new GetProductImagesQuery(Guid.Empty, null, true);

        LocalizerMock
            .Setup(x => x[ProductConsts.NotFound])
            .Returns("Product not found");

        var result = Validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }
} 