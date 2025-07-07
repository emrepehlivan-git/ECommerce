using ECommerce.Application.Helpers;
using ECommerce.Application.Services;
using ECommerce.Domain.Enums;
using FluentValidation.TestHelper;

namespace ECommerce.Application.UnitTests.Features.Products.V1.Commands;

public class UploadProductImagesCommandTests
{
    private readonly Mock<IProductRepository> ProductRepositoryMock;
    private readonly Mock<IProductImageRepository> ProductImageRepositoryMock;
    private readonly Mock<ICloudinaryService> CloudinaryServiceMock;
    private readonly Mock<ILazyServiceProvider> LazyServiceProviderMock;
    private readonly Mock<LocalizationHelper> LocalizerMock;
    private readonly UploadProductImagesHandler Handler;
    private readonly UploadProductImagesValidator Validator;

    public UploadProductImagesCommandTests()
    {
        ProductRepositoryMock = new Mock<IProductRepository>();
        ProductImageRepositoryMock = new Mock<IProductImageRepository>();
        CloudinaryServiceMock = new Mock<ICloudinaryService>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizerMock = new Mock<LocalizationHelper>();

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<LocalizationHelper>())
            .Returns(LocalizerMock.Object);

        Handler = new UploadProductImagesHandler(
            ProductRepositoryMock.Object,
            ProductImageRepositoryMock.Object,
            CloudinaryServiceMock.Object,
            LazyServiceProviderMock.Object);

        Validator = new UploadProductImagesValidator(LocalizerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnSuccessResult()
    {
        var productId = Guid.NewGuid();
        var command = CreateValidCommand(productId);
        var product = Product.Create("Test Product", "Description", 100m, Guid.NewGuid(), 10);

        ProductRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, null, false, default))
            .ReturnsAsync(product);

        ProductImageRepositoryMock
            .Setup(x => x.GetActiveByProductIdAsync(productId, default))
            .ReturnsAsync(new List<ProductImage>());

        ProductImageRepositoryMock
            .Setup(x => x.GetNextDisplayOrderAsync(productId, default))
            .ReturnsAsync(1);

        var cloudinaryResult = new CloudinaryUploadResult(
            "test-public-id",
            "https://test-url.com/image.jpg",
            "https://test-url.com/thumb.jpg",
            "https://test-url.com/large.jpg",
            1024000,
            "jpg",
            800,
            600,
            true);

        CloudinaryServiceMock
            .Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<string>(), default))
            .ReturnsAsync(cloudinaryResult);

        ProductImageRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ProductImage>(), default))
            .ReturnsAsync((ProductImage img) => img);

        var result = await Handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().CloudinaryPublicId.Should().Be("test-public-id");

        CloudinaryServiceMock.Verify(x => x.UploadImageAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<ImageType>(),
            It.IsAny<string>(),
            default), Times.Once);

        ProductImageRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ProductImage>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldReturnNotFoundResult()
    {
        var productId = Guid.NewGuid();
        var command = CreateValidCommand(productId);

        ProductRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, null, false, default))
            .ReturnsAsync((Product?)null);

        LocalizerMock
            .Setup(x => x[ProductConsts.NotFound])
            .Returns("Product not found");

        var result = await Handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_MaxImagesExceeded_ShouldReturnErrorResult()
    {
        var productId = Guid.NewGuid();
        var command = CreateCommandWithMultipleImages(productId, 5);
        var product = Product.Create("Test Product", "Description", 100m, Guid.NewGuid(), 10);

        var existingImages = Enumerable.Range(0, ProductConsts.MaxImagesPerProduct - 2)
            .Select(_ => CreateTestProductImage(productId))
            .ToList();

        ProductRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, null, false, default))
            .ReturnsAsync(product);

        ProductImageRepositoryMock
            .Setup(x => x.GetActiveByProductIdAsync(productId, default))
            .ReturnsAsync(existingImages);

        LocalizerMock
            .Setup(x => x[ProductConsts.MaxImagesExceeded])
            .Returns("Maximum images exceeded");

        var result = await Handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public void Validator_ValidCommand_ShouldNotHaveValidationErrors()
    {
        var command = CreateValidCommand(Guid.NewGuid());

        var result = Validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_EmptyImagesList_ShouldHaveValidationError()
    {
        var command = new UploadProductImagesCommand(Guid.NewGuid(), new List<ProductImageUploadRequest>());

        LocalizerMock
            .Setup(x => x[ProductConsts.ImageNotFound])
            .Returns("Image not found");

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Images);
    }

    private static UploadProductImagesCommand CreateValidCommand(Guid productId)
    {
        var imageStream = new MemoryStream(new byte[1024]);
        var imageRequest = new ProductImageUploadRequest(
            imageStream,
            "test.jpg",
            ImageType.Main,
            1,
            "Test alt text");

        return new UploadProductImagesCommand(productId, new List<ProductImageUploadRequest> { imageRequest });
    }

    private static UploadProductImagesCommand CreateCommandWithMultipleImages(Guid productId, int imageCount)
    {
        var imageRequests = new List<ProductImageUploadRequest>();
        for (int i = 0; i < imageCount; i++)
        {
            var imageStream = new MemoryStream(new byte[1024]);
            var imageRequest = new ProductImageUploadRequest(
                imageStream,
                $"test{i}.jpg",
                ImageType.Gallery,
                i + 1);
            imageRequests.Add(imageRequest);
        }

        return new UploadProductImagesCommand(productId, imageRequests);
    }

    private static ProductImage CreateTestProductImage(Guid productId)
    {
        return ProductImage.Create(
            productId,
            $"public-id-{Guid.NewGuid()}",
            "https://test.com/image.jpg",
            "https://test.com/thumb.jpg",
            "https://test.com/large.jpg",
            1,
            ImageType.Gallery,
            1024000,
            "Test alt text");
    }
} 