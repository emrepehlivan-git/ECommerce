using ECommerce.WebAPI;
using ECommerce.Application.Common.Logging;
using ECommerce.Persistence.Seeders;

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

        // Seed işlemini otomatik başlat
        using (var scope = app.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync();
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