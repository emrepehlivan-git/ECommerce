using ECommerce.SharedKernel.Events;

namespace ECommerce.Domain.Events.Cart;

public sealed record CartItemAddedEvent(
    Guid CartId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice) : IDomainEvent; 