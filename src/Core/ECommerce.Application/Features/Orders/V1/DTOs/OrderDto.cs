namespace ECommerce.Application.Features.Orders.V1.DTOs;

public sealed record OrderDto(
    Guid Id,
    Guid UserId,
    DateTime OrderDate,
    string Status,
    decimal TotalAmount,
    string? ShippingAddress,
    string? BillingAddress,
    IReadOnlyCollection<OrderItemDto> Items);