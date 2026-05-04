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

    /// <summary>
    /// Search messages within a request by keyword (Vietnamese diacritics-insensitive).
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchMessages(
        Guid requestId,
        [FromQuery] string q = "",
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new SearchMessagesQuery(requestId, q, limit),
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

    /// <summary>
    /// Get a structured summary of the conversation with AI-powered insights.
    /// Staff/Admin can view all. Client can only view their own requests.
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetConversationSummary(
        Guid requestId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetConversationSummaryQuery(requestId),
            cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> EditMessage(
        Guid requestId,
        Guid id,
        [FromBody] EditMessageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new EditMessageCommand(requestId, id, request.Content),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("{messageId:guid}/pin")]
    public async Task<IActionResult> PinMessage(
        Guid requestId,
        Guid messageId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new PinMessageCommand(requestId, messageId),
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

    /// <summary>
    /// Get the edit history of a message (previous versions, newest first).
    /// Accessible by all participants of the request.
    /// </summary>
    [HttpGet("{messageId:guid}/history")]
    public async Task<IActionResult> GetMessageHistory(
        Guid requestId,
        Guid messageId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetMessageHistoryQuery(requestId, messageId),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Toggle an emoji reaction on a message (add/remove).
    /// </summary>
    [HttpPost("{messageId:guid}/reactions")]
    public async Task<IActionResult> ToggleReaction(
        Guid requestId,
        Guid messageId,
        [FromBody] ToggleReactionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ToggleReactionCommand(messageId, request.Emoji),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Mark all messages in a request as read for the current user.
    /// </summary>
    [HttpPost("read")]
    public async Task<IActionResult> MarkRead(
        Guid requestId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkReadCommand(requestId), cancellationToken);
        return NoContent();
    }
    /// <summary>
    /// Answer an individual Missing Info question. Accessible by Client and Staff.
    /// </summary>
    [HttpPost("{messageId:guid}/answer-missing-info")]
    public async Task<IActionResult> AnswerMissingInfo(
        Guid requestId,
        Guid messageId,
        [FromBody] AnswerMissingInfoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new AnswerMissingInfoCommand(requestId, messageId, request.QuestionId, request.Answer),
            cancellationToken);
        return Ok(result);
    }
}

public record ToggleReactionRequest(string Emoji);
public record EditMessageRequest(string Content);
public record AnswerMissingInfoRequest(string QuestionId, string Answer);
