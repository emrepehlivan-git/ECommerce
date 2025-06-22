using ECommerce.WebAPI;
using ECommerce.Application.Common.Logging;

IECommerLogger<Program>? logger = null;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddPresentation(builder.Configuration);

    var app = builder.Build();

    logger = app.Services.GetRequiredService<IECommerLogger<Program>>();
    
    await app.ApplyMigrations();
    await app.UsePresentation(app.Environment);

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