using ECommerce.WebAPI;
using ECommerce.Application.Common.Logging;
using ECommerce.Infrastructure.Services;

IECommerceLogger<Program>? logger = null;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddApiServices(builder.Configuration);

    var app = builder.Build();

    logger = app.Services.GetRequiredService<IECommerceLogger<Program>>();
    
    await app.ApplyMigrations();

    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var permissionSeedingService = scope.ServiceProvider.GetRequiredService<PermissionSeedingService>();
            var result = await permissionSeedingService.SeedPermissionsAsync();
            app.Logger.LogInformation("Permission seeding: {Result}", result);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Permission seeding failed");
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