using ECommerce.Application.Features.Configuration.Commands.UpdateEmailSettings;
using ECommerce.Application.Features.Configuration.Queries.GetEmailSettings;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebAPI.Controllers;

public sealed class ConfigurationController : BaseApiController
{
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
}
