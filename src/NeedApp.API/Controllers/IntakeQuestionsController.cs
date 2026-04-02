using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeedApp.Application.DTOs.IntakeQuestion;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.API.Controllers;

[ApiController]
[Route("api/intake-question-sets")]
[Authorize(Roles = "Admin")]
public class IntakeQuestionsController(
    IIntakeQuestionSetRepository repository,
    IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var sets = await repository.GetAllAsync(cancellationToken);
        var result = sets.Select(s => new IntakeQuestionSetDto(
            s.Id, s.Name, s.Description, s.IsActive, s.IsDefault,
            [], s.CreatedAt
        ));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var set = await repository.GetWithQuestionsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(IntakeQuestionSet), id);

        var result = new IntakeQuestionSetDto(
            set.Id, set.Name, set.Description, set.IsActive, set.IsDefault,
            set.Questions.OrderBy(q => q.OrderIndex).Select(q => new IntakeQuestionDto(
                q.Id, q.Content, q.OrderIndex, q.IsRequired, q.Placeholder
            )).ToList(),
            set.CreatedAt
        );
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIntakeQuestionSetRequest request, CancellationToken cancellationToken)
    {
        // If marking as default, un-default existing ones
        if (request.IsDefault)
        {
            var existing = await repository.GetDefaultAsync(cancellationToken);
            if (existing != null)
            {
                existing.IsDefault = false;
                repository.Update(existing);
            }
        }

        var set = new IntakeQuestionSet
        {
            Name = request.Name,
            Description = request.Description,
            IsDefault = request.IsDefault
        };

        if (request.Questions?.Any() == true)
        {
            foreach (var q in request.Questions)
            {
                set.Questions.Add(new IntakeQuestion
                {
                    Content = q.Content,
                    OrderIndex = q.OrderIndex,
                    IsRequired = q.IsRequired,
                    Placeholder = q.Placeholder
                });
            }
        }

        await repository.AddAsync(set, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = set.Id }, new IntakeQuestionSetDto(
            set.Id, set.Name, set.Description, set.IsActive, set.IsDefault,
            set.Questions.Select(q => new IntakeQuestionDto(
                q.Id, q.Content, q.OrderIndex, q.IsRequired, q.Placeholder
            )).ToList(),
            set.CreatedAt
        ));
    }
}
