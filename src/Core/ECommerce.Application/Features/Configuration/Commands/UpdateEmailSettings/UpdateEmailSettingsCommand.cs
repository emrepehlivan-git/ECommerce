using MediatR;

namespace ECommerce.Application.Features.Configuration.Commands.UpdateEmailSettings;

public sealed record UpdateEmailSettingsCommand(
    string SmtpHost,
    int SmtpPort,
    string SmtpUser,
    string SmtpPassword,
    string FromEmail,
    string FromName
) : IRequest<UpdateEmailSettingsResponse>;

public sealed record UpdateEmailSettingsResponse(
    bool IsSuccess,
    string Message
); 