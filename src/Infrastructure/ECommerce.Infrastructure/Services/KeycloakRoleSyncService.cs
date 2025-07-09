using System.Security.Claims;
using Ardalis.Result;
using ECommerce.Application.Common.Logging;
using ECommerce.Application.Extensions;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.DependencyInjection;

namespace ECommerce.Infrastructure.Services;

public class KeycloakRoleSyncService(
    IRoleService roleService,
    IECommerceLogger<KeycloakRoleSyncService> logger) : IKeycloakRoleSyncService, IScopedDependency
{
    private readonly HashSet<string> _ignoredClientRoles = new()
    {
        "offline_access",
        "uma_authorization"
    };

    public async Task<Result> SyncUserRolesFromTokenAsync(User user, ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        try
        {
            var clientId = principal.FindFirstValue("aud") ?? string.Empty;
            logger.LogInformation("Senkronizasyon için token'dan okunan Audience (Client ID): {ClientId}", clientId);

            var clientRoles = principal.GetClientRoles();
            logger.LogInformation("Kullanıcı {UserId} için {Count} Keycloak client rolü bulundu: {Roles}", 
                user.Id, clientRoles.Count, string.Join(", ", clientRoles));

            if (clientRoles.Count == 0)
            {
                logger.LogWarning("Kullanıcı {UserId} için senkronize edilecek Keycloak client rolü bulunamadı. Token'ın 'aud' ve 'resource_access' claim'lerini kontrol edin.", user.Id);
                return Result.Success();
            }

            var systemRoles = FilterSystemRoles(clientRoles);
            logger.LogInformation("Client rollerinden sistem rolleri filtrelendi: {Roles}", string.Join(", ", systemRoles));

            var syncRolesResult = await SyncRolesToLocalSystemAsync(systemRoles, cancellationToken);
            if (!syncRolesResult.IsSuccess)
            {
                logger.LogWarning("Client rol senkronizasyonu başarısız ama devam ediliyor: {Error}", string.Join(", ", syncRolesResult.Errors));
            }

            var currentUserRoles = await roleService.GetUserRolesAsync(user);
            var rolesToAdd = systemRoles.Except(currentUserRoles).ToList();
            var rolesToRemove = currentUserRoles.Except(systemRoles)
                .Where(role => !IsProtectedRole(role))
                .ToList();

            foreach (var roleName in rolesToAdd)
            {
                try
                {
                    var addResult = await roleService.AddToRoleAsync(user, roleName);
                    if (addResult.Succeeded)
                    {
                        logger.LogInformation("Kullanıcı {UserId} için client rolü {RoleName} eklendi", user.Id, roleName);
                    }
                    else
                    {
                        logger.LogWarning("Kullanıcı {UserId} için client rolü {RoleName} eklenemedi: {Errors}",
                            user.Id, roleName, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Kullanıcı {UserId} için client rolü {RoleName} ekleme hatası", user.Id, roleName);
                }
            }

            foreach (var roleName in rolesToRemove)
            {
                try
                {
                    if (roleName.Equals("admin", StringComparison.OrdinalIgnoreCase) || 
                        roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        if (systemRoles.Any(r => r.Equals("admin", StringComparison.OrdinalIgnoreCase) || 
                                               r.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
                        {
                            logger.LogInformation("Kullanıcı {UserId} hala Keycloak client rollerinde admin rolüne sahip, yerel admin rolü korunuyor", user.Id);
                            continue;
                        }
                    }

                    var removeResult = await roleService.RemoveFromRoleAsync(user, roleName);
                    if (removeResult.Succeeded)
                    {
                        logger.LogInformation("Kullanıcı {UserId} için client rolü {RoleName} kaldırıldı", user.Id, roleName);
                    }
                    else
                    {
                        logger.LogWarning("Kullanıcı {UserId} için client rolü {RoleName} kaldırılamadı: {Errors}",
                            user.Id, roleName, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Kullanıcı {UserId} için client rolü {RoleName} kaldırma hatası", user.Id, roleName);
                }
            }

            logger.LogInformation("Kullanıcı {UserId} client rol senkronizasyonu tamamlandı. Eklenen: {Added}, Kaldırılan: {Removed}",
                user.Id, rolesToAdd.Count, rolesToRemove.Count);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Kullanıcı {UserId} client rol senkronizasyonu sırasında hata oluştu", user.Id);
            return Result.Error(ex.Message);
        }
    }

    public async Task<Result> SyncRolesToLocalSystemAsync(IEnumerable<string> rolesToSync, CancellationToken cancellationToken = default)
    {
        try
        {
            var createdRoles = new List<string>();
            
            foreach (var roleName in rolesToSync)
            {
                if (await roleService.RoleExistsAsync(roleName))
                {
                    continue;
                }

                var role = Role.Create(roleName);
                var createResult = await roleService.CreateRoleAsync(role);
                
                if (createResult.Succeeded)
                {
                    createdRoles.Add(roleName);
                    logger.LogInformation("Client rolünden sistem rolü oluşturuldu: {RoleName}", roleName);
                }
                else
                {
                    logger.LogWarning("Client rolünden sistem rolü oluşturulamadı {RoleName}: {Errors}",
                        roleName, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }

            if (createdRoles.Any())
            {
                logger.LogInformation("{Count} yeni sistem rolü client rollerinden oluşturuldu: {Roles}", 
                    createdRoles.Count, string.Join(", ", createdRoles));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Client rollerinden sistem rolü oluşturma sırasında hata oluştu");
            return Result.Error($"Client rollerinden sistem rolü oluşturma başarısız: {ex.Message}");
        }
    }

    public List<string> FilterSystemRoles(IEnumerable<string> clientRoles)
    {
        return [.. clientRoles
            .Where(role => !_ignoredClientRoles.Contains(role))
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct()];
    }

    private static bool IsProtectedRole(string roleName)
    {
        var protectedRoles = new[] { "Customer", "SuperAdmin" };
        return protectedRoles.Contains(roleName);
    }
}
