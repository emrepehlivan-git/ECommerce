using ECommerce.Application.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Application.Features.Configuration.Queries.GetEmailSettings;

public sealed class GetEmailSettingsQueryHandler(
    ILazyServiceProvider lazyServiceProvider) 
    : BaseHandler<GetEmailSettingsQuery, GetEmailSettingsResponse>(lazyServiceProvider)
{
    public override  Task<GetEmailSettingsResponse> Handle(GetEmailSettingsQuery request, CancellationToken cancellationToken)
    {
        var configuration = LazyServiceProvider.LazyGetRequiredService<IConfiguration>();
        
        var emailSettings = configuration.GetSection("EmailSettings");
        
        return Task.FromResult(new GetEmailSettingsResponse(
            SmtpHost: emailSettings["SmtpHost"] ?? string.Empty,
            SmtpPort: int.TryParse(emailSettings["SmtpPort"], out var port) ? port : 587,
            SmtpUser: emailSettings["SmtpUser"] ?? string.Empty,
            FromEmail: emailSettings["FromEmail"] ?? string.Empty,
            FromName: emailSettings["FromName"] ?? string.Empty
        ));
    }
} 