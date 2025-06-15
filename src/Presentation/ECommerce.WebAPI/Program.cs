using ECommerce.WebAPI;
using ECommerce.Persistence.Seeds;

ECommerce.Application.Common.Logging.ILogger? logger = null;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddPresentation(builder.Configuration);

    var app = builder.Build();

    await app.ApplyMigrations();

    if (app.Environment.IsDevelopment())
    {
        await app.SeedDatabaseAsync();
    }

    logger = app.Services.GetService<ECommerce.Application.Common.Logging.ILogger>();

    app.UsePresentation(app.Environment);

    app.Run();
}
catch (Exception ex)
{
    logger?.LogError(ex, "Application terminated unexpectedly");
}

public partial class Program { }