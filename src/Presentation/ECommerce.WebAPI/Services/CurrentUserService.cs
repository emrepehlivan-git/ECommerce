using ECommerce.Application.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Extensions;
using System.Security.Claims;

namespace ECommerce.WebAPI.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor, IPermissionService permissionService) : ICurrentUserService, IScopedDependency
{
    public string? UserId
        => httpContextAccessor.HttpContext?.User.GetUserId().ToString();

    public IEnumerable<string> GetPermissions()
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
        {
            return [];
        }

        var userRoles = httpContextAccessor.HttpContext.User.GetClientRoles();
        if (userRoles.Any(role => role.Equals("admin", StringComparison.OrdinalIgnoreCase) || 
                                 role.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
        {
            return GetAllSystemPermissions();
        }

        var userIdClaim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Enumerable.Empty<string>();
        }

        try
        {
            var permissionsTask = permissionService.GetUserPermissionsAsync(userId);
            var permissions = permissionsTask.GetAwaiter().GetResult();
            return permissions;
        }
        catch (Exception)
        {
            return [];
        }
    }

    public bool HasPermission(string permission)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var userRoles = httpContextAccessor.HttpContext.User.GetClientRoles();
        if (userRoles.Any(role => role.Equals("admin", StringComparison.OrdinalIgnoreCase) || 
                                 role.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        var permissions = GetPermissions();
        return permissions.Contains(permission);
    }

    public string? Email
        => httpContextAccessor.HttpContext?.User.GetEmail();

    public string? Name
        => httpContextAccessor.HttpContext?.User.GetFullName();

    public List<string> Roles
        => httpContextAccessor.HttpContext?.User.GetClientRoles() ?? [];
    
    private static List<string> GetAllSystemPermissions()
    {
        var permissions = new List<string>();

        var permissionTypes = typeof(PermissionConstants).GetNestedTypes();
        
        foreach (var type in permissionTypes)
        {
            var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string))
                {
                    var permissionValue = (string?)field.GetValue(null);
                    if (!string.IsNullOrEmpty(permissionValue))
                    {
                        permissions.Add(permissionValue);
                    }
                }
            }
        }

        return permissions;
    }
}
