using System.Globalization;
using ECommerce.Application;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Services;
using ECommerce.Infrastructure;
using ECommerce.Persistence;
using ECommerce.Persistence.Contexts;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.WebAPI.Authorization;
using ECommerce.WebAPI.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace ECommerce.WebAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureLocalization(services);

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                builder =>
                {
                    builder.WithOrigins(
                            "http://localhost:3000",
                            "https://localhost:3000",
                            "http://localhost:8088",
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

        ConfigureApiVersioning(services);

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddHttpContextAccessor();
        services.AddProblemDetails();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var keycloakOptions = configuration.GetSection("Keycloak");
                var authority = $"{keycloakOptions["auth-server-url"]!}realms/{keycloakOptions["realm"]!}";

                options.Authority = authority;
                options.RequireHttpsMetadata = keycloakOptions.GetValue<bool?>("require-https-metadata") ?? false;

                var publicAuthServerUrl = keycloakOptions["public-auth-server-url"] ?? authority;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidIssuers = [authority, publicAuthServerUrl.TrimEnd('/') + "/realms/" + keycloakOptions["realm"]],
                    // API'nin kendisi (`ecommerce-api`), frontend (`nextjs-client`) ve swagger gibi farklı client'lardan gelen token'ları kabul et.
                    ValidAudiences = [keycloakOptions["client-id"], "nextjs-client", "swagger-client", "account"],
                    ValidateLifetime = true
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var syncService =
                            context.HttpContext.RequestServices.GetRequiredService<IUserSynchronizationService>();
                        await syncService.SyncUserAsync(context.Principal!);
                    }
                };
            });

        ConfigureAuthorization(services);

        return services;
    }

    public static WebApplication UsePresentation(this WebApplication app, IWebHostEnvironment environment)
    {
        app.UseMiddleware<RequestTimingMiddleware>();

        app.UseRequestLocalization();

        app.UseCors("AllowAllOrigins");

        if (environment.IsDevelopment())
        {
            app.UseSwagger(c => { c.RouteTemplate = "swagger/{documentName}/swagger.json"; });
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce API v1");

                var keycloakOptions = app.Configuration.GetSection("Keycloak");
                options.OAuthClientId(keycloakOptions["client-id"]);
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

    private static void ConfigureAuthorization(IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();

            var permissionTypes = typeof(PermissionConstants).GetNestedTypes();
            foreach (var type in permissionTypes)
            {
                var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                foreach (var field in fields)
                    if (field.FieldType == typeof(string))
                    {
                        var permissionValue = (string)field.GetValue(null)!;
                        options.AddPolicy(permissionValue, policy =>
                            policy.AddRequirements(new PermissionRequirement(permissionValue)));
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

    private static void ConfigureSwagger(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ECommerce API",
                Version = "v1",
                Description = "ECommerce API with Keycloak Authentication and Versioning"
            });

            var keycloakOptions = configuration.GetSection("Keycloak");
            var authServerUrl = keycloakOptions["public-auth-server-url"] ?? keycloakOptions["auth-server-url"];
            var realm = keycloakOptions["realm"];
            var authorizationUrl = $"{authServerUrl}realms/{realm}/protocol/openid-connect/auth";
            var tokenUrl = $"{authServerUrl}realms/{realm}/protocol/openid-connect/token";

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(authorizationUrl),
                        TokenUrl = new Uri(tokenUrl),
                        Scopes = new Dictionary<string, string>
                        {
                            { "api", "API Access" },
                            { "openid", "OpenID" }
                        }
                    }
                }
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    new[] { "api", "openid" }
                }
            });
        });
    }

    private static void ConfigureLocalization(IServiceCollection services)
    {
        var supportedCultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("tr-TR")
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

    private static void ConfigureApiVersioning(IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new QueryStringApiVersionReader("version"),
                new HeaderApiVersionReader("X-Version")
            );
        });

        services.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });
    }
}