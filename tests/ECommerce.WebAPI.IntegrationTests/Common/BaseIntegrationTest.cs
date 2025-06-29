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
    }
    
    protected async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Veritabanındaki tüm tabloları temizle
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE cart_items CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE carts CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE product_stocks CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE products CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE categories CASCADE");
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE users CASCADE");
        
        await context.SaveChangesAsync();
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