using ECommerce.Domain.Enums;
using FluentValidation;
using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;

namespace ECommerce.Application.Features.Products.V1.DTOs;

/// <summary>
/// Image upload request DTO for API endpoints
/// </summary>
public sealed record ImageUploadDto(
    string FileName,
    string ContentType,
    long FileSizeBytes,
    ImageType ImageType,
    int DisplayOrder = 0,
    string? AltText = null
);

/// <summary>
/// Complete product image information
/// </summary>
public sealed record ProductImageDto(
    Guid Id,
    Guid ProductId,
    string CloudinaryPublicId,
    string ImageUrl,
    string? ThumbnailUrl,
    string? LargeUrl,
    ImageType ImageType,
    int DisplayOrder,
    bool IsActive,
    long FileSizeBytes,
    string? AltText,
    DateTime CreatedAt
);

/// <summary>
/// Simplified image info for listings
/// </summary>
public sealed record ProductImageSummaryDto(
    Guid Id,
    string ImageUrl,
    string? ThumbnailUrl,
    ImageType ImageType,
    int DisplayOrder,
    string? AltText
);

/// <summary>
/// Image reorder request
/// </summary>
public sealed record ImageReorderDto(
    Guid ImageId,
    int NewDisplayOrder
);

/// <summary>
/// Batch image upload response
/// </summary>
public sealed record ImageUploadResultDto(
    bool IsSuccessful,
    List<ProductImageSummaryDto> SuccessfulUploads,
    List<string> Errors
);

// Validators
public sealed class ImageUploadDtoValidator : AbstractValidator<ImageUploadDto>
{
    public ImageUploadDtoValidator(ILocalizationHelper localizer)
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage(localizer[ProductConsts.ImageNotFound])
            .Must(BeValidFileName)
            .WithMessage(localizer[ProductConsts.InvalidImageFormat]);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(BeValidImageContentType)
            .WithMessage(localizer[ProductConsts.InvalidImageFormat]);

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage(localizer[ProductConsts.ImageNotFound])
            .LessThanOrEqualTo(10 * 1024 * 1024) // 10MB
            .WithMessage(localizer[ProductConsts.ImageTooLarge]);

        RuleFor(x => x.ImageType)
            .IsInEnum()
            .WithMessage(localizer[ProductConsts.InvalidImageFormat]);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer[ProductConsts.ImageNotFound]);

        RuleFor(x => x.AltText)
            .MaximumLength(ProductConsts.AltTextMaxLength)
            .When(x => !string.IsNullOrEmpty(x.AltText))
            .WithMessage(localizer[ProductConsts.NormalizeMaxLength]);
    }

    private static bool BeValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return allowedExtensions.Contains(extension);
    }

    private static bool BeValidImageContentType(string contentType)
    {
        var allowedContentTypes = new[]
        {
            "image/jpeg",
            "image/jpg", 
            "image/png",
            "image/webp"
        };

        return allowedContentTypes.Contains(contentType.ToLowerInvariant());
    }
}

public sealed class ImageReorderDtoValidator : AbstractValidator<ImageReorderDto>
{
    public ImageReorderDtoValidator(ILocalizationHelper localizer)
    {
        RuleFor(x => x.ImageId)
            .NotEmpty()
            .WithMessage(localizer[ProductConsts.ImageNotFound]);

        RuleFor(x => x.NewDisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage(localizer[ProductConsts.ImageNotFound]);
    }
} 