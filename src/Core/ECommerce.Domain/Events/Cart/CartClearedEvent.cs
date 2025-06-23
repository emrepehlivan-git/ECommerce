using ECommerce.SharedKernel.Events;

namespace ECommerce.Domain.Events.Cart;

public sealed record CartClearedEvent(
    Guid CartId,
    Guid UserId) : IDomainEvent; 