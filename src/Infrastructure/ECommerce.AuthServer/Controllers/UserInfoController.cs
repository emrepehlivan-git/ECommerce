using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ECommerce.AuthServer.Controllers;

public sealed class UserInfoController(IUserService userService, IRoleService roleService, IPermissionService permissionService) : Controller
{
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo"), HttpPost("~/connect/userinfo"), Produces("application/json")]
    public async Task<IActionResult> UserInfo()
    {

        var user = await userService.FindByIdAsync(Guid.Parse(User.GetClaim(Claims.Subject) ?? string.Empty));
        if (user is null)

        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The specified access token is bound to an account that no longer exists."
                }!));
        }

        var claims = new Dictionary<string, object>
        {
            [Claims.Subject] = user.Id,
            [Claims.Email] = user.Email!,
            [Claims.Role] = await roleService.GetUserRolesAsync(user),
            [Claims.Audience] = "api",
            ["fullName"] = user.FullName.ToString(),
            ["permissions"] = await permissionService.GetUserPermissionsAsync(user.Id),
        };

        return Ok(claims);
    }

}
