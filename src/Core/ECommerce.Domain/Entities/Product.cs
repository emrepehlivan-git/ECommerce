using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Entities;

public sealed class Product : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; set; }
    public Price Price { get; private set; } = Price.Zero;
    public bool IsActive { get; private set; }
    public ProductStock Stock { get; set; } = null!;

    public Guid CategoryId { get; private set; }
    public Category Category { get; set; } = null!;

    internal Product()
    {
    }

    private Product(string name, string? description, decimal price, Guid categoryId, int initialStock)
    {
        SetName(name);
        SetDescription(description);
        Price = Price.Create(price);
        CategoryId = categoryId;
        IsActive = true;
        Stock = ProductStock.Create(Id, initialStock);
    }

    public static Product Create(string name, string? description, decimal price, Guid categoryId, int initialStock)
    {
        return new(name, description, price, categoryId, initialStock);
    }

    public void Update(string name, decimal price, Guid categoryId, string? description)
    {
        SetName(name);
        SetDescription(description);
        Price = Price.Create(price);
        CategoryId = categoryId;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative.", nameof(quantity));

        Stock.UpdateQuantity(quantity);
    }

    public bool HasSufficientStock(int requestedQuantity)
    {
        return Stock.Quantity >= requestedQuantity;
    }

    public bool IsOrderable(int requestedQuantity)
    {
        return IsActive && HasSufficientStock(requestedQuantity);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        if (name.Length < 3)
            throw new ArgumentException("Name cannot be less than 3 characters.", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Name cannot be longer than 100 characters.", nameof(name));

        Name = name;
    }

    private void SetDescription(string? description)
    {
        if (description != null && description.Length > 500)
            throw new ArgumentException("Description cannot be longer than 500 characters.", nameof(description));

        Description = description;
    }
}

