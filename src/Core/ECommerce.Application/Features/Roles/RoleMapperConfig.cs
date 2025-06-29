using ECommerce.Application.Features.Roles.DTOs;
using ECommerce.Domain.Entities;
using Mapster;

namespace ECommerce.Application.Features.Roles;

public static class RoleMapperConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Role, RoleDto>.NewConfig()
            .Map(dest => dest.Permissions, src => src.RolePermissions
                .Where(rp => rp.IsActive)
                .Select(rp => rp.Permission));

        TypeAdapterConfig<Permission, PermissionDto>.NewConfig();
    }
} 