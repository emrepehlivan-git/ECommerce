using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using ECommerce.Application.Common.Constants;

namespace ECommerce.WebAPI.IntegrationTests.Common;

public class TestPermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _defaultProvider;

    public TestPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _defaultProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _defaultProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _defaultProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if this is a permission-based policy
        if (IsPermissionPolicy(policyName))
        {
            var policy = new AuthorizationPolicyBuilder(TestAuthHandler.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .AddRequirements(new TestPermissionRequirement(policyName))
                .Build();
            
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _defaultProvider.GetPolicyAsync(policyName);
    }

    private static bool IsPermissionPolicy(string policyName)
    {
        // Check if the policy name matches any of our permission constants
        var permissionTypes = typeof(PermissionConstants).GetNestedTypes();
        foreach (var type in permissionTypes)
        {
            var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string))
                {
                    var permissionValue = (string)field.GetValue(null)!;
                    if (permissionValue == policyName)
                        return true;
                }
            }
        }
        return false;
    }
}

public record TestPermissionRequirement(string Permission) : IAuthorizationRequirement;