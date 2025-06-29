using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ECommerce.WebAPI.IntegrationTests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "Test";
    public static readonly string UserId = Guid.NewGuid().ToString();

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    private static ClaimsPrincipal? _claimsPrincipal;
    
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (_claimsPrincipal is not null)
        {
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(_claimsPrincipal, AuthenticationScheme)));
        }
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, UserId),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim("sub", UserId)
        };
        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        _claimsPrincipal = principal;
        
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    public static void ResetClaims()
    {
        _claimsPrincipal = null;
    }
} 