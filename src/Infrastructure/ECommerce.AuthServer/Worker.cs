using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ECommerce.AuthServer;

public class Worker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public Worker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        var client = await manager.FindByClientIdAsync("nextjs-client");
        if (client is not null)
        {
            await manager.DeleteAsync(client);
        }
        _ = await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "nextjs-client",
            DisplayName = "Next.js Client",
            ConsentType = ConsentTypes.Explicit,
            ClientType = ClientTypes.Public,
            RedirectUris =
        {
            new Uri("https://oauth.pstmn.io/v1/callback"),
            new Uri("http://localhost:3000/api/auth/callback/openiddict"),
            new Uri("https://oidcdebugger.com/debug"),
            new Uri("https://oauth.pstmn.io/v1/vscode-callback"),
        },
            PostLogoutRedirectUris = { new Uri("http://localhost:3000/") },
            Permissions =
        {
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.EndSession,
            Permissions.Endpoints.Token,
            Permissions.GrantTypes.AuthorizationCode,
            Permissions.ResponseTypes.Code,
            Permissions.Scopes.Email,
            Permissions.Scopes.Profile,
            Permissions.Scopes.Roles,
            $"{Permissions.Prefixes.Scope}api",
        },
            Requirements =
        {
            Requirements.Features.ProofKeyForCodeExchange
        }
        });

        var apiClient = await manager.FindByClientIdAsync("api");
        if (apiClient is not null)
        {
            await manager.DeleteAsync(apiClient);
        }
        _ = await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "api",
            ClientSecret = "api-secret",
            DisplayName = "API Client",
            ConsentType = ConsentTypes.Implicit,
            ClientType = ClientTypes.Confidential,
            Permissions =
            {
                Permissions.Endpoints.Introspection,
                $"{Permissions.Prefixes.Scope}api",
            }
        });

        var apiScope = await scopeManager.FindByNameAsync("api");
        if (apiScope is not null)
        {
            await scopeManager.DeleteAsync(apiScope);
        }
        await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
        {
            Name = "api",
            DisplayName = "API",
            Description = "API scope"
        });

        var swaggerClient = await manager.FindByClientIdAsync("swagger-client");
        if (swaggerClient is not null)
        {
            await manager.DeleteAsync(swaggerClient);
        }
        _ = await manager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "swagger-client",
            DisplayName = "Swagger UI",
            ConsentType = ConsentTypes.Implicit,
            ClientType = ClientTypes.Public,
            RedirectUris =
            {
                new Uri("https://localhost:4001/swagger/oauth2-redirect.html"),
                new Uri("http://localhost:4000/swagger/oauth2-redirect.html"),
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                $"{Permissions.Prefixes.Scope}api",
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange
            }
        });
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
