using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeedApp.Application.Features.Dashboard;

namespace NeedApp.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Admin,Staff")]
public class DashboardController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get aggregated dashboard statistics (status breakdown, daily trend, SLA metrics, staff performance).
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetDashboardStatsQuery(days), cancellationToken);
        return Ok(result);
    }
}
