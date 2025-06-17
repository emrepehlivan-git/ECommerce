using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel;

namespace ECommerce.Infrastructure.Services;

public sealed class PermissionService(IIdentityService identityService) : IPermissionService, IScopedDependency
{
    public async Task<bool> HasPermissionAsync(Guid userId, string permission)
    {
        var user = await identityService.FindByIdAsync(userId);
        if (user == null) return false;

        var userRoles = await identityService.GetUserRolesAsync(user);
        var roles = await identityService.GetAllRolesAsync();

        return roles
            .Where(r => userRoles.Contains(r.Name!))
            .SelectMany(r => r.RolePermissions)
            .Where(rp => rp.IsActive)
            .Any(rp => rp.Permission.Name == permission);
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
    {
        var user = await identityService.FindByIdAsync(userId);
        if (user == null) return Enumerable.Empty<string>();

        var userRoles = await identityService.GetUserRolesAsync(user);
        var roles = await identityService.GetAllRolesAsync();

        return roles
            .Where(r => userRoles.Contains(r.Name!))
            .SelectMany(r => r.RolePermissions)
            .Where(rp => rp.IsActive)
            .Select(rp => rp.Permission.Name)
            .Distinct();
    }

    public async Task<bool> AssignPermissionToRoleAsync(Guid roleId, string permission)
    {
        var role = await identityService.FindRoleByIdAsync(roleId);
        if (role == null) return false;

        var permissionEntity = Permission.Create(permission, string.Empty, permission.Split('.')[0], permission.Split('.')[1]);
        var rolePermission = RolePermission.Create(roleId, permissionEntity.Id);

        role.AddPermission(rolePermission);
        var result = await identityService.UpdateRoleAsync(role);

        return result.Succeeded;
    }

    public async Task<bool> RemovePermissionFromRoleAsync(Guid roleId, string permission)
    {
        var role = await identityService.FindRoleByIdAsync(roleId);
        if (role == null) return false;

        var rolePermission = role.RolePermissions
            .FirstOrDefault(rp => rp.Permission.Name == permission);

        if (rolePermission == null) return false;

        rolePermission.Deactivate();
        var result = await identityService.UpdateRoleAsync(role);

        return result.Succeeded;
    }

    public async Task<IEnumerable<string>> GetRolePermissionsAsync(Guid roleId)
    {
        var role = await identityService.FindRoleByIdAsync(roleId);
        if (role == null) return Enumerable.Empty<string>();

        return role.RolePermissions
            .Where(rp => rp.IsActive)
            .Select(rp => rp.Permission.Name);
    }
}