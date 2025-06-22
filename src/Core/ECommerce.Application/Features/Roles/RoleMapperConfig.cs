using ECommerce.Application.Features.Roles.DTOs;
using ECommerce.Domain.Entities;
using Mapster;

namespace ECommerce.Application.Features.Roles;

public static class RoleMapperConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Role, RoleDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Permissions, src => src.RolePermissions
                .Where(rp => rp.IsActive)
                .Select(rp => rp.Permission)
                .Adapt<IReadOnlyCollection<PermissionDto>>());

        TypeAdapterConfig<Permission, PermissionDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Module, src => src.Module)  
            .Map(dest => dest.Action, src => src.Action);
    }
} 