using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Infrastructure.Services;
using ECommerce.Infrastructure.Messaging;
using ECommerce.Persistence.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Infrastructure.Logging;
using Serilog;
using ECommerce.SharedKernel.Logging;

namespace ECommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfraServices();
        services.AddLogging(configuration);
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        return services;
    }

    private static void AddInfraServices(this IServiceCollection services)
    {
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddSingleton<IEventBusFactory, EventBusFactory>();
        services.AddSingleton<IEventBus>(sp =>
            sp.GetRequiredService<IEventBusFactory>().Create(sp));
    }

    private static void AddLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LoggingOptions>(configuration.GetSection("LoggingOptions"));
        var loggingOptions = configuration.GetSection("LoggingOptions").Get<LoggingOptions>() ?? new LoggingOptions();

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(Enum.Parse<Serilog.Events.LogEventLevel>(loggingOptions.MinimumLevel, true));

        if (loggingOptions.EnableConsole)
            loggerConfig = loggerConfig.WriteTo.Console(outputTemplate: loggingOptions.OutputTemplate);

        if (loggingOptions.EnableFile)
            loggerConfig = loggerConfig.WriteTo.File(loggingOptions.FilePath, rollingInterval: RollingInterval.Day, outputTemplate: loggingOptions.OutputTemplate);

        Log.Logger = loggerConfig
        .WriteTo.Seq(loggingOptions.SeqUrl)
        .Enrich.FromLogContext()
        .CreateLogger();
        services.AddSingleton<Application.Common.Logging.ILogger>(provider =>
                  new SerilogLogger(Log.Logger));
    }
}