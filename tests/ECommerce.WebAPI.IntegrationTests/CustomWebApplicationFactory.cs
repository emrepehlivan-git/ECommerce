using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authentication;

namespace ECommerce.WebAPI.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public readonly PostgreSqlContainer DbContainer;

    public CustomWebApplicationFactory()
    {
        DbContainer = new PostgreSqlBuilder()
            .WithDatabase("ecommerce")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSQL"] = DbContainer.GetConnectionString()
            };
            config.AddInMemoryCollection(overrides!);
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, _ => { });
        });
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
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DbContainer.DisposeAsync();
    }
}
