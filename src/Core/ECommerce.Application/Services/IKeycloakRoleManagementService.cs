using Ardalis.Result;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

/// <summary>
/// Keycloak role management service for bidirectional role synchronization
/// </summary>
public interface IKeycloakRoleManagementService
{
    /// <summary>
    /// Creates a role in Keycloak client
    /// </summary>
    /// <param name="roleName">Role name to create</param>
    /// <param name="clientId">Keycloak client ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of role creation</returns>
    Task<Result> CreateClientRoleAsync(string roleName, string clientId = "ecommerce-api", CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role from Keycloak client
    /// </summary>
    /// <param name="roleName">Role name to delete</param>
    /// <param name="clientId">Keycloak client ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of role deletion</returns>
    Task<Result> DeleteClientRoleAsync(string roleName, string clientId = "ecommerce-api", CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a client role to a user in Keycloak
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="roleName">Role name to assign</param>
    /// <param name="clientId">Keycloak client ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of role assignment</returns>
    Task<Result> AssignClientRoleToUserAsync(string userId, string roleName, string clientId = "ecommerce-api", CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a client role from a user in Keycloak
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="roleName">Role name to remove</param>
    /// <param name="clientId">Keycloak client ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of role removal</returns>
    Task<Result> RemoveClientRoleFromUserAsync(string userId, string roleName, string clientId = "ecommerce-api", CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all client roles assigned to a user in Keycloak
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="clientId">Keycloak client ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of assigned roles</returns>
    Task<Result<List<string>>> GetUserClientRolesAsync(string userId, string clientId = "ecommerce-api", CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a client role exists in Keycloak
    /// </summary>
    /// <param name="roleName">Role name to check</param>
    /// <param name="clientId">Keycloak client ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if role exists</returns>
    Task<Result<bool>> ClientRoleExistsAsync(string roleName, string clientId = "ecommerce-api", CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes local role to Keycloak (creates if not exists)
    /// </summary>
    /// <param name="role">Local role entity</param>
    /// <param name="clientId">Keycloak client ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of synchronization</returns>
    Task<Result> SyncRoleToKeycloakAsync(Role role, string clientId = "ecommerce-api", CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes all user roles to Keycloak
    /// </summary>
    /// <param name="user">User entity</param>
    /// <param name="clientId">Keycloak client ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of synchronization</returns>
    Task<Result> SyncUserRolesToKeycloakAsync(User user, string clientId = "ecommerce-api", CancellationToken cancellationToken = default);
}