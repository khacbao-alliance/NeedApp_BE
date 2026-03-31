using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeedApp.Application.Features.Users.Commands;
using NeedApp.Application.Features.Users.Queries;
using NeedApp.Application.Interfaces;

namespace NeedApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var result = await mediator.Send(new GetUserByIdQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var commandWithId = command with { Id = userId };
        var result = await mediator.Send(commandWithId, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }
}
