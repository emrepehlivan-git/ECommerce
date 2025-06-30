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
            .WithDatabase("ecommerce_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testConnectionString = DbContainer.GetConnectionString();
            
            if (testConnectionString.Contains("localhost:5432") || 
                testConnectionString.Contains("Database=ecommerce;") ||
                testConnectionString.Contains("Database=ecommerce_prod"))
            {
                throw new InvalidOperationException("Connection string is not valid!");
            }
            
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = testConnectionString,
                ["ConnectionStrings:PostgreSQL"] = testConnectionString
            };
            
            config.Sources.Clear();
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
