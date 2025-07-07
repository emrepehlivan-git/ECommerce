using ECommerce.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace ECommerce.WebAPI.DTOs;

public sealed record UploadProductImagesWebRequest(
    List<ProductImageWebDto> Images
);

public sealed record ProductImageWebDto(
    IFormFile File,
    ImageType ImageType,
    int DisplayOrder = 0,
    string? AltText = null
); 