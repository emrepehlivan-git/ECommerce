namespace ECommerce.Application.Features.Users.DTOs;

public sealed record UserDto(Guid Id, string Email, string FullName, bool IsActive, DateTime? Birthday);
