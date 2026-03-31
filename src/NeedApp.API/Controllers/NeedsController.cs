using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeedApp.Application.Features.Needs.Commands;
using NeedApp.Application.Features.Needs.Queries;
using NeedApp.Application.Interfaces;

namespace NeedApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NeedsController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetNeedsQuery(page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNeedByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNeedCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var commandWithUser = command with { UserId = userId };
        var result = await mediator.Send(commandWithUser, cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data) : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNeedCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var commandWithId = command with { Id = id, RequestingUserId = userId };
        var result = await mediator.Send(commandWithId, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var result = await mediator.Send(new DeleteNeedCommand(id, userId), cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
