using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeedApp.Application.DTOs.EmailPreference;
using NeedApp.Application.Features.EmailPreferences;

namespace NeedApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/email-preferences")]
public class EmailPreferencesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmailPreferenceQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdateEmailPreferenceRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateEmailPreferenceCommand(
                body.OnAssignment,
                body.OnStatusChange,
                body.OnOverdue,
                body.OnNewRequest,
                body.DigestFrequency),
            cancellationToken);
        return Ok(result);
    }
}
