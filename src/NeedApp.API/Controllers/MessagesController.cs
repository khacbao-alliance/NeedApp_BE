using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeedApp.Application.DTOs.Message;
using NeedApp.Application.Features.Messages.Commands;
using NeedApp.Application.Features.Messages.Queries;

namespace NeedApp.API.Controllers;

[ApiController]
[Route("api/requests/{requestId:guid}/messages")]
[Authorize]
public class MessagesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMessages(
        Guid requestId,
        [FromQuery] string? cursor = null,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetMessagesQuery(requestId, cursor, limit),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(
        Guid requestId,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new SendMessageCommand(requestId, request.Content, request.Type, request.ReplyToId),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("missing-info")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> SendMissingInfo(
        Guid requestId,
        [FromBody] SendMissingInfoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new SendMissingInfoCommand(requestId, request.Content, request.Questions),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMessage(
        Guid requestId,
        Guid id,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteMessageCommand(requestId, id), cancellationToken);
        return NoContent();
    }
}
