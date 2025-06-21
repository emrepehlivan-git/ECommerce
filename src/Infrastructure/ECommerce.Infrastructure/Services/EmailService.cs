using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace ECommerce.Infrastructure.Services;

public sealed class EmailService : IEmailService, IScopedDependency
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetLink)
    {
        var subject = "Şifre Sıfırlama Talebi";
        var body = $@"
            <h2>Şifre Sıfırlama</h2>
            <p>Merhaba,</p>
            <p>Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:</p>
            <p><a href='{resetLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Şifremi Sıfırla</a></p>
            <p>Bu bağlantı 2 saat boyunca geçerlidir.</p>
            <p>Eğer bu talebi siz yapmadıysanız, bu e-postayı görmezden gelebilirsiniz.</p>
            <br>
            <p>Saygılarımızla,<br>ECommerce Ekibi</p>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
    {
        var subject = "E-posta Doğrulama";
        var body = $@"
            <h2>E-posta Doğrulama</h2>
            <p>Merhaba,</p>
            <p>Hesabınızı aktifleştirmek için aşağıdaki bağlantıya tıklayın:</p>
            <p><a href='{confirmationLink}' style='background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>E-postamı Doğrula</a></p>
            <br>
            <p>Saygılarımızla,<br>ECommerce Ekibi</p>";

        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var fromName = _configuration["EmailSettings:FromName"];

            using var client = CreateSmtpClient();

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail!, fromName ?? "ECommerce"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("E-posta başarıyla gönderildi: {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E-posta gönderilirken hata oluştu: {Email}", toEmail);
            throw;
        }
    }

    private SmtpClient CreateSmtpClient()
    {
        var smtpSettings = _configuration.GetSection("EmailSettings");
        var smtpHost = smtpSettings["SmtpHost"];
        var smtpPort = int.Parse(smtpSettings["SmtpPort"] ?? "587");
        var smtpUser = smtpSettings["SmtpUser"];
        var smtpPassword = smtpSettings["SmtpPassword"];

        return new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(smtpUser, smtpPassword)
        };
    }
} 