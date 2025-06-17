using ECommerce.WebAPI;
using ECommerce.Persistence.Seeds;


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

    app.UsePresentation(app.Environment);

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application terminated unexpectedly: {ex}");
    throw;
}

public partial class Program { }