using ECommerce.Application.Interfaces;
using ECommerce.Domain.Enums;
using FluentValidation.TestHelper;

namespace ECommerce.Application.UnitTests.Features.Products.V1.Queries;

public class GetProductImagesQueryTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IProductImageRepository> _productImageRepositoryMock;
    private readonly Mock<ILazyServiceProvider> _lazyServiceProviderMock;
    private readonly Mock<ILocalizationHelper> _localizerMock;
    private readonly GetProductImagesQueryHandler _handler;
    private readonly GetProductImagesQueryValidator _validator;

    public GetProductImagesQueryTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _productImageRepositoryMock = new Mock<IProductImageRepository>();
        _lazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        _localizerMock = new Mock<ILocalizationHelper>();

        _localizerMock.Setup(x => x[It.IsAny<string>()]).Returns("some-string");

        _lazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<ILocalizationHelper>())
            .Returns(_localizerMock.Object);

        _handler = new GetProductImagesQueryHandler(
            _productRepositoryMock.Object,
            _productImageRepositoryMock.Object,
            _lazyServiceProviderMock.Object);

        _validator = new GetProductImagesQueryValidator(
            _productRepositoryMock.Object,
            _localizerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnProductImages()
    {
        var productId = Guid.NewGuid();
        var query = new GetProductImagesQuery(productId, null, true);

        _productRepositoryMock
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

        _productImageRepositoryMock
            .Setup(x => x.GetActiveByProductIdAsync(productId, default))
            .ReturnsAsync(productImages);

        var result = await _handler.Handle(query, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Count.Should().Be(1);
        result.Value.First().CloudinaryPublicId.Should().Be("public-id-1");
    }

    [Fact]
    public async Task Handle_QueryWithImageTypeFilter_ShouldReturnFilteredImages()
    {
        var productId = Guid.NewGuid();
        var query = new GetProductImagesQuery(productId, ImageType.Gallery, true);

        _productRepositoryMock
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

        _productImageRepositoryMock
            .Setup(x => x.GetByImageTypeAsync(productId, ImageType.Gallery, default))
            .ReturnsAsync(filteredImages);

        var result = await _handler.Handle(query, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Count.Should().Be(1);
        result.Value.First().ImageType.Should().Be(ImageType.Gallery);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldReturnNotFoundResult()
    {
        var productId = Guid.NewGuid();
        var query = new GetProductImagesQuery(productId, null, true);

        _productRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), default))
            .ReturnsAsync(false);

        _localizerMock
            .Setup(x => x[ProductConsts.NotFound])
            .Returns("Product not found");

        var result = await _handler.Handle(query, default);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Validator_ValidQuery_ShouldNotHaveValidationErrors()
    {
        var query = new GetProductImagesQuery(Guid.NewGuid(), null, true);

        _productRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), default))
            .ReturnsAsync(true);

        var result = await _validator.TestValidateAsync(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validator_EmptyProductId_ShouldHaveValidationError()
    {
        var query = new GetProductImagesQuery(Guid.Empty, null, true);

        _localizerMock
            .Setup(x => x[ProductConsts.NotFound])
            .Returns("Product not found");

        var result = await _validator.TestValidateAsync(query);

        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }
} 