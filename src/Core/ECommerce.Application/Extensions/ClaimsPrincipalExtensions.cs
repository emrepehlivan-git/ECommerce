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

    public static List<string> GetRealmRoles(this ClaimsPrincipal principal)
    {
        var realmAccessClaim = principal.FindFirstValue("realm_access");
        if (string.IsNullOrEmpty(realmAccessClaim))
            return [];

        try
        {
            using var doc = JsonDocument.Parse(realmAccessClaim);
            if (doc.RootElement.TryGetProperty("roles", out var rolesElement))
            {
                return [.. rolesElement.EnumerateArray()
                    .Where(role => role.ValueKind == JsonValueKind.String)
                    .Select(role => role.GetString()!)
                    .Where(role => !string.IsNullOrEmpty(role))];
            }
        }
        catch (JsonException)
        {
        }

        return [];
    }

    public static List<string> GetClientRoles(this ClaimsPrincipal principal, string clientId = "nextjs-client")
    {
        var resourceAccessClaim = principal.FindFirstValue("resource_access");
        if (string.IsNullOrEmpty(resourceAccessClaim))
            return [];

        try
        {
            using var doc = JsonDocument.Parse(resourceAccessClaim);
            if (doc.RootElement.TryGetProperty(clientId, out var clientElement) &&
                clientElement.TryGetProperty("roles", out var rolesElement))
            {
                return [.. rolesElement.EnumerateArray()
                    .Where(role => role.ValueKind == JsonValueKind.String)
                    .Select(role => role.GetString()!)
                    .Where(role => !string.IsNullOrEmpty(role))];
            }
        }
        catch (JsonException)
        {
        }

        return [];
    }

    public static List<string> GetAllKeycloakRoles(this ClaimsPrincipal principal, string clientId = "nextjs-client")
    {
        // Sadece client rollerini döndür
        return principal.GetClientRoles(clientId);
    }
} 