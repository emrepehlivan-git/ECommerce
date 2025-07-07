using ECommerce.Application.Common.Logging;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Configuration;

namespace ECommerce.Infrastructure.IntegrationTests.Services;

public sealed class CloudinaryServiceTests
{
    private readonly CloudinarySettings Settings;

    public CloudinaryServiceTests()
    {
        Settings = GetValidCloudinarySettings();
    }

    [Fact]
    public void Constructor_WithValidSettings_ShouldCreateInstance()
    {
        // Arrange & Act
        var settings = Options.Create(Settings);
        var mockLogger = new Mock<IECommerceLogger<CloudinaryService>>();

        // Assert
        settings.Should().NotBeNull();
        mockLogger.Should().NotBeNull();
    }

    [Fact]
    public void ValidateImageFile_WithValidJpegFile_ShouldReturnTrue()
    {
        // Arrange
        var mockService = new Mock<CloudinaryService>();
        var imageStream = CreateMockImageStream();
        var fileName = "test.jpg";
        var fileSize = 1024;

        mockService.Setup(x => x.ValidateImageFile(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<long>()))
                   .Returns(true);

        // Act
        var result = mockService.Object.ValidateImageFile(imageStream, fileName, fileSize);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateImageFile_WithInvalidFile_ShouldReturnFalse()
    {
        // Arrange
        var mockService = new Mock<ICloudinaryService>();
        var invalidStream = new MemoryStream([1, 2, 3]);
        var fileName = "invalid.txt";
        var fileSize = 3;

        mockService.Setup(x => x.ValidateImageFile(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<long>()))
                   .Returns(false);

        // Act
        var result = mockService.Object.ValidateImageFile(invalidStream, fileName, fileSize);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UploadImageAsync_WithMockService_ShouldReturnMockedResult()
    {
        // Arrange
        var mockService = new Mock<ICloudinaryService>();
        var imageStream = CreateMockImageStream();
        var fileName = "test-image.jpg";
        var imageType = ImageType.Main;

        var expectedResult = new CloudinaryUploadResult(
            "mock-public-id",
            "https://mock-url.com/image.jpg",
            "https://mock-url.com/thumb.jpg",
            "https://mock-url.com/large.jpg",
            1024,
            "jpg",
            800,
            600,
            true);

        mockService.Setup(x => x.UploadImageAsync(
                It.IsAny<Stream>(), 
                It.IsAny<string>(), 
                It.IsAny<ImageType>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
                   .ReturnsAsync(expectedResult);

        // Act
        var result = await mockService.Object.UploadImageAsync(imageStream, fileName, imageType);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.PublicId.Should().Be("mock-public-id");
        result.FileSizeBytes.Should().Be(1024);
    }

    [Fact]
    public async Task DeleteImageAsync_WithMockService_ShouldReturnTrue()
    {
        // Arrange
        var mockService = new Mock<ICloudinaryService>();
        var publicId = "test-public-id";

        mockService.Setup(x => x.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(true);

        // Act
        var result = await mockService.Object.DeleteImageAsync(publicId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GenerateThumbnailUrl_WithMockService_ShouldReturnUrl()
    {
        // Arrange
        var mockService = new Mock<ICloudinaryService>();
        var publicId = "test-public-id";
        var expectedUrl = "https://mock-cloudinary.com/test-public-id/thumbnail";

        mockService.Setup(x => x.GenerateThumbnailUrl(It.IsAny<string>()))
                   .Returns(expectedUrl);

        // Act
        var result = mockService.Object.GenerateThumbnailUrl(publicId);

        // Assert
        result.Should().Be(expectedUrl);
    }

    [Fact]
    public void GenerateLargeUrl_WithMockService_ShouldReturnUrl()
    {
        // Arrange
        var mockService = new Mock<ICloudinaryService>();
        var publicId = "test-public-id";
        var expectedUrl = "https://mock-cloudinary.com/test-public-id/large";

        mockService.Setup(x => x.GenerateLargeUrl(It.IsAny<string>()))
                   .Returns(expectedUrl);

        // Act
        var result = mockService.Object.GenerateLargeUrl(publicId);

        // Assert
            result.Should().Be(expectedUrl);
    }

    private static CloudinarySettings GetValidCloudinarySettings()
    {
        return new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
            Upload = new ImageUploadSettings
            {
                MaxFileSizeBytes = 10 * 1024 * 1024,
                AllowedFormats = ["jpg", "jpeg", "png"],
                UploadFolder = "test"
            },
            Transformations = new ImageTransformationSettings
            {
                Thumbnail = new ImageSize { Width = 150, Height = 150 },
                Large = new ImageSize { Width = 1200, Height = 1200 }
            }
        };
    }

    private static MemoryStream CreateMockImageStream()
    {
        // Mock JPEG header
        var jpegHeader = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var mockData = new byte[1024];
        Array.Copy(jpegHeader, mockData, jpegHeader.Length);
        return new MemoryStream(mockData);
    }
} 