using System.Security.Claims;
using System.Text.Json;

namespace ECommerce.Application.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var subjectId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return subjectId is null ? Guid.Empty : Guid.Parse(subjectId);
    }

    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Email);
    }

    public static string? GetFirstName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.GivenName);
    }

    public static string? GetLastName(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Surname);
    }

    public static string GetFullName(this ClaimsPrincipal principal)
    {
        var firstName = GetFirstName(principal);
        var lastName = GetLastName(principal);
        
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            return principal.FindFirstValue("name") ?? string.Empty;
        }

        return $"{firstName} {lastName}";
    }

    public static List<string> GetClientRoles(this ClaimsPrincipal principal)
    {
        var resourceAccessClaim = principal.FindFirstValue("resource_access");
        if (string.IsNullOrEmpty(resourceAccessClaim))
            return [];

        try
        {
            using var doc = JsonDocument.Parse(resourceAccessClaim);
            var allRoles = new List<string>();
            
            // Priority order: ecommerce-api, nextjs-client, swagger-client, then any other client
            var clientPriority = new[] { "ecommerce-api", "nextjs-client", "swagger-client" };
            
            foreach (var clientId in clientPriority)
            {
                if (doc.RootElement.TryGetProperty(clientId, out var clientElement) &&
                    clientElement.TryGetProperty("roles", out var rolesElement))
                {
                    var roles = rolesElement.EnumerateArray()
                        .Where(role => role.ValueKind == JsonValueKind.String)
                        .Select(role => role.GetString()!)
                        .Where(role => !string.IsNullOrEmpty(role))
                        .ToList();
                    
                    allRoles.AddRange(roles);
                }
            }
            
            // Check for any other clients not in priority list
            foreach (var property in doc.RootElement.EnumerateObject())
            {
                if (!clientPriority.Contains(property.Name) && 
                    property.Value.TryGetProperty("roles", out var rolesElement))
                {
                    var roles = rolesElement.EnumerateArray()
                        .Where(role => role.ValueKind == JsonValueKind.String)
                        .Select(role => role.GetString()!)
                        .Where(role => !string.IsNullOrEmpty(role))
                        .ToList();
                    
                    allRoles.AddRange(roles);
                }
            }
            
            return allRoles.Distinct().ToList();
        }
        catch (JsonException)
        {
        }

        return [];
    }
} 