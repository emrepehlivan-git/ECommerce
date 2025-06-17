using System.Security.Claims;
using ECommerce.Application.Interfaces;
using ECommerce.SharedKernel;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ECommerce.WebAPI.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService, IScopedDependency
{
    public string? UserId
        => httpContextAccessor.HttpContext?.User.FindFirst(Claims.Subject)?.Value;

    public IEnumerable<string> GetPermissions()
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
        {
            return Enumerable.Empty<string>();
        }

        var permissionClaims = httpContextAccessor.HttpContext.User.Claims
            .Where(c => c.Type == "permissions")
            .Select(c => c.Value);

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
        => httpContextAccessor.HttpContext?.User.FindFirst(Claims.Email)?.Value;

    public string? Name
        => httpContextAccessor.HttpContext?.User.FindFirst(Claims.Name)?.Value;

    public string? Role
        => httpContextAccessor.HttpContext?.User.FindFirst(Claims.Role)?.Value;
}
