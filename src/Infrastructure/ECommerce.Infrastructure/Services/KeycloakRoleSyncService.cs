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

            var currentUserRoles = await roleService.GetUserRolesAsync(user);
            
            // Only ensure customer role is present for every user
            if (!currentUserRoles.Contains("Customer", StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    var addResult = await roleService.AddToRoleAsync(user, "Customer");
                    if (addResult.Succeeded)
                    {
                        logger.LogInformation("Kullanıcı {UserId} için varsayılan 'Customer' rolü eklendi", user.Id);
                    }
                    else
                    {
                        logger.LogWarning("Kullanıcı {UserId} için varsayılan 'Customer' rolü eklenemedi: {Errors}",
                            user.Id, string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Kullanıcı {UserId} için varsayılan 'Customer' rolü ekleme hatası", user.Id);
                }
            }
            
            // Don't automatically assign other roles from Keycloak
            // Only log what roles were found but not auto-assigned
            var systemRoles = FilterSystemRoles(clientRoles);
            if (systemRoles.Any())
            {
                logger.LogInformation("Kullanıcı {UserId} için Keycloak client rolü bulundu ancak otomatik atanmadı: {Roles}", 
                    user.Id, string.Join(", ", systemRoles));
            }

            logger.LogInformation("Kullanıcı {UserId} client rol senkronizasyonu tamamlandı. Sadece Customer rolü kontrol edildi.", user.Id);

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
