using System.Security.Claims;
using Ardalis.Result;
using ECommerce.Application.Common.Logging;
using ECommerce.Application.Extensions;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Infrastructure.Services;

public class KeycloakRoleSyncService : IKeycloakRoleSyncService, IScopedDependency
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IRoleService _roleService;
    private readonly IECommerceLogger<KeycloakRoleSyncService> _logger;

    // Client rollerinde genellikle bu default roller olmaz, ancak yine de güvenlik için bırakıyoruz
    private readonly HashSet<string> _ignoredKeycloakRoles = new()
    {
        "default-roles-ecommerce",
        "offline_access", 
        "uma_authorization"
    };

    public KeycloakRoleSyncService(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IRoleService roleService,
        IECommerceLogger<KeycloakRoleSyncService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _roleService = roleService;
        _logger = logger;
    }

    public async Task<Result> SyncUserRolesFromTokenAsync(User user, ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        try
        {
            var keycloakRoles = principal.GetAllKeycloakRoles();
            _logger.LogInformation("Kullanıcı {UserId} için {Count} Keycloak client rolü bulundu: {Roles}", 
                user.Id, keycloakRoles.Count, string.Join(", ", keycloakRoles));

            var systemRoles = FilterSystemRoles(keycloakRoles);
            _logger.LogInformation("Sistem rolleri filtrelendi: {Roles}", string.Join(", ", systemRoles));

            var syncRolesResult = await SyncRolesToLocalSystemAsync(systemRoles, cancellationToken);
            if (!syncRolesResult.IsSuccess)
            {
                return syncRolesResult;
            }

            var currentUserRoles = await _roleService.GetUserRolesAsync(user);
            var rolesToAdd = systemRoles.Except(currentUserRoles).ToList();
            var rolesToRemove = currentUserRoles.Except(systemRoles)
                .Where(role => !IsProtectedRole(role))
                .ToList();

            foreach (var roleName in rolesToAdd)
            {
                var addResult = await _roleService.AddToRoleAsync(user, roleName);
                if (addResult.Succeeded)
                {
                    _logger.LogInformation("Kullanıcı {UserId} için {RoleName} rolü eklendi", user.Id, roleName);
                }
                else
                {
                    _logger.LogWarning("Kullanıcı {UserId} için {RoleName} rolü eklenemedi: {Errors}",
                        user.Id, roleName, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                }
            }

            foreach (var roleName in rolesToRemove)
            {
                var removeResult = await _roleService.RemoveFromRoleAsync(user, roleName);
                if (removeResult.Succeeded)
                {
                    _logger.LogInformation("Kullanıcı {UserId} için {RoleName} rolü kaldırıldı", user.Id, roleName);
                }
                else
                {
                    _logger.LogWarning("Kullanıcı {UserId} için {RoleName} rolü kaldırılamadı: {Errors}",
                        user.Id, roleName, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                }
            }

            _logger.LogInformation("Kullanıcı {UserId} rol senkronizasyonu tamamlandı. Eklenen: {Added}, Kaldırılan: {Removed}",
                user.Id, rolesToAdd.Count, rolesToRemove.Count);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı {UserId} rol senkronizasyonu sırasında hata oluştu", user.Id);
            return Result.Error($"Rol senkronizasyonu başarısız: {ex.Message}");
        }
    }

    public async Task<Result> SyncRolesToLocalSystemAsync(IEnumerable<string> rolesToSync, CancellationToken cancellationToken = default)
    {
        try
        {
            var createdRoles = new List<string>();
            
            foreach (var roleName in rolesToSync)
            {
                if (await _roleService.RoleExistsAsync(roleName))
                {
                    continue;
                }

                var role = Role.Create(roleName);
                var createResult = await _roleService.CreateRoleAsync(role);
                
                if (createResult.Succeeded)
                {
                    createdRoles.Add(roleName);
                    _logger.LogInformation("Rol oluşturuldu: {RoleName}", roleName);
                }
                else
                {
                    _logger.LogWarning("Rol oluşturulamadı {RoleName}: {Errors}",
                        roleName, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }

            if (createdRoles.Any())
            {
                _logger.LogInformation("{Count} yeni rol oluşturuldu: {Roles}", 
                    createdRoles.Count, string.Join(", ", createdRoles));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rol senkronizasyonu sırasında hata oluştu");
            return Result.Error($"Rol oluşturma başarısız: {ex.Message}");
        }
    }

    public List<string> FilterSystemRoles(IEnumerable<string> keycloakRoles)
    {
        return keycloakRoles
            .Where(role => !_ignoredKeycloakRoles.Contains(role))
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct()
            .ToList();
    }

    private bool IsProtectedRole(string roleName)
    {
        var protectedRoles = new[] { "Customer", "SuperAdmin" };
        return protectedRoles.Contains(roleName);
    }
}
