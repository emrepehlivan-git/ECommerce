using System.Security.Claims;
using Ardalis.Result;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

/// <summary>
/// Keycloak client token'ından gelen rol bilgilerini yerel sistemle senkronize eden servis
/// Not: Sadece client rolleri kullanılır, realm rolleri kullanılmaz
/// </summary>
public interface IKeycloakRoleSyncService
{
    /// <summary>
    /// Keycloak client token'ından gelen rolleri kullanıcıya atar
    /// </summary>
    /// <param name="user">Rolleri atanacak kullanıcı</param>
    /// <param name="principal">Keycloak token'ından oluşan ClaimsPrincipal (sadece client rolleri işlenir)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Senkronizasyon sonucu</returns>
    Task<Result> SyncUserRolesFromTokenAsync(User user, ClaimsPrincipal principal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Keycloak client rollerini yerel sisteme sync eder (rolleri oluşturur)
    /// </summary>
    /// <param name="rolesToSync">Senkronize edilecek client rol isimleri</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Senkronizasyon sonucu</returns>
    Task<Result> SyncRolesToLocalSystemAsync(IEnumerable<string> rolesToSync, CancellationToken cancellationToken = default);

    /// <summary>
    /// Client token'dan gelen rollerin hangileri sistem rolleri olduğunu belirler
    /// (Keycloak'ın varsayılan client rollerini filtreler)
    /// </summary>
    /// <param name="clientRoles">Keycloak client'ından gelen roller</param>
    /// <returns>İşlenebilir sistem rolleri</returns>
    List<string> FilterSystemRoles(IEnumerable<string> clientRoles);
}
