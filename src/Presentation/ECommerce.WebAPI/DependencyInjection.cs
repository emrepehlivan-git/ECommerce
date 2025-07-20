using System.Globalization;
using ECommerce.Application;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Logging;
using ECommerce.Application.Services;
using ECommerce.Infrastructure;
using ECommerce.Persistence;
using ECommerce.Persistence.Contexts;
using ECommerce.Persistence.Seeders;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.WebAPI.Authorization;
using ECommerce.WebAPI.Controllers.V1;
using ECommerce.WebAPI.Middlewares;
using ECommerce.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace ECommerce.WebAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<DataSeeder>();
        
        ConfigureLocalization(services);
        ConfigureSwagger(services, configuration);
        ConfigureApiVersioning(services);
       ConfigureAuthentication(services, configuration);
        ConfigureAuthorization(services);
        ConfigureRateLimiting(services);
        ConfigureCors(services);

        services.AddApplication()
            .AddInfrastructure(configuration)
            .AddPersistence(configuration);

        services.AddDependencies(typeof(DependencyInjection).Assembly);
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddHttpContextAccessor();
        services.AddProblemDetails();
        services.AddSignalR();

        return services;
    }

    private static void ConfigureCors(IServiceCollection services)
    {
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
                var publicAuthServerUrl = keycloakOptions["public-auth-server-url"] ?? keycloakOptions["auth-server-url"];
                var realm = keycloakOptions["realm"];
                options.OAuthClientId("swagger-client");
                options.OAuthUsePkce();
                options.OAuthScopeSeparator(" ");
                options.OAuth2RedirectUrl("http://localhost:4000/swagger/oauth2-redirect.html");
            });
        }

        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        
        if (!environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseRateLimiter();
        
        // Debug middleware BEFORE authentication
        app.Use(async (context, next) =>
        {
            Console.WriteLine($"BEFORE AUTH - Path: {context.Request.Path}");
            Console.WriteLine($"BEFORE AUTH - Auth header: {context.Request.Headers.Authorization.FirstOrDefault()}");
            await next();
            Console.WriteLine($"AFTER AUTH - User authenticated: {context.User.Identity?.IsAuthenticated}");
            Console.WriteLine($"AFTER AUTH - Claims count: {context.User.Claims.Count()}");
        });
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<NotificationHub>("/notificationHub");
        
        // Test endpoint to debug authentication
        app.MapGet("/test-auth", (HttpContext context) =>
        {
            Console.WriteLine($"Test endpoint - User authenticated: {context.User.Identity?.IsAuthenticated}");
            Console.WriteLine($"Test endpoint - Claims count: {context.User.Claims.Count()}");
            Console.WriteLine($"Test endpoint - Auth header: {context.Request.Headers.Authorization.FirstOrDefault()}");
            return Results.Ok(new { 
                IsAuthenticated = context.User.Identity?.IsAuthenticated,
                ClaimsCount = context.User.Claims.Count(),
                AuthHeader = context.Request.Headers.Authorization.FirstOrDefault()
            });
        });

        return app;
    }

    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var keycloakOptions = configuration.GetSection("Keycloak");
                var metadataUrl = keycloakOptions["metadata-url"]!;
                
                Console.WriteLine($"JWT Bearer Configuration:");
                Console.WriteLine($"Authority: {keycloakOptions["auth-server-url"]}");
                Console.WriteLine($"MetadataAddress: {metadataUrl}");
                Console.WriteLine($"Valid Issuers: {string.Join(", ", keycloakOptions.GetSection("valid-issuers").Get<string[]>() ?? Array.Empty<string>())}");
                Console.WriteLine($"Valid Audiences: {string.Join(", ", keycloakOptions.GetSection("valid-audiences").Get<string[]>() ?? Array.Empty<string>())}");

                options.Authority = keycloakOptions["auth-server-url"]!;
                options.MetadataAddress = metadataUrl;
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = false,
                    ValidIssuers = keycloakOptions.GetSection("valid-issuers").Get<string[]>()!,
                    ValidAudiences = keycloakOptions.GetSection("valid-audiences").Get<string[]>()!,
                };

                Console.WriteLine("JWT Bearer middleware configured with events");
                
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        Console.WriteLine("JWT OnMessageReceived triggered!");
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                        Console.WriteLine($"Request path: {context.Request.Path}");
                        Console.WriteLine($"Authorization header: {context.Request.Headers.Authorization}");
                        Console.WriteLine($"Exception type: {context.Exception.GetType().Name}");
                        Console.WriteLine($"Full exception: {context.Exception}");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($"JWT Challenge triggered for path: {context.Request.Path}");
                        Console.WriteLine($"Error: {context.Error}, Description: {context.ErrorDescription}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<IECommerceLogger<BaseApiV1Controller>>();
                        var claims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                        logger.LogInformation("Token claims: {Claims}", string.Join(", ", claims ?? new List<string>()));
                        
                        var syncService =
                            context.HttpContext.RequestServices.GetRequiredService<IUserSynchronizationService>();
                        await syncService.SyncUserAsync(context.Principal!);
                    }
                };
            });
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

    public static async Task ConfigurePermissions(this WebApplication app){
        using var scope = app.Services.CreateScope();
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

    private static void ConfigureRateLimiting(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("GlobalRateLimit", config =>
            {
                config.PermitLimit = 100;
                config.Window = TimeSpan.FromMinutes(1);
                config.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                config.QueueLimit = 10;
            });

            options.AddFixedWindowLimiter("AuthRateLimit", config =>
            {
                config.PermitLimit = 10;
                config.Window = TimeSpan.FromMinutes(1);
                config.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                config.QueueLimit = 5;
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken);
            };
        });
    }
}
