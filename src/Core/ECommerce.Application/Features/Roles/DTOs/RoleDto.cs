namespace ECommerce.Application.Features.Roles.DTOs;

public sealed record RoleDto(
    Guid Id,
    string Name,
    IReadOnlyCollection<PermissionDto> Permissions);

public sealed record PermissionDto(
    Guid Id,
    string Name,
    string Description,
    string Module,
    string Action);

public sealed record UserRoleDto(
    Guid UserId,
    string UserName,
    IReadOnlyCollection<string> Roles); 