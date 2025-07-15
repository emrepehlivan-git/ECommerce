using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using ECommerce.Application.Common.Logging;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Infrastructure.Services;

public sealed class KeycloakRoleManagementService(
    HttpClient httpClient,
    IConfiguration configuration,
    IECommerceLogger<KeycloakRoleManagementService> logger,
    UserManager<User> userManager,
    IRoleService roleService) : IKeycloakRoleManagementService, IScopedDependency
{
    private readonly string _authServerUrl = configuration.GetSection("Keycloak")["auth-server-url"]!;
    private readonly string _realm = configuration.GetSection("Keycloak")["realm"]!;
    private readonly string _clientId = configuration.GetSection("Keycloak")["admin-client-id"] ?? "admin-cli";
    private readonly string _clientSecret = configuration.GetSection("Keycloak")["admin-client-secret"] ?? "";
    private readonly string _adminUsername = configuration.GetSection("Keycloak")["admin-username"]!;
    private readonly string _adminPassword = configuration.GetSection("Keycloak")["admin-password"]!;

    public async Task<Result> CreateClientRoleAsync(string roleName, string? description = null, string clientId = "ecommerce-api", CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetKeycloakAdminTokenAsync();
            var keycloakClientId = await GetKeycloakClientIdAsync(token, clientId);
            
            if (string.IsNullOrEmpty(keycloakClientId))
            {
                return Result.Error($"Keycloak client '{clientId}' not found");
            }

            var roleData = new
            {
                name = roleName,
                description = description ?? $"Role: {roleName}",
                composite = false,
                clientRole = true
            };

            var url = $"{_authServerUrl}admin/realms/{_realm}/clients/{keycloakClientId}/roles";
            var content = new StringContent(JsonSerializer.Serialize(roleData), Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.PostAsync(url, content, cancellationToken);

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                logger.LogInformation("Keycloak client role '{RoleName}' created successfully for client '{ClientId}'", roleName, clientId);
                return Result.Success();
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Failed to create Keycloak client role '{RoleName}': {Error}", roleName, errorContent);
            return Result.Error($"Failed to create Keycloak role: {errorContent}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating Keycloak client role '{RoleName}'", roleName);
            return Result.Error($"Error creating Keycloak role: {ex.Message}");
        }
    }

    public async Task<Result> DeleteClientRoleAsync(string roleName, string clientId = "ecommerce-api", CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetKeycloakAdminTokenAsync();
            var keycloakClientId = await GetKeycloakClientIdAsync(token, clientId);
            
            if (string.IsNullOrEmpty(keycloakClientId))
            {
                return Result.Error($"Keycloak client '{clientId}' not found");
            }

            var url = $"{_authServerUrl}admin/realms/{_realm}/clients/{keycloakClientId}/roles/{roleName}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.DeleteAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogInformation("Keycloak client role '{RoleName}' deleted successfully for client '{ClientId}'", roleName, clientId);
                return Result.Success();
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Failed to delete Keycloak client role '{RoleName}': {Error}", roleName, errorContent);
            return Result.Error($"Failed to delete Keycloak role: {errorContent}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting Keycloak client role '{RoleName}'", roleName);
            return Result.Error($"Error deleting Keycloak role: {ex.Message}");
        }
    }

    public async Task<Result> AssignClientRoleToUserAsync(string userId, string roleName, string clientId = "ecommerce-api", CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetKeycloakAdminTokenAsync();
            var keycloakClientId = await GetKeycloakClientIdAsync(token, clientId);
            
            if (string.IsNullOrEmpty(keycloakClientId))
            {
                return Result.Error($"Keycloak client '{clientId}' not found");
            }

            // First, ensure the role exists in Keycloak
            var roleExistsResult = await ClientRoleExistsAsync(roleName, clientId, cancellationToken);
            if (!roleExistsResult.IsSuccess)
            {
                return Result.Error($"Failed to check if role exists: {string.Join(", ", roleExistsResult.Errors)}");
            }

            if (!roleExistsResult.Value)
            {
                var createResult = await CreateClientRoleAsync(roleName, clientId: clientId, cancellationToken: cancellationToken);
                if (!createResult.IsSuccess)
                {
                    return Result.Error($"Failed to create role in Keycloak: {string.Join(", ", createResult.Errors)}");
                }
            }

            // Get role details
            var roleUrl = $"{_authServerUrl}admin/realms/{_realm}/clients/{keycloakClientId}/roles/{roleName}";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var roleResponse = await httpClient.GetAsync(roleUrl, cancellationToken);

            if (!roleResponse.IsSuccessStatusCode)
            {
                var roleError = await roleResponse.Content.ReadAsStringAsync(cancellationToken);
                return Result.Error($"Failed to get role details: {roleError}");
            }

            var roleContent = await roleResponse.Content.ReadAsStringAsync(cancellationToken);
            var roleData = JsonSerializer.Deserialize<JsonElement>(roleContent);

            // Assign role to user
            var assignUrl = $"{_authServerUrl}admin/realms/{_realm}/users/{userId}/role-mappings/clients/{keycloakClientId}";
            var assignData = new[] { new {
                id = roleData.GetProperty("id").GetString(),
                name = roleData.GetProperty("name").GetString(),
                description = roleData.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                composite = roleData.GetProperty("composite").GetBoolean(),
                clientRole = roleData.GetProperty("clientRole").GetBoolean(),
                containerId = roleData.GetProperty("containerId").GetString()
            }};

            var assignContent = new StringContent(JsonSerializer.Serialize(assignData), Encoding.UTF8, "application/json");
            var assignResponse = await httpClient.PostAsync(assignUrl, assignContent, cancellationToken);

            if (assignResponse.IsSuccessStatusCode)
            {
                logger.LogInformation("Keycloak client role '{RoleName}' assigned to user '{UserId}' for client '{ClientId}'", roleName, userId, clientId);
                return Result.Success();
            }

            var assignError = await assignResponse.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Failed to assign Keycloak client role '{RoleName}' to user '{UserId}': {Error}", roleName, userId, assignError);
            return Result.Error($"Failed to assign Keycloak role: {assignError}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning Keycloak client role '{RoleName}' to user '{UserId}'", roleName, userId);
            return Result.Error($"Error assigning Keycloak role: {ex.Message}");
        }
    }

    public async Task<Result> RemoveClientRoleFromUserAsync(string userId, string roleName, string clientId = "ecommerce-api", CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetKeycloakAdminTokenAsync();
            var keycloakClientId = await GetKeycloakClientIdAsync(token, clientId);
            
            if (string.IsNullOrEmpty(keycloakClientId))
            {
                return Result.Error($"Keycloak client '{clientId}' not found");
            }

            // Get role details
            var roleUrl = $"{_authServerUrl}admin/realms/{_realm}/clients/{keycloakClientId}/roles/{roleName}";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var roleResponse = await httpClient.GetAsync(roleUrl, cancellationToken);

            if (!roleResponse.IsSuccessStatusCode)
            {
                if (roleResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    logger.LogWarning("Keycloak client role '{RoleName}' not found for removal", roleName);
                    return Result.Success(); // Role doesn't exist, consider it removed
                }
                var roleError = await roleResponse.Content.ReadAsStringAsync(cancellationToken);
                return Result.Error($"Failed to get role details: {roleError}");
            }

            var roleContent = await roleResponse.Content.ReadAsStringAsync(cancellationToken);
            var roleData = JsonSerializer.Deserialize<JsonElement>(roleContent);

            // Remove role from user
            var removeUrl = $"{_authServerUrl}admin/realms/{_realm}/users/{userId}/role-mappings/clients/{keycloakClientId}";
            var removeData = new[] { new {
                id = roleData.GetProperty("id").GetString(),
                name = roleData.GetProperty("name").GetString(),
                description = roleData.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                composite = roleData.GetProperty("composite").GetBoolean(),
                clientRole = roleData.GetProperty("clientRole").GetBoolean(),
                containerId = roleData.GetProperty("containerId").GetString()
            }};

            var removeContent = new StringContent(JsonSerializer.Serialize(removeData), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Delete, removeUrl) { Content = removeContent };
            var removeResponse = await httpClient.SendAsync(request, cancellationToken);

            if (removeResponse.IsSuccessStatusCode)
            {
                logger.LogInformation("Keycloak client role '{RoleName}' removed from user '{UserId}' for client '{ClientId}'", roleName, userId, clientId);
                return Result.Success();
            }

            var removeError = await removeResponse.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Failed to remove Keycloak client role '{RoleName}' from user '{UserId}': {Error}", roleName, userId, removeError);
            return Result.Error($"Failed to remove Keycloak role: {removeError}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing Keycloak client role '{RoleName}' from user '{UserId}'", roleName, userId);
            return Result.Error($"Error removing Keycloak role: {ex.Message}");
        }
    }

    public async Task<Result<List<string>>> GetUserClientRolesAsync(string userId, string clientId = "ecommerce-api", CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetKeycloakAdminTokenAsync();
            var keycloakClientId = await GetKeycloakClientIdAsync(token, clientId);
            
            if (string.IsNullOrEmpty(keycloakClientId))
            {
                return Result<List<string>>.Error($"Keycloak client '{clientId}' not found");
            }

            var url = $"{_authServerUrl}admin/realms/{_realm}/users/{userId}/role-mappings/clients/{keycloakClientId}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return Result<List<string>>.Error($"Failed to get user roles: {error}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var roles = JsonSerializer.Deserialize<JsonElement[]>(content);

            var roleNames = roles.Select(r => r.GetProperty("name").GetString()!)
                                 .Where(name => !string.IsNullOrEmpty(name))
                                 .ToList();

            return Result<List<string>>.Success(roleNames);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user client roles from Keycloak for user '{UserId}'", userId);
            return Result<List<string>>.Error($"Error getting user roles: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ClientRoleExistsAsync(string roleName, string clientId = "ecommerce-api", CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetKeycloakAdminTokenAsync();
            var keycloakClientId = await GetKeycloakClientIdAsync(token, clientId);
            
            if (string.IsNullOrEmpty(keycloakClientId))
            {
                return Result<bool>.Error($"Keycloak client '{clientId}' not found");
            }

            var url = $"{_authServerUrl}admin/realms/{_realm}/clients/{keycloakClientId}/roles/{roleName}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.GetAsync(url, cancellationToken);

            return Result<bool>.Success(response.IsSuccessStatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if client role exists in Keycloak: '{RoleName}'", roleName);
            return Result<bool>.Error($"Error checking role existence: {ex.Message}");
        }
    }

    public async Task<Result> SyncRoleToKeycloakAsync(Role role, string clientId = "ecommerce-api", CancellationToken cancellationToken = default)
    {
        try
        {
            var existsResult = await ClientRoleExistsAsync(role.Name!, clientId, cancellationToken);
            if (!existsResult.IsSuccess)
            {
                return Result.Error($"Failed to check if role exists: {string.Join(", ", existsResult.Errors)}");
            }

            if (!existsResult.Value)
            {
                var createResult = await CreateClientRoleAsync(role.Name!, role.Description, clientId, cancellationToken);
                if (!createResult.IsSuccess)
                {
                    return Result.Error($"Failed to create role in Keycloak: {string.Join(", ", createResult.Errors)}");
                }
            }

            logger.LogInformation("Role '{RoleName}' synchronized to Keycloak client '{ClientId}'", role.Name, clientId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error synchronizing role '{RoleName}' to Keycloak", role.Name);
            return Result.Error($"Error synchronizing role: {ex.Message}");
        }
    }

    public async Task<Result> SyncUserRolesToKeycloakAsync(User user, string clientId = "ecommerce-api", CancellationToken cancellationToken = default)
    {
        try
        {
            var localRoles = await roleService.GetUserRolesAsync(user);
            var keycloakRolesResult = await GetUserClientRolesAsync(user.Id.ToString(), clientId, cancellationToken);

            if (!keycloakRolesResult.IsSuccess)
            {
                return Result.Error($"Failed to get Keycloak roles: {string.Join(", ", keycloakRolesResult.Errors)}");
            }

            var keycloakRoles = keycloakRolesResult.Value;

            // Roles to add to Keycloak
            var rolesToAdd = localRoles.Except(keycloakRoles, StringComparer.OrdinalIgnoreCase).ToList();
            
            // Roles to remove from Keycloak
            var rolesToRemove = keycloakRoles.Except(localRoles, StringComparer.OrdinalIgnoreCase).ToList();

            // Add missing roles
            foreach (var roleName in rolesToAdd)
            {
                var assignResult = await AssignClientRoleToUserAsync(user.Id.ToString(), roleName, clientId, cancellationToken);
                if (!assignResult.IsSuccess)
                {
                    logger.LogWarning("Failed to assign role '{RoleName}' to user '{UserId}' in Keycloak: {Error}",
                        roleName, user.Id, string.Join(", ", assignResult.Errors));
                }
            }

            // Remove extra roles (only if they're not protected)
            var protectedRoles = new[] { "customer", "admin", "manager" };
            foreach (var roleName in rolesToRemove)
            {
                if (protectedRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase))
                {
                    continue; // Don't remove protected roles
                }

                var removeResult = await RemoveClientRoleFromUserAsync(user.Id.ToString(), roleName, clientId, cancellationToken);
                if (!removeResult.IsSuccess)
                {
                    logger.LogWarning("Failed to remove role '{RoleName}' from user '{UserId}' in Keycloak: {Error}",
                        roleName, user.Id, string.Join(", ", removeResult.Errors));
                }
            }

            logger.LogInformation("User '{UserId}' roles synchronized to Keycloak. Added: {Added}, Removed: {Removed}",
                user.Id, rolesToAdd.Count, rolesToRemove.Count);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error synchronizing user roles to Keycloak for user '{UserId}'", user.Id);
            return Result.Error($"Error synchronizing user roles: {ex.Message}");
        }
    }

    private async Task<string> GetKeycloakAdminTokenAsync()
    {
        var tokenEndpoint = $"{_authServerUrl}realms/{_realm}/protocol/openid-connect/token";

        var tokenRequest = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret),
            new KeyValuePair<string, string>("username", _adminUsername),
            new KeyValuePair<string, string>("password", _adminPassword)
        });

        var response = await httpClient.PostAsync(tokenEndpoint, tokenRequest);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to get Keycloak admin token: {content}");

        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }

    private async Task<string?> GetKeycloakClientIdAsync(string token, string clientId)
    {
        var url = $"{_authServerUrl}admin/realms/{_realm}/clients?clientId={clientId}";

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var clients = JsonSerializer.Deserialize<JsonElement[]>(content);

        return clients.FirstOrDefault().TryGetProperty("id", out var id) ? id.GetString() : null;
    }
}