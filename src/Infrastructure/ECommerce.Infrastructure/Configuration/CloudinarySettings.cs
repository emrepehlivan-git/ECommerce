namespace ECommerce.Infrastructure.Configuration;

public sealed class CloudinarySettings
{
    public const string SectionName = "Cloudinary";
    
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public bool Secure { get; set; } = true;
    
    // Image upload settings
    public ImageUploadSettings Upload { get; set; } = new();
    
    // Transformation settings
    public ImageTransformationSettings Transformations { get; set; } = new();
}

public sealed class ImageUploadSettings
{
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB
    public string[] AllowedFormats { get; set; } = ["jpg", "jpeg", "png", "webp"];
    public string UploadFolder { get; set; } = "ecommerce/products";
    public bool UniqueFilename { get; set; } = true;
    public bool OverwriteExisting { get; set; } = false;
    public int MaxImagesPerProduct { get; set; } = 10;
}

public sealed class ImageTransformationSettings
{
    public ImageSize Thumbnail { get; set; } = new() { Width = 150, Height = 150, Quality = 80 };
    public ImageSize Small { get; set; } = new() { Width = 300, Height = 300, Quality = 85 };
    public ImageSize Medium { get; set; } = new() { Width = 600, Height = 600, Quality = 90 };
    public ImageSize Large { get; set; } = new() { Width = 1200, Height = 1200, Quality = 95 };
}

public sealed class ImageSize
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Quality { get; set; } = 85;
    public string Crop { get; set; } = "fill";
    public string Format { get; set; } = "auto";
} 