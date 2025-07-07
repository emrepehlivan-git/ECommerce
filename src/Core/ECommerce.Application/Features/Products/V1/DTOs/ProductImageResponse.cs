using ECommerce.Domain.Enums;

namespace ECommerce.Application.Features.Products.V1.DTOs;

public sealed record ProductImageResponseDto(
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
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record UploadProductImagesResponse(
    List<ProductImageResponseDto> UploadedImages,
    int SuccessfulCount,
    int TotalCount,
    List<string> Errors
); 