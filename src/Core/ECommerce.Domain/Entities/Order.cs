using ECommerce.Domain.Enums;
using ECommerce.Domain.Events.Stock;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Entities;

public sealed class Order : AuditableEntity
{
    private readonly List<OrderItem> _items = [];


    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }

    public Address ShippingAddress { get; private set; } = null!;
    public Address BillingAddress { get; private set; } = null!;

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    internal Order()
    {
    }

    private Order(Guid userId, Address shippingAddress, Address billingAddress)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        TotalAmount = 0;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
    }

    public static Order Create(Guid userId, Address shippingAddress, Address billingAddress)
    {
        return new(userId, shippingAddress, billingAddress);
    }

    public void AddItem(Guid productId, Price unitPrice, int quantity)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem is not null)
        {
            var oldQuantity = existingItem.Quantity;
            existingItem.UpdateQuantity(oldQuantity + quantity);
            AddDomainEvent(new StockReservedEvent(productId, quantity));
        }
        else
        {
            var orderItem = OrderItem.Create(Id, productId, unitPrice.Value, quantity);
            _items.Add(orderItem);
            AddDomainEvent(new StockReservedEvent(productId, quantity));
        }

        RecalculateTotalAmount();
    }


    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);

        if (item is not null)
        {
            _items.Remove(item);
            AddDomainEvent(new StockNotReservedEvent(productId, item.Quantity));
            RecalculateTotalAmount();
        }
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);

        if (item is not null)
        {
            var oldQuantity = item.Quantity;

            if (quantity <= 0)
            {
                _items.Remove(item);
                AddDomainEvent(new StockNotReservedEvent(productId, oldQuantity));
            }
            else
            {
                var quantityDifference = quantity - oldQuantity;
                item.UpdateQuantity(quantity);

                if (quantityDifference > 0)
                {
                    AddDomainEvent(new StockReservedEvent(productId, quantityDifference));
                }
                else if (quantityDifference < 0)
                {
                    AddDomainEvent(new StockNotReservedEvent(productId, Math.Abs(quantityDifference)));
                }
            }

            RecalculateTotalAmount();
        }
    }

    private void RecalculateTotalAmount()
    {
        TotalAmount = _items.Sum(i => i.TotalPrice.Value);
    }

    public void UpdateStatus(OrderStatus status)
    {
        Status = status;
    }

    public void UpdateAddresses(Address shippingAddress, Address billingAddress)
    {
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
    }
}

