using ECommerce.Application.Common.Logging;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Persistence.Contexts;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ECommerce.Infrastructure.Services;

public sealed class KeycloakPermissionSyncService(
    HttpClient httpClient,
    IConfiguration configuration,
    IECommerceLogger<KeycloakPermissionSyncService> logger,
    ApplicationDbContext context
    ) : IKeycloakPermissionSyncService, IScopedDependency
{
    public async Task<string> GetKeycloakAdminTokenAsync()
    {
        var keycloakOptions = configuration.GetSection("Keycloak");
        var authServerUrl = keycloakOptions["auth-server-url"];
        var realm = keycloakOptions["realm"];
        var clientId = keycloakOptions["admin-client-id"] ?? "admin-cli";
        var clientSecret = keycloakOptions["admin-client-secret"];
        var adminUsername = keycloakOptions["admin-username"];
        var adminPassword = keycloakOptions["admin-password"];

        var tokenEndpoint = $"{authServerUrl}realms/{realm}/protocol/openid-connect/token";

        var tokenRequest = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret!),
            new KeyValuePair<string, string>("username", adminUsername!),
            new KeyValuePair<string, string>("password", adminPassword!)
        });

        var response = await httpClient.PostAsync(tokenEndpoint, tokenRequest);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Keycloak token alınamadı: {content}");

        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }

    public async Task SyncPermissionsToKeycloakAsync()
    {
        try
        {
            var token = await GetKeycloakAdminTokenAsync();
            var keycloakOptions = configuration.GetSection("Keycloak")!;
            var authServerUrl = keycloakOptions["auth-server-url"]!;
            var realm = keycloakOptions["realm"]!;
            var clientId = keycloakOptions["client-id"]!;

            var keycloakClientId = await GetKeycloakClientIdAsync(token, authServerUrl, realm, clientId);

            var localPermissions = await context.Permissions
                .AsNoTracking()
                .ToListAsync();

            foreach (var permission in localPermissions)
                await CreateOrUpdateKeycloakRoleAsync(token, authServerUrl, realm, keycloakClientId, permission);

            logger.LogInformation("Permission'lar Keycloak'a sync edildi. Toplam: {Count}", localPermissions.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Keycloak permission sync hatası");
            throw;
        }
    }

    private async Task<string> GetKeycloakClientIdAsync(string token, string authServerUrl, string realm, string clientId)
    {
        var clientsEndpoint = $"{authServerUrl}admin/realms/{realm}/clients?clientId={clientId}";
        
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await httpClient.GetAsync(clientsEndpoint);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Keycloak client bulunamadı: {content}");

        using var doc = JsonDocument.Parse(content);
        var clients = doc.RootElement.EnumerateArray();
        return clients.First().GetProperty("id").GetString()!;
    }

    private async Task CreateOrUpdateKeycloakRoleAsync(string token, string authServerUrl, string realm, string keycloakClientId, Permission permission)
    {
        var roleEndpoint = $"{authServerUrl}admin/realms/{realm}/clients/{keycloakClientId}/roles";
        
        var roleData = new
        {
            name = permission.Name,
            description = permission.Description,
            attributes = new Dictionary<string, string[]>
            {
                ["module"] = [permission.Module],
                ["action"] = [permission.Action]
            }
        };

        var json = JsonSerializer.Serialize(roleData);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await httpClient.PostAsync(roleEndpoint, content);

        if (response.IsSuccessStatusCode)
            logger.LogDebug("Keycloak role oluşturuldu: {RoleName}", permission.Name);
        else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            logger.LogDebug("Keycloak role zaten mevcut: {RoleName}", permission.Name);
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogWarning("Keycloak role oluşturulamadı: {RoleName}, Hata: {Error}", permission.Name, errorContent);
        }
    }

    public async Task AssignPermissionsToKeycloakUserAsync(string userId, IEnumerable<string> permissions)
    {
        try
        {
            var token = await GetKeycloakAdminTokenAsync();
            var keycloakOptions = configuration.GetSection("Keycloak")!;
            var authServerUrl = keycloakOptions["auth-server-url"]!;
            var realm = keycloakOptions["realm"]!;
            var clientId = keycloakOptions["client-id"]!;

            var keycloakClientId = await GetKeycloakClientIdAsync(token, authServerUrl, realm, clientId);

            var userRolesEndpoint = $"{authServerUrl}admin/realms/{realm}/users/{userId}/role-mappings/clients/{keycloakClientId}";
            
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var currentRolesResponse = await httpClient.GetAsync(userRolesEndpoint);
            
            var currentRoles = new List<string>();
            if (currentRolesResponse.IsSuccessStatusCode)
            {
                var currentRolesContent = await currentRolesResponse.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(currentRolesContent);
                currentRoles = [.. doc.RootElement.EnumerateArray().Select(role => role.GetProperty("name").GetString()!)];
            }
    
            var rolesToAdd = permissions.Where(p => !currentRoles.Contains(p)).ToList();
            
            if (rolesToAdd.Count != 0)
            {
                var rolesToAssign = new List<object>();
                foreach (var roleName in rolesToAdd)
                {
                    var roleEndpoint = $"{authServerUrl}admin/realms/{realm}/clients/{keycloakClientId}/roles/{roleName}";
                    var roleResponse = await httpClient.GetAsync(roleEndpoint);
                    
                    if (roleResponse.IsSuccessStatusCode)
                    {
                        var roleContent = await roleResponse.Content.ReadAsStringAsync();
                        using var roleDoc = JsonDocument.Parse(roleContent);
                        rolesToAssign.Add(new
                        {
                            id = roleDoc.RootElement.GetProperty("id").GetString(),
                            name = roleDoc.RootElement.GetProperty("name").GetString()
                        });
                    }
                }

                if (rolesToAssign.Count != 0)
                {
                    var assignRolesJson = JsonSerializer.Serialize(rolesToAssign);
                    var assignContent = new StringContent(assignRolesJson, System.Text.Encoding.UTF8, "application/json");
                    
                    var assignResponse = await httpClient.PostAsync(userRolesEndpoint, assignContent);
                    
                    if (assignResponse.IsSuccessStatusCode)
                    {
                        logger.LogInformation("User {UserId} için {Count} permission Keycloak'a atandı", userId, rolesToAssign.Count);
                    }
                    else
                    {
                        var errorContent = await assignResponse.Content.ReadAsStringAsync();
                        logger.LogWarning("User {UserId} için permission atama hatası: {Error}", userId, errorContent);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "User {UserId} için Keycloak permission atama hatası", userId);
            throw;
        }
    }
} 