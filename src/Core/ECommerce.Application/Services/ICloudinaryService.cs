using ECommerce.Domain.Enums;

namespace ECommerce.Application.Services;

public interface ICloudinaryService
{
    Task<CloudinaryUploadResult> UploadImageAsync(Stream imageStream, string fileName, ImageType imageType, string? altText = null, CancellationToken cancellationToken = default);
    
    Task<List<CloudinaryUploadResult>> UploadImagesAsync(IEnumerable<ImageUploadRequest> images, ImageType imageType, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
    Task<bool> DeleteImagesAsync(IEnumerable<string> publicIds, CancellationToken cancellationToken = default);
    string GenerateThumbnailUrl(string publicId);
    string GenerateLargeUrl(string publicId);
    string GenerateUrl(string publicId, int width, int height, int quality = 85);
    bool ValidateImageFile(Stream imageStream, string fileName, long fileSizeBytes);
    Task<CloudinaryStorageInfo> GetStorageInfoAsync(CancellationToken cancellationToken = default);
    Task DeleteAllImagesAsync(CancellationToken cancellationToken = default);
}

public sealed record ImageUploadRequest(
    Stream ImageStream,
    string FileName,
    long FileSizeBytes);

public sealed record CloudinaryUploadResult(
    string PublicId,
    string SecureUrl,
    string ThumbnailUrl,
    string LargeUrl,
    long FileSizeBytes,
    string Format,
    int Width,
    int Height,
    bool IsSuccessful,
    string? ErrorMessage = null);

public sealed record CloudinaryStorageInfo(
    long TotalBytes,
    long UsedBytes,
    long AvailableBytes,
    int TotalImages); 