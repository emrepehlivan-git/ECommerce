namespace ECommerce.Application.Features.UserAddresses.V1.DTOs;

public sealed record UserAddressDto(
    Guid Id,
    Guid UserId,
    string Label,
    string Street,
    string City,
    string ZipCode,
    string Country,
    bool IsDefault,
    bool IsActive,
    DateTime CreatedAt); 