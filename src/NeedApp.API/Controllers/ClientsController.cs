using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeedApp.Application.DTOs.Client;
using NeedApp.Application.Features.Clients.Commands;

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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetClient(Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement GetClientQuery
        return Ok();
    }
}
