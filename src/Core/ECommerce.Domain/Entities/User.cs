using System.Text.RegularExpressions;
using ECommerce.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Domain.Entities;

public sealed class User : IdentityUser<Guid>
{
    private readonly List<UserAddress> _addresses = [];

    public FullName FullName { get; private set; } = null!;
    public bool IsActive { get; private set; }
    
    public IReadOnlyCollection<UserAddress> Addresses => _addresses.AsReadOnly();

    internal User()
    {
    }

    private User(string email, FullName fullName)
    {
        SetEmail(email);
        FullName = fullName;
        IsActive = true;
    }

    public static User Create(string email, string firstName, string lastName)
    {
        return new(email, FullName.Create(firstName, lastName));
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public void UpdateName(string firstName, string lastName) => FullName = FullName.Create(firstName, lastName);

    private void SetEmail(string email)
    {
        const string EmailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        if (!Regex.IsMatch(email, EmailRegex))
            throw new ArgumentException("Invalid email address.");

        Email = email;
        UserName = email;
    }
}