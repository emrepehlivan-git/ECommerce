namespace ECommerce.Application.Features.Products.V1.DTOs;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string? CategoryName,
    int StockQuantity,
    bool IsActive,
    List<ProductImageDto> Images);