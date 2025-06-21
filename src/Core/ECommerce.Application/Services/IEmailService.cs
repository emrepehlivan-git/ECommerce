namespace ECommerce.Application.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetLink);
    Task SendEmailConfirmationAsync(string email, string confirmationLink);
} 