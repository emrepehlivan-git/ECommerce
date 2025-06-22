namespace ECommerce.Application.Services;

/// <summary>
/// Permission management service interface
/// Admin'in her zaman tüm permission'lara sahip olması için kullanılır
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Admin role'ünün tüm permission'lara sahip olduğundan emin olur
    /// </summary>
    Task EnsureAdminHasAllPermissionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sistemdeki tüm permission'ları getirir (cached)
    /// </summary>
    Task<IReadOnlyList<(string PermissionName, string Module, string Action)>> GetAllPermissionConstantsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirli bir role'e yeni permission'lar ekler
    /// </summary>
    Task AssignPermissionsToRoleAsync(string roleName, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sistemde yeni permission'lar eklendikten sonra çağrılır
    /// </summary>
    Task SyncPermissionsAsync(CancellationToken cancellationToken = default);

    // Backwards compatibility methodları
    Task<bool> HasPermissionAsync(Guid userId, string permission);
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId);
    Task<bool> AssignPermissionToRoleAsync(Guid roleId, string permission);
    Task<bool> RemovePermissionFromRoleAsync(Guid roleId, string permission);
    Task<IEnumerable<string>> GetRolePermissionsAsync(Guid roleId);
}