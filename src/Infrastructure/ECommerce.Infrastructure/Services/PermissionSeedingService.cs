using ECommerce.Application.Common.Logging;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Persistence.Contexts;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ECommerce.Infrastructure.Services;

public class PermissionSeedingService : IScopedDependency
{
    private readonly ApplicationDbContext _context;
    private readonly IKeycloakPermissionSyncService _keycloakSyncService;
    private readonly IECommerceLogger<PermissionSeedingService> _logger;

    public PermissionSeedingService(
        ApplicationDbContext context,
        IKeycloakPermissionSyncService keycloakSyncService,
        IECommerceLogger<PermissionSeedingService> logger)
    {
        _context = context;
        _keycloakSyncService = keycloakSyncService;
        _logger = logger;
    }

    public async Task<string> SeedPermissionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var permissionDefinitions = GetPermissionDefinitions();
            
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

            if (newPermissions.Any())
            {
                await _context.Permissions.AddRangeAsync(newPermissions, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Database'e {Count} yeni permission eklendi", newPermissions.Count);
            }

            await SyncToKeycloakAsync();
            await EnsureAdminHasAllPermissionsAsync(cancellationToken);

            var message = $"Permission seeding tamamlandı. Database'e {newPermissions.Count} yeni permission eklendi.";
            _logger.LogInformation(message);
            
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Permission seeding hatası");
            throw new Exception("Permission seeding sırasında hata oluştu: " + ex.Message, ex);
        }
    }

    private List<(string PermissionName, string Module, string Action)> GetPermissionDefinitions()
    {
        var permissionDefinitions = new List<(string, string, string)>();
        var permissionTypes = typeof(PermissionConstants).GetNestedTypes();

        foreach (var type in permissionTypes)
        {
            var module = type.Name; // Products, Orders, etc.
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string))
                {
                    var permissionName = (string)field.GetValue(null)!;
                    var action = field.Name; // Create, Update, Delete, etc.
                    permissionDefinitions.Add((permissionName, module, action));
                }
            }
        }

        return permissionDefinitions;
    }

    private string GeneratePermissionDescription(string action, string module)
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

    private async Task SyncToKeycloakAsync()
    {
        try
        {
            await _keycloakSyncService.SyncPermissionsToKeycloakAsync();
            _logger.LogInformation("Permission'lar Keycloak'a sync edildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Keycloak sync hatası");
            // Keycloak sync hatası permission seeding'i engellemez
        }
    }

    private async Task EnsureAdminHasAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Admin rolünü bul
            var adminRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "admin", cancellationToken);
            
            if (adminRole == null)
            {
                _logger.LogWarning("Admin role bulunamadı, admin role oluşturuluyor");
                adminRole = Role.Create("admin");
                await _context.Roles.AddAsync(adminRole, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Tüm permissionları al
            var allPermissions = await _context.Permissions
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Admin rolünün mevcut permissionları
            var existingRolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == adminRole.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync(cancellationToken);

            var existingPermissionSet = existingRolePermissions.ToHashSet();
            var newRolePermissions = new List<RolePermission>();

            foreach (var permission in allPermissions)
            {
                if (!existingPermissionSet.Contains(permission.Id))
                {
                    var rolePermission = RolePermission.Create(adminRole.Id, permission.Id);
                    newRolePermissions.Add(rolePermission);
                }
            }

            if (newRolePermissions.Any())
            {
                await _context.RolePermissions.AddRangeAsync(newRolePermissions, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Admin rolüne {Count} yeni permission eklendi", newRolePermissions.Count);
            }
            else
            {
                _logger.LogInformation("Admin rolünde tüm permission'lar mevcut");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin permission atama hatası");
        }
    }
} 