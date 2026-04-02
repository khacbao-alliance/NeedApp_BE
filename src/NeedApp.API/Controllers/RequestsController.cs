using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeedApp.Application.DTOs.Request;
using NeedApp.Application.Features.Requests.Commands;
using NeedApp.Application.Features.Requests.Queries;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;

namespace NeedApp.API.Controllers;

[ApiController]
[Route("api/requests")]
[Authorize]
public class RequestsController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] RequestStatus? status = null,
        [FromQuery] RequestPriority? priority = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetRequestsQuery(page, pageSize, search, status, priority),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateRequestCommand(request.Title, request.Description, request.Priority),
            cancellationToken);
        return CreatedAtAction(nameof(GetRequest), new { id = result.RequestId }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRequest(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRequestByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateRequestStatusCommand(id, request.Status),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Admin assigns a specific staff to a request.
    /// </summary>
    [HttpPatch("{id:guid}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRequest(Guid id, [FromBody] AssignRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new AssignRequestCommand(id, request.StaffUserId),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Staff self-assigns to a request. Only works if request is not already assigned.
    /// </summary>
    [HttpPatch("{id:guid}/self-assign")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> SelfAssign(Guid id, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var result = await mediator.Send(
            new AssignRequestCommand(id, userId),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Admin unassigns staff from a request.
    /// </summary>
    [HttpPatch("{id:guid}/unassign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnassignRequest(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UnassignRequestCommand(id),
            cancellationToken);
        return Ok(result);
    }
}
