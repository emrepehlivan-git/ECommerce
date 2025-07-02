using ECommerce.Application.Services;
using System;
using ECommerce.Persistence.Contexts;
using ECommerce.Persistence;
using static OpenIddict.Abstractions.OpenIddictConstants;
using ECommerce.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using ECommerce.AuthServer;
using ECommerce.AuthServer.Services;
using ECommerce.Infrastructure;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.WithOrigins(
            "http://localhost:4000",
            "https://localhost:4001",
            "http://localhost:3000",
            "https://localhost:3000",
            "http://ecommerce.webapi:8080",
            "https://ecommerce.webapi:8081",
            "http://ecommerce.client:3000",
            "https://ecommerce.client:3000"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = "/Account/Login");

builder.Services.AddQuartz(options =>
{
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<ApplicationDbContext>();
        
        options.UseQuartz();
    })
    .AddServer(options =>
    {
        // Her zaman localhost issuer kullan - port mapping sayesinde çalışacak
        var issuer = "https://localhost:5002/";
        
        Console.WriteLine($"[AUTH-SERVER] Selected Issuer: {issuer}");
        
        options.SetIssuer(issuer);
        
        options.SetAuthorizationEndpointUris("/connect/authorize")
               .SetTokenEndpointUris("/connect/token")
               .SetUserInfoEndpointUris("/connect/userinfo")
               .SetIntrospectionEndpointUris("/connect/introspect")
               .SetEndSessionEndpointUris("/connect/logout");

        options.RegisterScopes(
            Scopes.Address,
            Scopes.Email,
            Scopes.Phone,
            Scopes.Profile,
            Scopes.Roles,
            Scopes.OfflineAccess,
            "api");

        options.AllowAuthorizationCodeFlow()
                .AllowClientCredentialsFlow()
                .AllowRefreshTokenFlow()
                .RequireProofKeyForCodeExchange();

        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        options.DisableAccessTokenEncryption();

        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableStatusCodePagesIntegration()
               .EnableUserInfoEndpointPassthrough()
               .EnableEndSessionEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
        options.UseSystemNetHttp();
    });

builder.Services.AddHostedService<Worker>();


var app = builder.Build();



if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowAllOrigins");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();