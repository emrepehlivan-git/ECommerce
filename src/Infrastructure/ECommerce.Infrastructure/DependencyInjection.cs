using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Infrastructure.Services;
using ECommerce.Persistence.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Infrastructure.Logging;
using Serilog;
using ECommerce.SharedKernel.Logging;
using ECommerce.SharedKernel.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using OpenTelemetry.Exporter;

namespace ECommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDependencies(typeof(DependencyInjection).Assembly);
        services.AddLogging(configuration);
        services.AddObservability(configuration);

        // Add caching services
        services.AddMemoryCache();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        return services;
    }
    private static void AddLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LoggingOptions>(configuration.GetSection("LoggingOptions"));
        var loggingOptions = configuration.GetSection("LoggingOptions").Get<LoggingOptions>() ?? new LoggingOptions();

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(Enum.Parse<Serilog.Events.LogEventLevel>(loggingOptions.MinimumLevel, true))
            .Enrich.FromLogContext();

        if (loggingOptions.EnableConsole)
            loggerConfig = loggerConfig.WriteTo.Console(outputTemplate: loggingOptions.OutputTemplate);

        if (loggingOptions.EnableFile)
            loggerConfig = loggerConfig.WriteTo.File(loggingOptions.FilePath, rollingInterval: RollingInterval.Day, outputTemplate: loggingOptions.OutputTemplate);

        // Seq sink'ini ekle
        loggerConfig = loggerConfig.WriteTo.Seq(loggingOptions.SeqUrl);

        Log.Logger = loggerConfig.CreateLogger();
        
        services.AddSingleton<Application.Common.Logging.ILogger>(provider =>
                  new SerilogLogger(Log.Logger));
    }

    public static IServiceCollection AddObservability(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var serviceName = configuration["OpenTelemetry:ServiceName"] ?? "ECommerce.WebAPI";
        var serviceVersion = configuration["OpenTelemetry:ServiceVersion"] ?? "1.0.0";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "unknown",
                    ["service.instance.id"] = Environment.MachineName
                }))
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = FilterRequests;
                        options.EnrichWithHttpRequest = EnrichWithHttpRequest;
                        options.EnrichWithHttpResponse = EnrichWithHttpResponse;
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                    })
                    .AddSource("ECommerce.*");

                // Add exporters based on configuration
                var jaegerEndpoint = configuration["OpenTelemetry:Jaeger:Endpoint"];
                if (!string.IsNullOrEmpty(jaegerEndpoint))
                {
                    tracerProviderBuilder.AddJaegerExporter(options =>
                    {
                        options.AgentHost = configuration["OpenTelemetry:Jaeger:AgentHost"] ?? "localhost";
                        options.AgentPort = int.Parse(configuration["OpenTelemetry:Jaeger:AgentPort"] ?? "6831");
                    });
                }

                var otlpEndpoint = configuration["OpenTelemetry:OTLP:Endpoint"];
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    tracerProviderBuilder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            })
            .WithMetrics(meterProviderBuilder =>
            {
                meterProviderBuilder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter("ECommerce.*");

                // Add exporters
                meterProviderBuilder.AddPrometheusExporter();

                var otlpEndpoint = configuration["OpenTelemetry:OTLP:Endpoint"];
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    meterProviderBuilder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            });

        return services;
    }

    private static bool FilterRequests(HttpContext context)
    {
        // Filter out health checks and metrics endpoints
        var path = context.Request.Path.Value;
        return !string.IsNullOrEmpty(path) &&
               !path.StartsWith("/health") &&
               !path.StartsWith("/metrics") &&
               !path.StartsWith("/swagger");
    }

    private static void EnrichWithHttpRequest(Activity activity, HttpRequest request)
    {
        activity.SetTag("http.user_agent", request.Headers["User-Agent"].FirstOrDefault());
        activity.SetTag("http.client_ip", GetClientIpAddress(request));
        
        if (request.Headers.ContainsKey("X-Correlation-ID"))
        {
            activity.SetTag("correlation.id", request.Headers["X-Correlation-ID"].FirstOrDefault());
        }
    }

    private static void EnrichWithHttpResponse(Activity activity, HttpResponse response)
    {
        activity.SetTag("http.response.size", response.ContentLength);
    }

    private static string GetClientIpAddress(HttpRequest request)
    {
        return request.Headers["X-Forwarded-For"].FirstOrDefault() ??
               request.Headers["X-Real-IP"].FirstOrDefault() ??
               request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}