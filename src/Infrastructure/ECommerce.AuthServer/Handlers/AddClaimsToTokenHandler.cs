using System.Security.Claims;
using ECommerce.Application.Features.Categories.Queries;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.AuthServer.Controllers;
using ECommerce.AuthServer.Helpers;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

public class AddClaimsToTokenHandler(IUserService userService, IRoleService roleService, IPermissionService permissionService, IOpenIddictScopeManager scopeManager)
: IOpenIddictServerHandler<ProcessSignInContext>
{
    public async ValueTask HandleAsync(ProcessSignInContext context)
    {
        var principal = context.Principal;

        if (principal is { Identity: { IsAuthenticated: false } })
            return;

        if (!Guid.TryParse(principal?.GetClaim(Claims.Subject), out var userId)) return;

        var user = await userService.FindByIdAsync(userId);
        if (user is null) return;

        var identity = principal?.Identity as ClaimsIdentity;
        var permissions = await permissionService.GetUserPermissionsAsync(user.Id);

        identity?.SetClaim(Claims.Subject, user.Id.ToString());
        identity?.SetClaim(Claims.Audience, "api");
        identity?.SetClaim(Claims.Email, user.Email);
        identity?.SetClaim("fullName", user.FullName.ToString());
        identity?.SetClaims(Claims.Role, [.. await roleService.GetUserRolesAsync(user)]);
        identity?.SetClaims("permissions", [.. permissions]);
        identity?.SetScopes(context.Request.GetScopes());
        identity?.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());
        identity?.SetDestinations(AuthorizationController.GetDestinations);
    }

    public static OpenIddictServerHandlerDescriptor Descriptor { get; }
          = OpenIddictServerHandlerDescriptor.CreateBuilder<ProcessSignInContext>()
              .UseScopedHandler<AddClaimsToTokenHandler>()
              .SetOrder(100_000)
              .Build();
}
