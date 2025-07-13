using ECommerce.Application.Common.Logging;
using ECommerce.Persistence.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Persistence.Extensions;

public static class DatabaseSeederExtensions
{
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<IECommerceLogger<DataSeeder>>();
        
        var seeder = new DataSeeder(context, logger);
        await seeder.SeedAllAsync();
    }

    public static async Task SeedDatabaseAsync(this ApplicationDbContext context, IECommerceLogger<DataSeeder> logger)
    {
        var seeder = new DataSeeder(context, logger);
        await seeder.SeedAllAsync();
    }
}