using Microsoft.AspNetCore.Hosting;
using Testcontainers.PostgreSql;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authentication;
using ECommerce.WebAPI.IntegrationTests.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ECommerce.Application.Services;
using Moq;

namespace ECommerce.WebAPI.IntegrationTests;

public class TestKeycloakPermissionSyncService : IKeycloakPermissionSyncService
{
    public Task AssignPermissionsToKeycloakUserAsync(string userId, IEnumerable<string> permissions)
    {
        return Task.CompletedTask;
    }

    public Task SyncPermissionsToKeycloakAsync()
    {
        return Task.CompletedTask;
    }

    public Task UpdateRolePermissionsInKeycloakAsync(string roleName, List<string> permissions)
    {
        return Task.CompletedTask;
    }
}

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public readonly PostgreSqlContainer DbContainer;

    public CustomWebApplicationFactory()
    {
        DbContainer = new PostgreSqlBuilder()
            .WithDatabase("ecommerce_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using an in-memory database for testing
            services.AddDbContextPool<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(DbContainer.GetConnectionString());
            });

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();

            services.AddAuthentication(defaultScheme: TestAuthHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, _ => { });

            // Add the permission-based authorization infrastructure for tests
            services.AddSingleton<IAuthorizationPolicyProvider, TestPermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, TestPermissionAuthorizationHandler>();

            // Replace CurrentUserService with test implementation
            services.RemoveAll<ICurrentUserService>();
            services.AddScoped<ICurrentUserService, TestCurrentUserService>();
            
            services.RemoveAll<IKeycloakPermissionSyncService>();
            services.AddScoped<IKeycloakPermissionSyncService, TestKeycloakPermissionSyncService>();

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(TestAuthHandler.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });
        });

        builder.UseEnvironment("Testing");
    }

    public async Task ApplyMigrations()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public HttpClient CreateUnauthenticatedClient()
    {
        return this.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Clear all authentication and authorization services for an unauthenticated client
                services.RemoveAll<Microsoft.AspNetCore.Authentication.IAuthenticationService>();
                services.RemoveAll<IAuthenticationSchemeProvider>();
                services.RemoveAll<IAuthenticationHandlerProvider>();
                services.RemoveAll<IAuthorizationPolicyProvider>();
                services.RemoveAll<IAuthorizationHandler>();

                // Add minimal authentication that doesn't auto-authenticate and a default authorization
                services.AddAuthentication("NoAuth")
                    .AddScheme<AuthenticationSchemeOptions, NoAuthHandler>("NoAuth", _ => { });
                
                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder("NoAuth")
                        .RequireAuthenticatedUser()
                        .Build();
                });
            });
        }).CreateClient();
    }

    public async Task InitializeAsync()
    {
        await DbContainer.StartAsync();
        
        var connectionString = DbContainer.GetConnectionString();
        if (!connectionString.Contains("ecommerce_test"))
        {
            throw new InvalidOperationException("Test container correctly started!");
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DbContainer.DisposeAsync();
    }
}
