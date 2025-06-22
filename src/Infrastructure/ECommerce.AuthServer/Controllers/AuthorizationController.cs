using System.Security.Claims;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.AuthServer.Helpers;
using ECommerce.AuthServer.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ECommerce.AuthServer.Controllers;

public class AuthorizationController(
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictAuthorizationManager authorizationManager,
    IUserService userService,
    Application.Services.IAuthenticationService authenticationService)
    : Controller
{
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var result = await HttpContext.AuthenticateAsync();
        if (result == null || !result.Succeeded || request.HasPromptValue(PromptValues.Login) ||
           (request.MaxAge != null && result.Properties?.IssuedUtc != null &&
            DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
        {
            if (request.HasPromptValue(PromptValues.None))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                    }!));
            }

            var prompt = string.Join(" ", request.GetPromptValues().Remove(PromptValues.Login));

            var parameters = Request.HasFormContentType ?
                Request.Form.Where(parameter => parameter.Key != Parameters.Prompt).ToList() :
                Request.Query.Where(parameter => parameter.Key != Parameters.Prompt).ToList();

            parameters.Add(KeyValuePair.Create(Parameters.Prompt, new StringValues(prompt)));

            return Challenge(new AuthenticationProperties
            {
                RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters)
            });
        }

        var user = await userService.GetUserByPrincipalAsync(result.Principal) ??
            throw new InvalidOperationException("The user details cannot be retrieved.");
        var clientId = request.ClientId ?? throw new InvalidOperationException("The client ID cannot be retrieved.");

        var application = await applicationManager.FindByClientIdAsync(clientId) ??
            throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        var authorizations = await authorizationManager.FindAsync(
            subject: user.Id.ToString(),
            client: await applicationManager.GetIdAsync(application),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        switch (await applicationManager.GetConsentTypeAsync(application))
        {
            case ConsentTypes.External when authorizations.Count is 0:
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The logged in user is not allowed to access this client application."
                    }!));

            case ConsentTypes.Implicit:
            case ConsentTypes.External when authorizations.Count is not 0:
            case ConsentTypes.Explicit when authorizations.Count is not 0 && !request.HasPromptValue(PromptValues.Consent):
                var identity = new ClaimsIdentity(
                    authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                    nameType: Claims.Name,
                    roleType: Claims.Role);

                identity.SetClaim(Claims.Subject, user.Id.ToString());
                

                var authorization = authorizations.LastOrDefault();
                authorization ??= await authorizationManager.CreateAsync(
                    identity: identity,
                    subject: user.Id.ToString(),
                    client: await applicationManager.GetIdAsync(application) ?? throw new InvalidOperationException("The client ID cannot be retrieved."),
                    type: AuthorizationTypes.Permanent,
                    scopes: identity.GetScopes());

                identity.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization));

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            case ConsentTypes.Explicit when request.HasPromptValue(PromptValues.None):
            case ConsentTypes.Systematic when request.HasPromptValue(PromptValues.None):
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "Interactive user consent is required."
                    }!));

            default:
                return View(new ConsentViewModel
                {
                    ApplicationName = await applicationManager.GetLocalizedDisplayNameAsync(application) ?? string.Empty,
                    Scope = request.Scope ?? string.Empty
                });
        }
    }

    [Authorize, FormValueRequired("submit.Accept")]
    [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var user = await userService.GetUserByPrincipalAsync(User) ??
            throw new InvalidOperationException("The user details cannot be retrieved.");

        var application = await applicationManager.FindByClientIdAsync(request.ClientId!) ??
            throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        var authorizations = await authorizationManager.FindAsync(
            subject: user.Id.ToString(),
            client: await applicationManager.GetIdAsync(application),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        if (authorizations.Count is 0 && await applicationManager.HasConsentTypeAsync(application, ConsentTypes.External))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The logged in user is not allowed to access this client application."
                }!));
        }

        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, user.Id.ToString());
        
        var authorization = authorizations.LastOrDefault();
        authorization ??= await authorizationManager.CreateAsync(
            identity: identity,
            subject: user.Id.ToString(),
            client: await applicationManager.GetIdAsync(application) ?? throw new InvalidOperationException("The client ID cannot be retrieved."),
            type: AuthorizationTypes.Permanent,
            scopes: identity.GetScopes());

        identity.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization));

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [Authorize, FormValueRequired("submit.Deny")]
    [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
    public IActionResult Deny() => Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

    [HttpGet("~/connect/logout")]
    public IActionResult Logout() => View();

    [ActionName(nameof(Logout)), HttpPost("~/connect/logout"), ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutPost()
    {
        await authenticationService.SignOutAsync();

        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = "/"
            });
    }

    [HttpPost("~/connect/token"), IgnoreAntiforgeryToken, Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme) ??
                throw new InvalidOperationException("The user details cannot be retrieved.");

            var user = await userService.FindByIdAsync(Guid.Parse(result.Principal?.GetClaim(Claims.Subject) ?? string.Empty));
            if (user is null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                    }!));
            }

            if (!await userService.CanSignInAsync(user))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                    }!));
            }

            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.SetClaim(Claims.Subject, user.Id.ToString());
            

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    public static IEnumerable<string> GetDestinations(Claim claim)
    {
        switch (claim.Type)
        {
            case Claims.Name or "fullName":
                yield return Destinations.AccessToken;

                if (claim.Subject?.HasScope(Scopes.Profile) == true)
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;

                if (claim.Subject?.HasScope(Scopes.Email) == true)
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;

                if (claim.Subject?.HasScope(Scopes.Roles) == true)
                    yield return Destinations.IdentityToken;

                yield break;

            case "permissions":
                yield return Destinations.AccessToken;
                yield break;

            case "AspNet.Identity.SecurityStamp":
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}