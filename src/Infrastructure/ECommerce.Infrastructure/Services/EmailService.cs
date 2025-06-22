using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ECommerce.Infrastructure.Services;

public sealed class EmailService : IEmailService, IScopedDependency
{
    private readonly IConfiguration _configuration;
    private readonly Application.Common.Logging.IECommerLogger<EmailService>? _logger;

    public EmailService(IConfiguration configuration, Application.Common.Logging.IECommerLogger<EmailService>? logger = null)
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
            var fromEmail = _configuration["EmailSettings:FromEmail"] ?? throw new InvalidOperationException("E-posta gönderici adresi konfigürasyonu bulunamadı.");
            var fromName = _configuration["EmailSettings:FromName"];

            using var client = CreateSmtpClient();

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName ?? "ECommerce"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            
            await client.SendMailAsync(mailMessage);
        }
        catch (SmtpException smtpEx)
        {
            _logger?.LogError(smtpEx, "SMTP hatası - E-posta gönderilirken hata oluştu: {Email} - SMTP Durum: {StatusCode}, Mesaj: {Message}", 
                toEmail, smtpEx.StatusCode, smtpEx.Message);
            
            if (smtpEx.StatusCode == SmtpStatusCode.MailboxBusy || 
                smtpEx.StatusCode == SmtpStatusCode.ClientNotPermitted ||
                smtpEx.StatusCode == SmtpStatusCode.InsufficientStorage)
            {
                throw new InvalidOperationException("SMTP kimlik doğrulama hatası. Lütfen e-posta ayarlarını kontrol edin.");
            }
            
            throw new InvalidOperationException($"E-posta gönderimi başarısız: {smtpEx.Message}", smtpEx);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "E-posta gönderilirken beklenmeyen hata oluştu: {Email}", toEmail);
            throw new InvalidOperationException($"E-posta gönderimi başarısız: {ex.Message}", ex);
        }
    }

    private SmtpClient CreateSmtpClient()
    {
        var smtpSettings = _configuration.GetSection("EmailSettings");
        var smtpHost = smtpSettings["SmtpHost"] ?? throw new InvalidOperationException("SMTP Host konfigürasyonu bulunamadı.");
        var smtpPortValue = smtpSettings["SmtpPort"] ?? throw new InvalidOperationException("SMTP Port konfigürasyonu bulunamadı.");
        var smtpUser = smtpSettings["SmtpUser"] ?? throw new InvalidOperationException("SMTP kullanıcı adı konfigürasyonu bulunamadı.");
        var smtpPassword = smtpSettings["SmtpPassword"] ?? throw new InvalidOperationException("SMTP şifre konfigürasyonu bulunamadı.");

        var smtpPort = int.Parse(smtpPortValue);
        
        _logger?.LogDebug("SMTP Client oluşturuluyor - Host: {SmtpHost}, Port: {SmtpPort}, User: {SmtpUser}", 
            smtpHost, smtpPort, smtpUser);

        // Ethereal Email için özel ayarlar
        if (smtpHost.Contains("ethereal.email", StringComparison.OrdinalIgnoreCase))
        {
            ConfigureEtherealEmail();
        }
        
        var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(smtpUser, smtpPassword),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 60000 // 60 saniye timeout - Ethereal için daha uzun
        };

        _logger?.LogDebug("SMTP Client yapılandırması tamamlandı - SSL: {EnableSsl}, Timeout: {Timeout}ms", 
            client.EnableSsl, client.Timeout);

        return client;
    }

    private void ConfigureEtherealEmail()
    {
        _logger?.LogDebug("Ethereal Email için özel konfigürasyon uygulanıyor");
        
        // TLS/SSL protokol ayarları
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
        
        // SSL Sertifika doğrulama callback'i
        ServicePointManager.ServerCertificateValidationCallback = EtherealCertificateValidation;
        
        // Diğer ayarlar
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.CheckCertificateRevocationList = false;
        ServicePointManager.DefaultConnectionLimit = 10;
        ServicePointManager.MaxServicePointIdleTime = 30000;
        
        _logger?.LogDebug("Ethereal Email SSL/TLS konfigürasyonu tamamlandı");
    }

    private bool EtherealCertificateValidation(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        _logger?.LogDebug("SSL Sertifika doğrulaması - PolicyErrors: {PolicyErrors}, Certificate Subject: {Subject}", 
            sslPolicyErrors, certificate?.Subject ?? "N/A");
            
        // Development ortamında Ethereal için tüm sertifikaları kabul et
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            _logger?.LogDebug("SSL sertifika doğrulama başarılı");
            return true;
        }

        // Ethereal Email sertifikası için özel kontroller
        if (certificate?.Subject?.Contains("ethereal.email") == true)
        {
            _logger?.LogInformation("Ethereal Email sertifikası kabul edildi - PolicyErrors: {PolicyErrors}", sslPolicyErrors);
            return true; // Development için Ethereal sertifikalarını kabul et
        }

        _logger?.LogWarning("SSL sertifika doğrulama başarısız - PolicyErrors: {PolicyErrors}", sslPolicyErrors);
        return false;
    }
} 
