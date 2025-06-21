using System.Security.Claims;
using ECommerce.Application.Services;

namespace ECommerce.AuthServer.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId
        => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? Email
        => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

    public string? Name
        => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

    public string? Role => throw new NotImplementedException();

    public IEnumerable<string> GetPermissions()
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
        {
            return Enumerable.Empty<string>();
        }

        var permissionClaims = httpContextAccessor.HttpContext.User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value);

        return permissionClaims;
    }

    public bool HasPermission(string permission)
    {
        var permissions = GetPermissions();
        return permissions.Contains(permission);
    }
}