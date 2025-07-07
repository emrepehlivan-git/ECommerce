using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ECommerce.Application.Common.Logging;
using ECommerce.Application.Services;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Configuration;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Services;

public sealed class CloudinaryService : ICloudinaryService, IScopedDependency
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly IECommerceLogger<CloudinaryService> _logger;

    public CloudinaryService(IOptions<CloudinarySettings> settings, IECommerceLogger<CloudinaryService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        
        var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
        _cloudinary = new Cloudinary(account) { Api = { Secure = _settings.Secure } };
    }

    public async Task<CloudinaryUploadResult> UploadImageAsync(Stream imageStream, string fileName, ImageType imageType, string? altText = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ValidateImageFile(imageStream, fileName, imageStream.Length))
            {
                return new CloudinaryUploadResult(
                    string.Empty, string.Empty, string.Empty, string.Empty, 0, 
                    string.Empty, 0, 0, false, "Invalid image file");
            }

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, imageStream),
                Folder = _settings.Upload.UploadFolder,
                PublicId = GeneratePublicId(fileName, imageType),
                UniqueFilename = _settings.Upload.UniqueFilename,
                Overwrite = _settings.Upload.OverwriteExisting,
                Context = new StringDictionary($"alt={altText}|type={imageType}"),
                Tags = $"product,{imageType.ToString().ToLower()}",
                EagerTransforms = GetEagerTransforms()
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

            if (uploadResult.Error != null)
            {
                _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                return new CloudinaryUploadResult(
                    string.Empty, string.Empty, string.Empty, string.Empty, 0,
                    string.Empty, 0, 0, false, uploadResult.Error.Message);
            }

            var thumbnailUrl = GenerateThumbnailUrl(uploadResult.PublicId);
            var largeUrl = GenerateLargeUrl(uploadResult.PublicId);

            _logger.LogInformation("Successfully uploaded image {PublicId} with size {Bytes} bytes", 
                uploadResult.PublicId, uploadResult.Bytes);

            return new CloudinaryUploadResult(
                uploadResult.PublicId,
                uploadResult.SecureUrl.ToString(),
                thumbnailUrl,
                largeUrl,
                uploadResult.Bytes,
                uploadResult.Format,
                uploadResult.Width,
                uploadResult.Height,
                true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image to Cloudinary");
            return new CloudinaryUploadResult(
                string.Empty, string.Empty, string.Empty, string.Empty, 0,
                string.Empty, 0, 0, false, ex.Message);
        }
    }

    public async Task<List<CloudinaryUploadResult>> UploadImagesAsync(IEnumerable<ImageUploadRequest> images, ImageType imageType, CancellationToken cancellationToken = default)
    {
        var results = new List<CloudinaryUploadResult>();
        var uploadTasks = images.Select(image => 
            UploadImageAsync(image.ImageStream, image.FileName, imageType, cancellationToken: cancellationToken));

        try
        {
            var uploadResults = await Task.WhenAll(uploadTasks);
            results.AddRange(uploadResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk image upload");
        }

        return results;
    }

    public async Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            
            if (result.Result == "ok")
            {
                _logger.LogInformation("Successfully deleted image {PublicId}", publicId);
                return true;
            }

            _logger.LogWarning("Failed to delete image {PublicId}: {Result}", publicId, result.Result);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image {PublicId}", publicId);
            return false;
        }
    }

    public async Task<bool> DeleteImagesAsync(IEnumerable<string> publicIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleteParams = new DelResParams
            {
                PublicIds = publicIds.ToList()
            };

            var result = await _cloudinary.DeleteResourcesAsync(deleteParams);
            var success = result.Deleted?.Count == publicIds.Count();
            
            if (success)
            {
                _logger.LogInformation("Successfully deleted {Count} images", publicIds.Count());
            }
            else
            {
                _logger.LogWarning("Partial deletion success for images. Expected: {Expected}, Deleted: {Actual}", 
                    publicIds.Count(), result.Deleted?.Count ?? 0);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting multiple images");
            return false;
        }
    }

    public string GenerateThumbnailUrl(string publicId)
    {
        var transformation = new Transformation()
            .Width(_settings.Transformations.Thumbnail.Width)
            .Height(_settings.Transformations.Thumbnail.Height)
            .Crop(_settings.Transformations.Thumbnail.Crop)
            .Quality(_settings.Transformations.Thumbnail.Quality)
            .FetchFormat(_settings.Transformations.Thumbnail.Format);

        return _cloudinary.Api.UrlImgUp.Transform(transformation).BuildUrl(publicId);
    }

    public string GenerateLargeUrl(string publicId)
    {
        var transformation = new Transformation()
            .Width(_settings.Transformations.Large.Width)
            .Height(_settings.Transformations.Large.Height)
            .Crop(_settings.Transformations.Large.Crop)
            .Quality(_settings.Transformations.Large.Quality)
            .FetchFormat(_settings.Transformations.Large.Format);

        return _cloudinary.Api.UrlImgUp.Transform(transformation).BuildUrl(publicId);
    }

    public string GenerateUrl(string publicId, int width, int height, int quality = 85)
    {
        var transformation = new Transformation()
            .Width(width)
            .Height(height)
            .Crop("fill")
            .Quality(quality)
            .FetchFormat("auto");

        return _cloudinary.Api.UrlImgUp.Transform(transformation).BuildUrl(publicId);
    }

    public bool ValidateImageFile(Stream imageStream, string fileName, long fileSizeBytes)
    {
        // Dosya boyutu kontrolü
        if (fileSizeBytes > _settings.Upload.MaxFileSizeBytes)
        {
            _logger.LogWarning("File size {Size} exceeds maximum allowed size {MaxSize}", 
                fileSizeBytes, _settings.Upload.MaxFileSizeBytes);
            return false;
        }

        // Dosya formatı kontrolü
        var extension = Path.GetExtension(fileName)?.TrimStart('.').ToLower();
        if (string.IsNullOrEmpty(extension) || !_settings.Upload.AllowedFormats.Contains(extension))
        {
            _logger.LogWarning("File format {Extension} is not allowed", extension);
            return false;
        }

        // Stream kontrolü
        if (imageStream == null || !imageStream.CanRead || imageStream.Length == 0)
        {
            _logger.LogWarning("Invalid image stream");
            return false;
        }

        return true;
    }

    public async Task<CloudinaryStorageInfo> GetStorageInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var usage = await _cloudinary.GetUsageAsync();
            
            return new CloudinaryStorageInfo(
                usage.Storage.Limit,
                usage.Storage.Used,
                usage.Storage.Limit - usage.Storage.Used,
                usage.Resources
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Cloudinary storage info");
            return new CloudinaryStorageInfo(0, 0, 0, 0);
        }
    }

    private string GeneratePublicId(string fileName, ImageType imageType)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return $"{imageType.ToString().ToLower()}_{nameWithoutExtension}_{timestamp}";
    }

    private List<Transformation> GetEagerTransforms()
    {
        return new List<Transformation>
        {
            // Thumbnail transformation
            new Transformation()
                .Width(_settings.Transformations.Thumbnail.Width)
                .Height(_settings.Transformations.Thumbnail.Height)
                .Crop(_settings.Transformations.Thumbnail.Crop)
                .Quality(_settings.Transformations.Thumbnail.Quality),
            
            // Large transformation
            new Transformation()
                .Width(_settings.Transformations.Large.Width)
                .Height(_settings.Transformations.Large.Height)
                .Crop(_settings.Transformations.Large.Crop)
                .Quality(_settings.Transformations.Large.Quality)
        };
    }
} 