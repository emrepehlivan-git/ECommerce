using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace ECommerce.Infrastructure.Services;


public sealed class PermissionService(
    ApplicationDbContext context,
    RoleManager<Role> roleManager,
    UserManager<User> userManager,
    ILogger<PermissionService> logger) : IPermissionService
{
    private readonly ApplicationDbContext _context = context;
    private readonly RoleManager<Role> _roleManager = roleManager;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ILogger<PermissionService> _logger = logger;

    public async Task EnsureAdminHasAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ensuring Admin has all permissions...");

        var adminRole = await _roleManager.FindByNameAsync("ADMIN") ?? 
                       await _roleManager.FindByNameAsync("Admin");

        if (adminRole == null)
        {
            _logger.LogWarning("Admin role not found. Cannot assign permissions.");
            return;
        }

        var allPermissions = await _context.Permissions
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        
        var existingAdminPermissionIdsList = await _context.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == adminRole.Id && rp.IsActive)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);
        
        var existingAdminPermissionIds = existingAdminPermissionIdsList.ToHashSet();

        var missingPermissions = allPermissions
            .Where(p => !existingAdminPermissionIds.Contains(p.Id))
            .ToList();

        if (missingPermissions.Count == 0)
        {
            _logger.LogInformation("Admin already has all {PermissionCount} permissions", allPermissions.Count);
            return;
        }

        var newRolePermissions = missingPermissions
            .Select(permission => RolePermission.Create(adminRole.Id, permission.Id))
            .ToList();

        await _context.RolePermissions.AddRangeAsync(newRolePermissions, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added {Count} missing permissions to Admin role. Total permissions: {Total}", 
            missingPermissions.Count, allPermissions.Count);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<(string PermissionName, string Module, string Action)>> GetAllPermissionConstantsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = new List<(string, string, string)>();
        var modules = typeof(Application.Common.Constants.PermissionConstants).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        foreach (var module in modules)
        {
            var fields = module.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            
            foreach (var field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                {
                    var permissionName = (string)field.GetValue(null)!;
                    var moduleName = module.Name;
                    var actionName = field.Name;
                    permissions.Add((permissionName, moduleName, actionName));
                }
            }
        }
        
        return Task.FromResult<IReadOnlyList<(string, string, string)>>(permissions.AsReadOnly());
    }

    /// <inheritdoc />
    public async Task AssignPermissionsToRoleAsync(string roleName, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            _logger.LogWarning("Role {RoleName} not found", roleName);
            return;
        }

        var permissionNamesList = permissionNames.ToList();
        var permissions = await _context.Permissions
            .Where(p => permissionNamesList.Contains(p.Name))
            .ToListAsync(cancellationToken);

        var existingRolePermissionIds = await _context.RolePermissions
            .Where(rp => rp.RoleId == role.Id && rp.IsActive)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        var existingIds = existingRolePermissionIds.ToHashSet();
        var newPermissions = permissions.Where(p => !existingIds.Contains(p.Id)).ToList();

        if (newPermissions.Count == 0)
        {
            _logger.LogInformation("Role {RoleName} already has all requested permissions", roleName);
            return;
        }

        var newRolePermissions = newPermissions
            .Select(permission => RolePermission.Create(role.Id, permission.Id))
            .ToList();

        await _context.RolePermissions.AddRangeAsync(newRolePermissions, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added {Count} permissions to role {RoleName}", newPermissions.Count, roleName);
    }

    /// <inheritdoc />
    public async Task SyncPermissionsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Syncing permissions...");

        var permissionDefinitions = await GetAllPermissionConstantsAsync(cancellationToken);
        var existingPermissions = await _context.Permissions
            .AsNoTracking()
            .Select(p => p.Name)
            .ToListAsync(cancellationToken);

        var existingPermissionSet = existingPermissions.ToHashSet();
        var newPermissions = new List<Permission>();

        foreach (var (permissionName, module, action) in permissionDefinitions)
        {
            if (!existingPermissionSet.Contains(permissionName))
            {
                var description = GeneratePermissionDescription(action, module);
                var permission = Permission.Create(permissionName, description, module, action);
                newPermissions.Add(permission);
            }
        }

        if (newPermissions.Count > 0)
        {
            await _context.Permissions.AddRangeAsync(newPermissions, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Added {Count} new permissions to database", newPermissions.Count);
        }
    
        await EnsureAdminHasAllPermissionsAsync(cancellationToken);
    }

    private static string GeneratePermissionDescription(string action, string module)
    {
        return action.ToLower() switch
        {
            "read" => $"Read {module.ToLower()}",
            "view" => $"View {module.ToLower()}",
            "create" => $"Create {module.ToLower()}",
            "update" => $"Update {module.ToLower()}",
            "delete" => $"Delete {module.ToLower()}",
            "manage" => $"Manage {module.ToLower()}",
            _ => $"{action} {module.ToLower()}"
        };
    }

    // Backwards compatibility implementations
    public async Task<bool> HasPermissionAsync(Guid userId, string permission)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var userRoles = await _userManager.GetRolesAsync(user);
        if (!userRoles.Any()) return false;

        return await _context.Roles
            .Where(r => userRoles.Contains(r.Name!))
            .SelectMany(r => r.RolePermissions)
            .Where(rp => rp.IsActive)
            .AnyAsync(rp => rp.Permission.Name == permission);
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return Enumerable.Empty<string>();

        var userRoles = await _userManager.GetRolesAsync(user);
        if (!userRoles.Any()) return Enumerable.Empty<string>();

        return await _context.Roles
            .AsNoTracking()
            .Where(r => userRoles.Contains(r.Name!))
            .SelectMany(r => r.RolePermissions)
            .Where(rp => rp.IsActive)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync();
    }

    public async Task<bool> AssignPermissionToRoleAsync(Guid roleId, string permission)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null) return false;

        var permissionEntity = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == permission);

        if (permissionEntity == null) return false;

        var existingRolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionEntity.Id);

        if (existingRolePermission != null)
        {
            if (!existingRolePermission.IsActive)
            {
                existingRolePermission.Activate();
                await _context.SaveChangesAsync();
            }
            return true;
        }

        var rolePermission = RolePermission.Create(role.Id, permissionEntity.Id);
        await _context.RolePermissions.AddAsync(rolePermission);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemovePermissionFromRoleAsync(Guid roleId, string permission)
    {
        var rolePermission = await _context.RolePermissions
            .Include(rp => rp.Permission)
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.Permission.Name == permission && rp.IsActive);

        if (rolePermission == null) return false;

        rolePermission.Deactivate();
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<string>> GetRolePermissionsAsync(Guid roleId)
    {
        return await _context.RolePermissions
            .Include(rp => rp.Permission)
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId && rp.IsActive)
            .Select(rp => rp.Permission.Name)
            .ToListAsync();
    }
}