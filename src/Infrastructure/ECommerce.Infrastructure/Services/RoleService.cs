using Ardalis.Result;
using ECommerce.Application.Common.Logging;
using ECommerce.Application.Extensions;
using ECommerce.Application.Features.Roles.V1.DTOs;
using ECommerce.Application.Parameters;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public sealed class RoleService(
    UserManager<User> userManager, 
    RoleManager<Role> roleManager,
    IKeycloakPermissionSyncService keycloakSyncService,
    IPermissionService permissionService,
    IKeycloakRoleManagementService keycloakRoleManagementService,
    IECommerceLogger<RoleService> logger) : IRoleService, IScopedDependency
{
    public async Task<IList<string>> GetRolesAsync()
    {
        return await roleManager.Roles.Select(r => r.Name!).ToListAsync();
    }

    public async Task<IList<string>> GetUserRolesAsync(User user)
    {
        return await userManager.GetRolesAsync(user);
    }

    public async Task<Role?> FindRoleByIdAsync(Guid roleId)
    {
        return await roleManager.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public async Task<List<Role>> FindRolesByIdsAsync(List<Guid> ids, CancellationToken cancellationToken)
    {
        return await roleManager.Roles.Where(r => ids.Contains(r.Id)).ToListAsync(cancellationToken);
    }

    public async Task<Role?> FindRoleByNameAsync(string roleName)
    {
        return await roleManager.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name == roleName);
    }

    public async Task<IdentityResult> CreateRoleAsync(Role role)
    {
        var result = await roleManager.CreateAsync(role);
        
        if (result.Succeeded)
        {
            // Sync role to Keycloak
            await keycloakRoleManagementService.SyncRoleToKeycloakAsync(role);
        }
        
        return result;
    }

    public async Task<IdentityResult> UpdateRoleAsync(Role role)
    {
        return await roleManager.UpdateAsync(role);
    }

    public async Task<IdentityResult> DeleteRoleAsync(Role role)
    {
        var result = await roleManager.DeleteAsync(role);
        
        if (result.Succeeded)
        {
            // Remove role from Keycloak
            await keycloakRoleManagementService.DeleteClientRoleAsync(role.Name!);
        }
        
        return result;
    }

    public async Task<IdentityResult> DeleteRolesAsync(List<Role> roles)
    {
        foreach (var role in roles)
        {
            var result = await DeleteRoleAsync(role);
            if (!result.Succeeded)
                return result;
        }
        return IdentityResult.Success;
    }

    public async Task<PagedResult<List<RoleDto>>> GetAllRolesAsync(int page, int pageSize, string search, bool includePermissions = false)
    {
        IQueryable<Role> query = roleManager.Roles;

        if (includePermissions)
        {
            query = query.Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission);
        }

        if (!string.IsNullOrEmpty(search))
            query = query.Where(r => r.Name != null && r.Name.ToLower().Contains(search.ToLower()));

        return await query.ApplyPagingAsync<Role, RoleDto>(new PageableRequestParams(page, pageSize), cancellationToken: CancellationToken.None);
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await roleManager.RoleExistsAsync(roleName);
    }

    public async Task<IdentityResult> AddToRoleAsync(User user, string role)
    {
        var result = await userManager.AddToRoleAsync(user, role);
        
        if (result.Succeeded)
        {
            // Sync role to Keycloak
            await keycloakRoleManagementService.AssignClientRoleToUserAsync(user.Id.ToString(), role);
            
            // Sync permissions to Keycloak
            await SyncUserPermissionsToKeycloakAsync(user.Id);
        }
        
        return result;
    }

    public async Task<IdentityResult> RemoveFromRoleAsync(User user, string role)
    {
        var result = await userManager.RemoveFromRoleAsync(user, role);
        
        if (result.Succeeded)
        {
            // Remove role from Keycloak
            await keycloakRoleManagementService.RemoveClientRoleFromUserAsync(user.Id.ToString(), role);
            
            // Sync permissions to Keycloak
            await SyncUserPermissionsToKeycloakAsync(user.Id);
        }
        
        return result;
    }

    private async Task SyncUserPermissionsToKeycloakAsync(Guid userId)
    {
        try
        {
            var userPermissions = await permissionService.GetUserPermissionsAsync(userId);
            await keycloakSyncService.AssignPermissionsToKeycloakUserAsync(userId.ToString(), userPermissions);
            logger.LogDebug("User {UserId} permissions synced to Keycloak", userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Keycloak permission sync error for user {UserId}", userId);
        }
    }
} 