namespace ECommerce.Domain.Entities;

public sealed class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Cart Cart { get; private set; } = null!;

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice => UnitPrice * Quantity;

    internal CartItem()
    {
    }

    private CartItem(Guid cartId, Guid productId, decimal unitPrice, int quantity)
    {
        Id = Guid.NewGuid();
        CartId = cartId;
        ProductId = productId;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public static CartItem Create(Guid cartId, Guid productId, decimal unitPrice, int quantity)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentException("Cart ID cannot be empty.", nameof(cartId));

        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        return new(cartId, productId, unitPrice, quantity);
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Quantity = quantity;
    }

    public void UpdateUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        UnitPrice = unitPrice;
    }
} 