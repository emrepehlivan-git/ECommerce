namespace ECommerce.Application.Features.Carts.V1.DTOs;

public sealed record CartDto(
    Guid Id,
    Guid UserId,
    List<CartItemDto> Items,
    decimal TotalAmount,
    int TotalItems,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public CartDto() : this(
        Id: Guid.Empty,
        UserId: Guid.Empty,
        Items: [],
        TotalAmount: 0,
        TotalItems: 0,
        CreatedAt: DateTime.UtcNow,
        UpdatedAt: null)
    {
    }
}

public sealed record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

public sealed record CartSummaryDto(
    Guid Id,
    int TotalItems,
    decimal TotalAmount); 