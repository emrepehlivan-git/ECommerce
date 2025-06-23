using ECommerce.SharedKernel.Events;

namespace ECommerce.Domain.Events.Cart;

public sealed record CartItemRemovedEvent(
    Guid CartId,
    Guid ProductId) : IDomainEvent; 