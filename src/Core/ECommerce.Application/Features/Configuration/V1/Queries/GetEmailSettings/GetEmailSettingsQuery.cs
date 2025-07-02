using MediatR;

namespace ECommerce.Application.Features.Configuration.V1.Queries.GetEmailSettings;

public sealed record GetEmailSettingsQuery() : IRequest<GetEmailSettingsResponse>;

public sealed record GetEmailSettingsResponse(
    string SmtpHost,
    int SmtpPort,
    string SmtpUser,
    string FromEmail,
    string FromName
); 