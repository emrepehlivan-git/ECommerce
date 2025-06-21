using ECommerce.Application.Features.Configuration.Commands.UpdateEmailSettings;
using ECommerce.Application.Features.Configuration.Queries.GetEmailSettings;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebAPI.Controllers;

public sealed class ConfigurationController : BaseApiController
{
    private readonly IEmailService _emailService;
    private readonly Application.Common.Logging.ILogger _logger;

    public ConfigurationController(IEmailService emailService, Application.Common.Logging.ILogger logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPut("email-settings")]
    public async Task<IActionResult> UpdateEmailSettings([FromBody] UpdateEmailSettingsCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("email-settings")]
    public async Task<IActionResult> GetEmailSettings()
    {
        var result = await Mediator.Send(new GetEmailSettingsQuery());
        return Ok(result);
    }

    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest request)
    {
        try
        {
            _logger.LogInformation("Test e-postası gönderiliyor - Alıcı: {ToEmail}", request.ToEmail);
            await _emailService.SendPasswordResetEmailAsync(request.ToEmail, "https://localhost:5002/test");
            _logger.LogInformation("Test e-postası başarıyla gönderildi - Alıcı: {ToEmail}", request.ToEmail);
            return Ok(new { Success = true, Message = "Test e-postası başarıyla gönderildi!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test e-postası gönderilirken hata oluştu - Alıcı: {ToEmail}", request.ToEmail);
            return BadRequest(new { Success = false, Message = $"E-posta gönderimi başarısız: {ex.Message}" });
        }
    }

    [HttpPost("test-logger")]
    public IActionResult TestLogger()
    {
        _logger.LogInformation("Test log mesajı - Information level");
        _logger.LogWarning("Test log mesajı - Warning level");
        _logger.LogError("Test log mesajı - Error level");
        _logger.LogDebug("Test log mesajı - Debug level");
        _logger.LogCritical("Test log mesajı - Critical level");
        
        try
        {
            throw new Exception("Test exception");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test exception log mesajı");
        }
        
        return Ok(new { Success = true, Message = "Logger test mesajları gönderildi. Seq'i kontrol edin!" });
    }
}

public sealed record TestEmailRequest(string ToEmail);   