using OpenIddict.Abstractions;
using Quartz;
using System.Diagnostics;

namespace ECommerce.AuthServer.Jobs;

[DisallowConcurrentExecution]
public sealed class OpenIddictMaintenanceJob : IJob
{
    private readonly ILogger<OpenIddictMaintenanceJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    public OpenIddictMaintenanceJob(ILogger<OpenIddictMaintenanceJob> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var authorizationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
        var tokenManager = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();

        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("OpenIddict bakım job'ı başlatılıyor...");

            var totalTokens = 0;
            var expiredTokens = 0;
            
            await foreach (var token in tokenManager.ListAsync())
            {
                totalTokens++;
                var expirationDate = await tokenManager.GetExpirationDateAsync(token);
                if (expirationDate.HasValue && expirationDate.Value < DateTime.UtcNow)
                {
                    expiredTokens++;
                }
            }
    
            var totalAuthorizations = 0;
            var expiredAuthorizations = 0;
            
            await foreach (var authorization in authorizationManager.ListAsync())
            {
                totalAuthorizations++;
                var creationDate = await authorizationManager.GetCreationDateAsync(authorization);
                if (creationDate.HasValue && creationDate.Value.AddDays(7) < DateTime.UtcNow)
                {
                    expiredAuthorizations++;
                }
            }

            _logger.LogInformation(
                "OpenIddict bakım job'ı tamamlandı. " +
                "Toplam Token: {TotalTokens}, Süresi Dolmuş Token: {ExpiredTokens}, " +
                "Toplam Authorization: {TotalAuthorizations}, Süresi Dolmuş Authorization: {ExpiredAuthorizations}, " +
                "Süre: {Duration}ms",
                totalTokens, expiredTokens, totalAuthorizations, expiredAuthorizations, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenIddict bakım job'ı sırasında hata oluştu");
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }
} 