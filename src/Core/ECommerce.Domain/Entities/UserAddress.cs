using ECommerce.Domain.ValueObjects;

namespace ECommerce.Domain.Entities;

public sealed class UserAddress : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    
    public string Label { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    internal UserAddress()
    {
    }

    private UserAddress(Guid userId, string label, Address address, bool isDefault = false)
    {
        UserId = userId;
        SetLabel(label);
        Address = address;
        IsDefault = isDefault;
        IsActive = true;
    }

    public static UserAddress Create(Guid userId, string label, Address address, bool isDefault = false)
    {
        return new(userId, label, address, isDefault);
    }

    public void Update(string label, Address address)
    {
        SetLabel(label);
        Address = address;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
    }

    public void UnsetAsDefault()
    {
        IsDefault = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private void SetLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label cannot be null or empty.", nameof(label));

        if (label.Length < 2)
            throw new ArgumentException("Label cannot be less than 2 characters.", nameof(label));

        if (label.Length > 50)
            throw new ArgumentException("Label cannot be longer than 50 characters.", nameof(label));

        Label = label;
    }
} 