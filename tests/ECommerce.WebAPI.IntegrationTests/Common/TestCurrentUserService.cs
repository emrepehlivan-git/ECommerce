using ECommerce.Application.Services;
using ECommerce.Application.Common.Constants;

namespace ECommerce.WebAPI.IntegrationTests.Common;

public class TestCurrentUserService : ICurrentUserService
{
    public string? UserId => TestAuthHandler.TestUserId;

    public string? Email => "testuser@example.com";

    public string? Name => "Test User";

    public List<string> Roles => ["admin"];

    public IEnumerable<string> GetPermissions()
    {
        // Return all permissions for the test user
        return GetAllSystemPermissions();
    }

    public bool HasPermission(string permission)
    {
        // Test user has all permissions
        return true;
    }

    private static IEnumerable<string> GetAllSystemPermissions()
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
                    var permissionValue = (string)field.GetValue(null)!;
                    permissions.Add(permissionValue);
                }
            }
        }
        
        return permissions;
    }
}