using ECommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace ECommerce.WebAPI.Authorization;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

public sealed class PermissionAuthorizationHandler(ICurrentUserService currentUserService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        Console.WriteLine($"Permission check: {requirement.Permission}");
        Console.WriteLine($"User authenticated: {context.User.Identity?.IsAuthenticated}");
        Console.WriteLine($"User identity type: {context.User.Identity?.GetType().Name}");
        Console.WriteLine($"Claims count: {context.User.Claims.Count()}");
        Console.WriteLine($"Authentication type: {context.User.Identity?.AuthenticationType}");
        Console.WriteLine($"User name: {context.User.Identity?.Name}");
        
        if (context.User.Identity?.IsAuthenticated != true)
        {
            Console.WriteLine("Authentication failed in permission handler");
            context.Fail();
            return Task.CompletedTask;
        }

        if (currentUserService.HasPermission(requirement.Permission))
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