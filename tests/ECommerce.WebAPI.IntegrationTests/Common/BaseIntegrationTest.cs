using Microsoft.AspNetCore.Identity;

namespace ECommerce.WebAPI.IntegrationTests.Common;

public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected HttpClient Client { get; set; } = default!;

    protected BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = Factory.CreateClient();
        
        VerifyTestDatabase();
    }
    
    private void VerifyTestDatabase()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var connectionString = context.Database.GetConnectionString();
        
        if (connectionString != null &&
            (connectionString.Contains("Database=ecommerce;") ||
             connectionString.Contains("localhost:5432") ||
             connectionString.Contains("ecommerce_prod") ||
             !connectionString.Contains("ecommerce_test")))
        {
            throw new InvalidOperationException(
                $"TEHLIKE! Integration test gerçek veritabanına bağlanmaya çalışıyor: {connectionString}. " +
                "Bu kesinlikle engellenmelidir!");
        }
    }

    protected async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        await context.Database.EnsureDeletedAsync();
        
        await context.Database.EnsureCreatedAsync();
        
        await CreateTestUserAsync();
    }

    protected async Task CreateTestUserAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var user = new User
        {
            Id = Guid.Parse(TestAuthHandler.TestUserId),
            UserName = "testuser",
            NormalizedUserName = "TESTUSER",
            Email = "testuser@example.com",
            NormalizedEmail = "TESTUSER@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        user.UpdateName("Test", "User");
        user.Activate();
        
        var result = await userManager.CreateAsync(user, "Password123!");

        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Unable to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
} 