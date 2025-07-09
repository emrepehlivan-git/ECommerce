using Microsoft.AspNetCore.Hosting;
using Testcontainers.PostgreSql;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authentication;
using ECommerce.WebAPI.IntegrationTests.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Internal;

namespace ECommerce.WebAPI.IntegrationTests;

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
