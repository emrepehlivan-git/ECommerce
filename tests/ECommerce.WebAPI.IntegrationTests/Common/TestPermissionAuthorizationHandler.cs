using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ECommerce.WebAPI.IntegrationTests.Common;

public class TestPermissionAuthorizationHandler : AuthorizationHandler<TestPermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TestPermissionRequirement requirement)
    {
        if (context.User == null || !context.User.Identity!.IsAuthenticated)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Check if the user has the required permission
        var permissions = context.User.FindAll("Permission").Select(c => c.Value);
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}