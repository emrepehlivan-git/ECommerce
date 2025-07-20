using ECommerce.Application.Features.Dashboard.V1.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebAPI.Controllers.V1;

[Authorize(Roles = "Admin")]
public sealed class DashboardController : BaseApiV1Controller
{
    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var query = new GetDashboardStatsQuery();
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("recent-activity")]
    public async Task<IActionResult> GetRecentActivity([FromQuery] int count = 10)
    {
        var query = new GetRecentActivityQuery(count);
        var result = await Mediator.Send(query);
        return Ok(result);
    }
}