using ECommerce.Application.Helpers;
using ECommerce.Application.Services;
using ECommerce.Domain.Enums;
using FluentValidation.TestHelper;

namespace ECommerce.Application.UnitTests.Features.Products.V1.Commands;

public class DeleteProductImageCommandTests
{
    private readonly Mock<IProductImageRepository> ProductImageRepositoryMock;
    private readonly Mock<ICloudinaryService> CloudinaryServiceMock;
    private readonly Mock<ILazyServiceProvider> LazyServiceProviderMock;
    private readonly Mock<LocalizationHelper> LocalizerMock;
    private readonly DeleteProductImageCommandHandler Handler;
    private readonly DeleteProductImageCommandValidator Validator;
    private readonly Mock<IProductRepository> ProductRepositoryMock;

    public DeleteProductImageCommandTests()
    {
        ProductImageRepositoryMock = new Mock<IProductImageRepository>();
        CloudinaryServiceMock = new Mock<ICloudinaryService>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizerMock = new Mock<LocalizationHelper>();
        ProductRepositoryMock = new Mock<IProductRepository>();

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<LocalizationHelper>())
            .Returns(LocalizerMock.Object);

        Handler = new DeleteProductImageCommandHandler(
            ProductImageRepositoryMock.Object,
            CloudinaryServiceMock.Object,
            LazyServiceProviderMock.Object);

        Validator = new DeleteProductImageCommandValidator(
            ProductRepositoryMock.Object,
            ProductImageRepositoryMock.Object,
            LocalizerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldDeleteImageSuccessfully()
    {
        var productId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var command = new DeleteProductImageCommand(productId, imageId);

        var productImage = ProductImage.Create(
            productId,
            "test-public-id",
            "https://test.com/image.jpg",
            "https://test.com/thumb.jpg",
            "https://test.com/large.jpg",
            1,
            ImageType.Main,
            1024000,
            "Test alt text");

        ProductImageRepositoryMock
            .Setup(x => x.GetByIdAsync(imageId, null, false, default))
            .ReturnsAsync(productImage);

        CloudinaryServiceMock
            .Setup(x => x.DeleteImageAsync("test-public-id", default))
            .ReturnsAsync(true);

        var result = await Handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();

        CloudinaryServiceMock.Verify(x => x.DeleteImageAsync("test-public-id", default), Times.Once);
        ProductImageRepositoryMock.Verify(x => x.Delete(productImage), Times.Once);
    }

    [Fact]
    public async Task Handle_ImageNotFound_ShouldReturnNotFoundResult()
    {
        var productId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var command = new DeleteProductImageCommand(productId, imageId);

        ProductImageRepositoryMock
            .Setup(x => x.GetByIdAsync(imageId, null, false, default))
            .ReturnsAsync((ProductImage?)null);

        LocalizerMock
            .Setup(x => x[ProductConsts.ImageNotFound])
            .Returns("Image not found");

        var result = await Handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);

        CloudinaryServiceMock.Verify(x => x.DeleteImageAsync(It.IsAny<string>(), default), Times.Never);
        ProductImageRepositoryMock.Verify(x => x.Delete(It.IsAny<ProductImage>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ImageBelongsToWrongProduct_ShouldReturnNotFoundResult()
    {
        var productId = Guid.NewGuid();
        var wrongProductId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var command = new DeleteProductImageCommand(productId, imageId);

        var productImage = ProductImage.Create(
            wrongProductId,
            "test-public-id",
            "https://test.com/image.jpg",
            null,
            null,
            1,
            ImageType.Main,
            1024000,
            null);

        ProductImageRepositoryMock
            .Setup(x => x.GetByIdAsync(imageId, null, false, default))
            .ReturnsAsync(productImage);

        LocalizerMock
            .Setup(x => x[ProductConsts.ImageNotFound])
            .Returns("Image not found");

        var result = await Handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);

        CloudinaryServiceMock.Verify(x => x.DeleteImageAsync(It.IsAny<string>(), default), Times.Never);
        ProductImageRepositoryMock.Verify(x => x.Delete(It.IsAny<ProductImage>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CloudinaryDeleteFails_ShouldReturnErrorResult()
    {
        var productId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var command = new DeleteProductImageCommand(productId, imageId);

        var productImage = ProductImage.Create(
            productId,
            "test-public-id",
            "https://test.com/image.jpg",
            null,
            null,
            1,
            ImageType.Main,
            1024000,
            null);

        ProductImageRepositoryMock
            .Setup(x => x.GetByIdAsync(imageId, null, false, default))
            .ReturnsAsync(productImage);

        CloudinaryServiceMock
            .Setup(x => x.DeleteImageAsync("test-public-id", default))
            .ReturnsAsync(false);

        LocalizerMock
            .Setup(x => x[ProductConsts.ImageDeleteFailed])
            .Returns("Image delete failed");

        var result = await Handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);

        CloudinaryServiceMock.Verify(x => x.DeleteImageAsync("test-public-id", default), Times.Once);
        ProductImageRepositoryMock.Verify(x => x.Delete(It.IsAny<ProductImage>()), Times.Never);
    }

    [Fact]
    public void Validator_ValidCommand_ShouldNotHaveValidationErrors()
    {
        var productId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var command = new DeleteProductImageCommand(productId, imageId);

        ProductRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), default))
            .ReturnsAsync(true);

        ProductImageRepositoryMock
            .Setup(x => x.GetByIdAsync(imageId, null, false, default))
            .ReturnsAsync(ProductImage.Create(
                productId,
                "test-public-id",
                "https://test.com/image.jpg",
                null,
                null,
                1,
                ImageType.Main,
                1024000,
                null));

        var result = Validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_EmptyProductId_ShouldHaveValidationError()
    {
        var command = new DeleteProductImageCommand(Guid.Empty, Guid.NewGuid());

        LocalizerMock
            .Setup(x => x[ProductConsts.NotFound])
            .Returns("Product not found");

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public void Validator_EmptyImageId_ShouldHaveValidationError()
    {
        var command = new DeleteProductImageCommand(Guid.NewGuid(), Guid.Empty);

        LocalizerMock
            .Setup(x => x[ProductConsts.ImageNotFound])
            .Returns("Image not found");

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ImageId);
    }
} 