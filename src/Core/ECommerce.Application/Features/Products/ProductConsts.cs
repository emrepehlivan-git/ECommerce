namespace ECommerce.Application.Features.Products;

public static class ProductConsts
{
    public const string NameExists = "Product:Name:Exists";
    public const string NotFound = "Product:NotFound";
    public const string NameIsRequired = "Product:Name:IsRequired";
    public const string NameMustBeAtLeastCharacters = "Product:Name:MustBeAtLeastCharacters";
    public const string NameMustBeLessThanCharacters = "Product:Name:MustBeLessThanCharacters";
    public const string PriceMustBeGreaterThanZero = "Product:Price:MustBeGreaterThanZero";
    public const string CategoryNotFound = "Product:Category:NotFound";
    public const string StockQuantityMustBeGreaterThanZero = "Product:StockQuantity:MustBeGreaterThanZero";
    
    // Image constants
    public const string ImageNotFound = "Product:Image:NotFound";
    public const string ImageUploadFailed = "Product:Image:UploadFailed";
    public const string ImageDeleteFailed = "Product:Image:DeleteFailed";
    public const string MaxImagesExceeded = "Product:Image:MaxImagesExceeded";
    public const string InvalidImageFormat = "Product:Image:InvalidFormat";
    public const string ImageTooLarge = "Product:Image:TooLarge";
    public const string NormalizeMaxLength = "Product:Image:NormalizeMaxLength";
    
    // Image validation messages
    public const string ImageFileNameRequired = "Product:Image:FileNameRequired";
    public const string ImageStreamRequired = "Product:Image:StreamRequired";
    public const string ImageDisplayOrderInvalid = "Product:Image:DisplayOrderInvalid";
    public const string ImageContentTypeInvalid = "Product:Image:ContentTypeInvalid";
    public const string AtLeastOneImageRequired = "Product:Image:AtLeastOneImageRequired";
    public const string ImageTypeRequired = "Product:Image:TypeRequired";
    public const string AltTextTooLong = "Product:Image:AltTextTooLong";
    public const string CloudinaryUploadError = "Product:Image:CloudinaryUploadError";
    public const string DatabaseSaveError = "Product:Image:DatabaseSaveError";
    public const string ImageCleanupFailed = "Product:Image:CleanupFailed";
    
    // Limits and constraints
    public const int NameMinLength = 3;
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 500;
    public const int MaxImagesPerProduct = 10;
    public const int AltTextMaxLength = 250;
    public const long MaxImageSizeBytes = 10 * 1024 * 1024; // 10MB
    public const int MinDisplayOrder = 0;
    public const int MaxDisplayOrder = 999;
}