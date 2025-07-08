using System.Security.Claims;
using Ardalis.Result;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

/// <summary>
/// Keycloak token'ından gelen rol bilgilerini yerel sistemle senkronize eden servis
/// </summary>
public interface IKeycloakRoleSyncService
{
    /// <summary>
    /// Keycloak token'ından gelen rolleri kullanıcıya atar
    /// </summary>
    /// <param name="user">Rolleri atanacak kullanıcı</param>
    /// <param name="principal">Keycloak token'ından oluşan ClaimsPrincipal</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Senkronizasyon sonucu</returns>
    Task<Result> SyncUserRolesFromTokenAsync(User user, ClaimsPrincipal principal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Keycloak'ta bulunan rolleri yerel sisteme sync eder (rolleri oluşturur)
    /// </summary>
    /// <param name="rolesToSync">Senkronize edilecek rol isimleri</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Senkronizasyon sonucu</returns>
    Task<Result> SyncRolesToLocalSystemAsync(IEnumerable<string> rolesToSync, CancellationToken cancellationToken = default);

    /// <summary>
    /// Token'dan gelen rollerin hangileri sistem rolleri olduğunu belirler
    /// (Keycloak'ın default rollerini filtreler)
    /// </summary>
    /// <param name="keycloakRoles">Keycloak'tan gelen roller</param>
    /// <returns>İşlenebilir sistem rolleri</returns>
    List<string> FilterSystemRoles(IEnumerable<string> keycloakRoles);
}
