using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeedApp.Application.DTOs.Invitation;
using NeedApp.Application.Features.Invitations.Commands;
using NeedApp.Application.Features.Invitations.Queries;

namespace NeedApp.API.Controllers;

[ApiController]
[Route("api/invitations")]
[Authorize]
public class InvitationsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get all pending invitations for the current user.
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPendingInvitationsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Accept or decline an invitation.
    /// </summary>
    [HttpPut("{id:guid}/respond")]
    public async Task<IActionResult> Respond(
        Guid id,
        [FromBody] RespondInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RespondInvitationCommand(id, request.Accept),
            cancellationToken);
        return Ok(result);
    }
}
