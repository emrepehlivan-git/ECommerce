using ECommerce.Application.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ECommerce.Application.Common.Logging;

namespace ECommerce.Application.Features.Configuration.Commands.UpdateEmailSettings;

public sealed class UpdateEmailSettingsCommandHandler(ILazyServiceProvider lazyServiceProvider) 
    : BaseHandler<UpdateEmailSettingsCommand, UpdateEmailSettingsResponse>(lazyServiceProvider)
{
    public override async Task<UpdateEmailSettingsResponse> Handle(UpdateEmailSettingsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var logger = LazyServiceProvider.LazyGetRequiredService<ILogger>();
            
            logger.LogInformation("Email ayarları güncelleniyor - SMTP Host: {SmtpHost}, Port: {SmtpPort}, User: {SmtpUser}", 
                request.SmtpHost, request.SmtpPort, request.SmtpUser);

            // Environment variables ile configuration güncelle
            Environment.SetEnvironmentVariable("EmailSettings__SmtpHost", request.SmtpHost);
            Environment.SetEnvironmentVariable("EmailSettings__SmtpPort", request.SmtpPort.ToString());
            Environment.SetEnvironmentVariable("EmailSettings__SmtpUser", request.SmtpUser);
            Environment.SetEnvironmentVariable("EmailSettings__SmtpPassword", request.SmtpPassword);
            Environment.SetEnvironmentVariable("EmailSettings__FromEmail", request.FromEmail);
            Environment.SetEnvironmentVariable("EmailSettings__FromName", request.FromName);

            logger.LogDebug("Environment variables ayarlandı");

            // Configuration reload
            var configuration = LazyServiceProvider.LazyGetRequiredService<IConfiguration>();
            if (configuration is IConfigurationRoot configRoot)
            {
                configRoot.Reload();
                logger.LogDebug("Configuration reload edildi");
            }

            // Test için SMTP ayarlarını logla
            var currentSmtpHost = configuration["EmailSettings:SmtpHost"];
            var currentSmtpUser = configuration["EmailSettings:SmtpUser"];
            
            logger.LogInformation("Configuration güncellendi - Current SMTP Host: {CurrentSmtpHost}, User: {CurrentSmtpUser}", 
                currentSmtpHost, currentSmtpUser);

            logger.LogInformation("Email ayarları environment variables ile güncellendi");
            
            return new UpdateEmailSettingsResponse(true, 
                "Email ayarları başarıyla güncellendi. Değişikliklerin tam olarak aktif olması için uygulamayı yeniden başlatmanızı öneririz.");
        }
        catch (Exception ex)
        {
            var logger = LazyServiceProvider.LazyGetRequiredService<ILogger>();
            logger.LogError(ex, "Email ayarları güncellenirken hata oluştu: {Message}", ex.Message);
            return new UpdateEmailSettingsResponse(false, $"Hata: {ex.Message}");
        }
    }
} 