using System.Globalization; 
using ECommerce.Application;
using ECommerce.Application.Constants;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure;
using ECommerce.Persistence;
using ECommerce.Persistence.Contexts;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.WebAPI.Authorization;
using ECommerce.WebAPI.Middlewares;
using ECommerce.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;

namespace ECommerce.WebAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureLocalization(services);
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins", builder =>
            {
                builder.WithOrigins(
                    "http://localhost:3000", 
                    "https://localhost:3000",
                    "https://localhost:5002",
                    "http://localhost:5001",
                    "http://localhost:4000",
                    "https://localhost:4001"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            });
        });
        
        ConfigureSwagger(services, configuration);

        services.AddApplication()
            .AddInfrastructure(configuration)
            .AddPersistence(configuration);
        services.AddDependencies(typeof(DependencyInjection).Assembly);

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddHttpContextAccessor();
        services.AddProblemDetails();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        ConfigureOpenIddict(services, configuration);

        services.AddAuthorization();

        return services;
    }

    public static WebApplication UsePresentation(this WebApplication app, IWebHostEnvironment environment)
    {
        app.UseRequestLocalization();
        
        app.UseCors("AllowAllOrigins");

        if (environment.IsDevelopment())
        {
            app.UseSwagger(c => {
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce API v1");
                options.OAuthClientId("swagger-client");
                options.OAuthUsePkce();
                options.OAuthScopeSeparator(" ");
            });
        }

        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

    private static void AddAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            var permissionTypes = typeof(PermissionConstants).GetNestedTypes();
            foreach (var type in permissionTypes)
            {
                var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(string))
                    {
                        var permissionValue = (string)field.GetValue(null)!;
                        options.AddPolicy(permissionValue, policy =>
                            policy.AddRequirements(new PermissionRequirement(permissionValue)));
                    }
                }
            }
        });
    }

    public static async Task ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
            await dbContext.Database.MigrateAsync();
    }

    private static void ConfigureOpenIddict(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenIddict()
            .AddValidation(options =>
            {
                options.SetIssuer(new Uri(configuration["Authentication:Authority"]!));
                options.AddAudiences(configuration["Authentication:Audience"]!);

                options.UseSystemNetHttp(httpOptions =>
                {
                    httpOptions.ConfigureHttpClientHandler(handler =>
                    {
                        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                    });
                });

                options.UseAspNetCore();

            });
    }

    private static void ConfigureSwagger(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "ECommerce API",
                Version = "v1",
                Description = "ECommerce API with OpenIddict Authentication"
            });
            
            var authServerUrl = configuration["Authentication:SwaggerAuthority"] ?? "https://localhost:5002";
            
            options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.OAuth2,
                Flows = new Microsoft.OpenApi.Models.OpenApiOAuthFlows
                {
                    AuthorizationCode = new Microsoft.OpenApi.Models.OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{authServerUrl}/connect/authorize"),
                        TokenUrl = new Uri($"{authServerUrl}/connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "api", "API Access" },
                            { "openid", "OpenID" },
                            { "profile", "Profile" },
                            { "email", "Email" }
                        }
                    }
                }
            });
            
            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    new[] { "api" , "openid" , "profile" , "email" , "address" , "phone" , "roles" }
                }
            });
        });
    }

    private static void ConfigureLocalization(IServiceCollection services)
    {
        var supportedCultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("tr-TR"),
        };

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("en-US");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.RequestCultureProviders =
            [
                new AcceptLanguageHeaderRequestCultureProvider()
            ];
        });
    }
}