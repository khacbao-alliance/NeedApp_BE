using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeedApp.Application.DTOs.User;
using NeedApp.Application.Features.Users.Commands;
using NeedApp.Application.Features.Users.Queries;
using NeedApp.Domain.Enums;

namespace NeedApp.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] UserRole? role = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetUsersQuery(page, pageSize, search, role),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateUserCommand(request.Email, request.Password, request.Name, request.Role),
            cancellationToken);
        return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateUserCommand(id, request.Name, request.Role),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }
}
