using ECommerce.Domain.Events.Cart;

namespace ECommerce.Domain.Entities;

public sealed class Cart : AuditableEntity
{
    private readonly List<CartItem> _items = [];

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(item => item.TotalPrice);
    public int TotalItems => _items.Sum(item => item.Quantity);

    internal Cart()
    {
    }

    private Cart(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
    }

    public static Cart Create(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        return new(userId);
    }

    public void AddItem(Guid productId, decimal unitPrice, int quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var cartItem = CartItem.Create(Id, productId, unitPrice, quantity);
            _items.Add(cartItem);
            AddDomainEvent(new CartItemAddedEvent(Id, productId, quantity, unitPrice));
        }
    }

    public void RemoveItem(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            _items.Remove(item);
            AddDomainEvent(new CartItemRemovedEvent(Id, productId));
        }
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            item.UpdateQuantity(quantity);
        }
    }

    public void Clear()
    {
        if (_items.Any())
        {
            _items.Clear();
            AddDomainEvent(new CartClearedEvent(Id, UserId));
        }
    }

    public bool HasItem(Guid productId)
    {
        return _items.Any(i => i.ProductId == productId);
    }

    public CartItem? GetItem(Guid productId)
    {
        return _items.FirstOrDefault(i => i.ProductId == productId);
    }

    public bool IsEmpty => !_items.Any();
} 