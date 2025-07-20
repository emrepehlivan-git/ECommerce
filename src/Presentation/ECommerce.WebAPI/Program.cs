using ECommerce.WebAPI;
using ECommerce.Application.Common.Logging;
using ECommerce.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;

IECommerceLogger<Program>? logger = null;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddApiServices(builder.Configuration);

    var app = builder.Build();

    logger = app.Services.GetRequiredService<IECommerceLogger<Program>>();
    
    if (!app.Environment.IsEnvironment("Testing"))
    {
        await app.ApplyMigrations();
        await app.ConfigurePermissions();

        // Seed işlemini sadece development environment'ta ve veri yoksa çalıştır
        if (app.Environment.IsDevelopment())
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ECommerce.Persistence.Contexts.ApplicationDbContext>();
                var needsSeeding = !await context.Categories.AnyAsync() || !await context.Products.AnyAsync();
                
                if (needsSeeding)
                {
                    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                    await seeder.SeedAsync();
                }
                else
                {
                    logger.LogInformation("Data already exists, skipping seeding.");
                }
            }
        }
    }

    app.UsePresentation(app.Environment);


    app.Run();
}
catch (Exception ex)
{
    if (logger != null)
    {
        logger.LogError(ex, "Application terminated unexpectedly: {Message}", ex.Message);
    }
    else
    {
        Console.WriteLine($"Application terminated unexpectedly: {ex.Message}");
        Console.WriteLine(ex.ToString());
    }
    throw;
}

public partial class Program { }