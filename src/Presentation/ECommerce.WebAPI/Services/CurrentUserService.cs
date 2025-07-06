using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using System.Security.Claims;

namespace ECommerce.WebAPI.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor, IPermissionService permissionService) : ICurrentUserService, IScopedDependency
{
    public string? UserId
        => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public IEnumerable<string> GetPermissions()
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
        {
            return Enumerable.Empty<string>();
        }

        var userIdClaim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Enumerable.Empty<string>();
        }

        try
        {
            // Async method'u sync olarak çağırıyoruz - bu ideal değil ama mevcut interface'i koruyoruz
            var permissionsTask = permissionService.GetUserPermissionsAsync(userId);
            var permissions = permissionsTask.GetAwaiter().GetResult();
            return permissions;
        }
        catch (Exception)
        {
            return Enumerable.Empty<string>();
        }
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
