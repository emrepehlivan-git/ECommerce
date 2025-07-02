using ECommerce.Application.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ECommerce.Application.Common.Logging;

namespace ECommerce.Application.Features.Configuration.V1.Commands.UpdateEmailSettings;

public sealed class UpdateEmailSettingsCommandHandler(ILazyServiceProvider lazyServiceProvider) 
    : BaseHandler<UpdateEmailSettingsCommand, UpdateEmailSettingsResponse>(lazyServiceProvider)
{
    public override Task<UpdateEmailSettingsResponse> Handle(UpdateEmailSettingsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var logger = LazyServiceProvider.LazyGetRequiredService<IECommerLogger<UpdateEmailSettingsCommandHandler>>();
            
            logger.LogInformation("Email ayarları güncelleniyor - SMTP Host: {SmtpHost}, Port: {SmtpPort}, User: {SmtpUser}", 
                request.SmtpHost, request.SmtpPort, request.SmtpUser);

            Environment.SetEnvironmentVariable("EmailSettings__SmtpHost", request.SmtpHost);
            Environment.SetEnvironmentVariable("EmailSettings__SmtpPort", request.SmtpPort.ToString());
            Environment.SetEnvironmentVariable("EmailSettings__SmtpUser", request.SmtpUser);
            Environment.SetEnvironmentVariable("EmailSettings__SmtpPassword", request.SmtpPassword);
            Environment.SetEnvironmentVariable("EmailSettings__FromEmail", request.FromEmail);
            Environment.SetEnvironmentVariable("EmailSettings__FromName", request.FromName);

            var configuration = LazyServiceProvider.LazyGetRequiredService<IConfiguration>();
            if (configuration is IConfigurationRoot configRoot)
            {
                configRoot.Reload();
            }

            return Task.FromResult(new UpdateEmailSettingsResponse(true, 
                "Email ayarları başarıyla güncellendi. Değişikliklerin tam olarak aktif olması için uygulamayı yeniden başlatmanızı öneririz."));
        }
        catch (Exception ex)
        {
            var logger = LazyServiceProvider.LazyGetRequiredService<IECommerLogger<UpdateEmailSettingsCommandHandler>>();
            logger.LogError(ex, "Email ayarları güncellenirken hata oluştu: {Message}", ex.Message);
            return Task.FromResult(new UpdateEmailSettingsResponse(false, $"Hata: {ex.Message}"));
        }
    }
} 
