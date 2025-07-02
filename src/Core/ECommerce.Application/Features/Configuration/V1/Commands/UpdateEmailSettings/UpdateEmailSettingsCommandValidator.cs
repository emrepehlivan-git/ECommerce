using FluentValidation;

namespace ECommerce.Application.Features.Configuration.V1.Commands.UpdateEmailSettings;

public sealed class UpdateEmailSettingsCommandValidator : AbstractValidator<UpdateEmailSettingsCommand>
{
    public UpdateEmailSettingsCommandValidator()
    {
        RuleFor(x => x.SmtpHost)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.SmtpPort)
            .GreaterThan(0)
            .LessThanOrEqualTo(65535);

        RuleFor(x => x.SmtpUser)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.SmtpPassword)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.FromEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.FromName)
            .NotEmpty()
            .MaximumLength(100);
    }
} 