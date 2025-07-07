using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public sealed class ProductImage : AuditableEntity
{
    public Guid ProductId { get; private set; }
    public string CloudinaryPublicId { get; private set; } = string.Empty;
    public string ImageUrl { get; private set; } = string.Empty;
    public string? ThumbnailUrl { get; private set; }
    public string? LargeUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public ImageType ImageType { get; private set; }
    public bool IsActive { get; private set; }
    public long FileSizeBytes { get; private set; }
    public string? AltText { get; private set; }

    public Product Product { get; set; } = null!;

    internal ProductImage()
    {
    }

    private ProductImage(
        Guid productId, 
        string cloudinaryPublicId, 
        string imageUrl, 
        string? thumbnailUrl, 
        string? largeUrl,
        int displayOrder, 
        ImageType imageType,
        long fileSizeBytes,
        string? altText)
    {
        ProductId = productId;
        SetCloudinaryData(cloudinaryPublicId, imageUrl, thumbnailUrl, largeUrl, fileSizeBytes);
        SetDisplayOrder(displayOrder);
        ImageType = imageType;
        IsActive = true;
        AltText = altText;
    }

    public static ProductImage Create(
        Guid productId, 
        string cloudinaryPublicId, 
        string imageUrl, 
        string? thumbnailUrl, 
        string? largeUrl,
        int displayOrder, 
        ImageType imageType,
        long fileSizeBytes,
        string? altText = null)
    {
        return new ProductImage(productId, cloudinaryPublicId, imageUrl, thumbnailUrl, largeUrl, displayOrder, imageType, fileSizeBytes, altText);
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        SetDisplayOrder(displayOrder);
    }

    public void UpdateImageType(ImageType imageType)
    {
        ImageType = imageType;
    }

    public void UpdateUrls(string imageUrl, string? thumbnailUrl, string? largeUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be null or empty.", nameof(imageUrl));

        ImageUrl = imageUrl;
        ThumbnailUrl = thumbnailUrl;
        LargeUrl = largeUrl;
    }

    public void UpdateAltText(string? altText)
    {
        AltText = altText;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private void SetCloudinaryData(string cloudinaryPublicId, string imageUrl, string? thumbnailUrl, string? largeUrl, long fileSizeBytes)
    {
        if (string.IsNullOrWhiteSpace(cloudinaryPublicId))
            throw new ArgumentException("Cloudinary Public ID cannot be null or empty.", nameof(cloudinaryPublicId));

        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be null or empty.", nameof(imageUrl));

        if (fileSizeBytes <= 0)
            throw new ArgumentException("File size must be greater than zero.", nameof(fileSizeBytes));

        CloudinaryPublicId = cloudinaryPublicId;
        ImageUrl = imageUrl;
        ThumbnailUrl = thumbnailUrl;
        LargeUrl = largeUrl;
        FileSizeBytes = fileSizeBytes;
    }

    private void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new ArgumentException("Display order cannot be negative.", nameof(displayOrder));

        DisplayOrder = displayOrder;
    }
} 