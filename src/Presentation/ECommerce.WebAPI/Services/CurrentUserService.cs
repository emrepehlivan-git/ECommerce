using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using System.Security.Claims;

namespace ECommerce.WebAPI.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService, IScopedDependency
{
    public string? UserId
        => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public IEnumerable<string> GetPermissions()
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
        {
            return Enumerable.Empty<string>();
        }

        var permissionClaims = httpContextAccessor.HttpContext.User.Claims
            .Where(c => c.Type == "permissions")
            .Select(c => c.Value)
            .ToList();

        if (!permissionClaims.Any())
        {
            var resourceAccessClaim = httpContextAccessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == "resource_access");
            
            if (resourceAccessClaim != null)
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(resourceAccessClaim.Value);
                    var clientId = "ecommerce-api";
                    
                    if (doc.RootElement.TryGetProperty(clientId, out var clientElement) &&
                        clientElement.TryGetProperty("roles", out var rolesElement))
                    {
                        permissionClaims = rolesElement.EnumerateArray()
                            .Select(role => role.GetString()!)
                            .Where(role => !string.IsNullOrEmpty(role))
                            .ToList();
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                }
            }
        }

        return permissionClaims;
    }

    public bool HasPermission(string permission)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var permissions = GetPermissions();
        return permissions.Contains(permission);
    }

    public string? Email
        => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

    public string? Name
        => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

    public string? Role
        => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
}
