using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeedApp.Application.DTOs.Client;
using NeedApp.Application.Features.Clients.Commands;
using NeedApp.Application.Features.Clients.Queries;

namespace NeedApp.API.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateClientCommand(request.Name, request.Description, request.ContactEmail, request.ContactPhone),
            cancellationToken);
        return CreatedAtAction(nameof(GetClient), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> UpdateClient(Guid id, [FromBody] UpdateClientRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateClientCommand(id, request.Name, request.Description, request.ContactEmail, request.ContactPhone),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetClient(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement GetClientQuery
        return Ok();
    }

    /// <summary>
    /// Get all members of a client.
    /// Client users can only view their own client. Staff/Admin can view any.
    /// </summary>
    [HttpGet("{clientId:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid clientId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetClientMembersQuery(clientId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Invite a user to the client by email. Only the Client Owner can do this.
    /// The user must already have a NeedApp account.
    /// Creates a pending invitation that the user must accept or decline.
    /// </summary>
    [HttpPost("{clientId:guid}/members")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> InviteMember(
        Guid clientId,
        [FromBody] AddMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new InviteClientMemberCommand(clientId, request.Email, request.Role),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Remove a member from the client. Only the Client Owner can do this.
    /// The Owner cannot remove themselves.
    /// </summary>
    [HttpDelete("{clientId:guid}/members/{userId:guid}")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> RemoveMember(
        Guid clientId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new RemoveClientMemberCommand(clientId, userId), cancellationToken);
        return NoContent();
    }
}
