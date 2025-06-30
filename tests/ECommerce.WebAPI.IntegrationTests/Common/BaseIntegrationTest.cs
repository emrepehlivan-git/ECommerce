using ECommerce.Domain.Entities;
using ECommerce.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.WebAPI.IntegrationTests.Common;

public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        
        // GÜVENLİK KONTROLÜ: Gerçek veritabanına bağlanmayı engelle
        VerifyTestDatabase();
    }
    
    private void VerifyTestDatabase()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var connectionString = context.Database.GetConnectionString();
        
        // ASLA gerçek veritabanına bağlanmamalı
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
        
        // Bir kez daha güvenlik kontrolü
        VerifyTestDatabase();
        
        // Testcontainer ile çalışırken database'i tamamen temizleyip yeniden oluşturuyoruz
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // Migration'ları uygula
        await context.Database.MigrateAsync();
    }
    
    protected async Task<User> CreateUserAsync(string email = "testuser@example.com")
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var userId = Guid.Parse(TestAuthHandler.UserId);
        
        // Önce kullanıcının var olup olmadığını kontrol et
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (existingUser != null)
        {
            return existingUser;
        }

        var user = User.Create(email, "Test", "User");
        user.Id = userId;
        
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        return user;
    }
} 