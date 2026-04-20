using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeedApp.Application.DTOs.SlaConfig;
using NeedApp.Application.Features.SlaConfig;

namespace NeedApp.API.Controllers;

[ApiController]
[Route("api/sla-config")]
public class SlaConfigController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get all SLA deadline configurations (any authenticated user can read).
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSlaConfigsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update SLA deadline configurations (Admin only, upsert).
    /// </summary>
    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        [FromBody] UpdateSlaConfigsRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateSlaConfigsCommand(body.Configs),
            cancellationToken);
        return Ok(result);
    }
}
