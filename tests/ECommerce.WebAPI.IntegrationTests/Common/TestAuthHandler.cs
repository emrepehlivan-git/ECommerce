using System.Security.Claims;
using System.Text.Encodings.Web;
using ECommerce.Application.Common.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.WebAPI.IntegrationTests.Common;

public class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string AuthenticationScheme = "Test";
    public const string TestUserId = "d5c6b8a0-7e1a-4b9a-9b3a-1b2c3d4e5f6a";
    
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, "Test user"),
            new (ClaimTypes.NameIdentifier, TestUserId),
            new (ClaimTypes.Role, "admin"),
            
            new ("Permission", PermissionConstants.Products.Create),
            new ("Permission", PermissionConstants.Products.Update),
            new ("Permission", PermissionConstants.Products.Delete),
            new ("Permission", PermissionConstants.Products.Manage),
            
            new ("Permission", PermissionConstants.Orders.View),
            new ("Permission", PermissionConstants.Orders.Create),
            new ("Permission", PermissionConstants.Orders.Update),
            new ("Permission", PermissionConstants.Orders.Delete),
            new ("Permission", PermissionConstants.Orders.Manage),
            
            new ("Permission", PermissionConstants.Categories.Create),
            new ("Permission", PermissionConstants.Categories.Update),
            new ("Permission", PermissionConstants.Categories.Delete),
            new ("Permission", PermissionConstants.Categories.Manage),
            
            new ("Permission", PermissionConstants.Users.View),
            new ("Permission", PermissionConstants.Users.Create),
            new ("Permission", PermissionConstants.Users.Update),
            new ("Permission", PermissionConstants.Users.Delete),
            new ("Permission", PermissionConstants.Users.Manage),

            new ("Permission", PermissionConstants.AdminPanel.Access),

            new ("Permission", PermissionConstants.Roles.Read),
            new ("Permission", PermissionConstants.Roles.View),
            new ("Permission", PermissionConstants.Roles.Create),
            new ("Permission", PermissionConstants.Roles.Update),
            new ("Permission", PermissionConstants.Roles.Delete),
            new ("Permission", PermissionConstants.Roles.Manage)
        };
        
        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
} 