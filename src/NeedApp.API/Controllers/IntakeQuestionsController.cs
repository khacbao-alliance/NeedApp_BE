using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeedApp.Application.DTOs.IntakeQuestion;
using NeedApp.Application.Features.IntakeQuestions.Commands;
using NeedApp.Application.Features.IntakeQuestions.Queries;

namespace NeedApp.API.Controllers;

[ApiController]
[Route("api/intake-question-sets")]
[Authorize(Roles = "Admin")]
public class IntakeQuestionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetIntakeQuestionSetsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetIntakeQuestionSetByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIntakeQuestionSetRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateIntakeQuestionSetCommand(
                request.Name,
                request.Description,
                request.IsDefault,
                request.Questions?.Select(q => new CreateIntakeQuestionRequest(q.Content, q.OrderIndex, q.IsRequired, q.Placeholder)).ToList()),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateIntakeQuestionSetRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateIntakeQuestionSetCommand(
                id,
                request.Name,
                request.Description,
                request.IsDefault,
                request.IsActive,
                request.Questions?.Select(q => new CreateIntakeQuestionRequest(q.Content, q.OrderIndex, q.IsRequired, q.Placeholder)).ToList()),
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ToggleIntakeQuestionSetActiveCommand(id), cancellationToken);
        return Ok(new { id = result.Id, isActive = result.IsActive });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteIntakeQuestionSetCommand(id), cancellationToken);
        return NoContent();
    }
}
