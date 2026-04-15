using MediatR;
using NeedApp.Application.DTOs.IntakeQuestion;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.IntakeQuestions.Commands;

// ── Create ────────────────────────────────────────────

public record CreateIntakeQuestionSetCommand(
    string Name,
    string? Description,
    bool IsDefault,
    List<CreateIntakeQuestionRequest>? Questions) : IRequest<IntakeQuestionSetDto>;

public class CreateIntakeQuestionSetCommandHandler(
    IIntakeQuestionSetRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateIntakeQuestionSetCommand, IntakeQuestionSetDto>
{
    public async Task<IntakeQuestionSetDto> Handle(
        CreateIntakeQuestionSetCommand request, CancellationToken cancellationToken)
    {
        if (request.IsDefault)
        {
            var existing = await repository.GetDefaultAsync(cancellationToken);
            if (existing != null)
            {
                var trackedExisting = await repository.GetByIdAsync(existing.Id, cancellationToken);
                if (trackedExisting != null) trackedExisting.IsDefault = false;
            }
        }

        var set = new IntakeQuestionSet
        {
            Name = request.Name,
            Description = request.Description,
            IsDefault = request.IsDefault,
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
                    Placeholder = q.Placeholder,
                });
            }
        }

        await repository.AddAsync(set, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new IntakeQuestionSetDto(
            set.Id, set.Name, set.Description, set.IsActive, set.IsDefault,
            set.Questions.Select(q => new IntakeQuestionDto(
                q.Id, q.Content, q.OrderIndex, q.IsRequired, q.Placeholder
            )).ToList(),
            set.CreatedAt);
    }
}

// ── Update ────────────────────────────────────────────

public record UpdateIntakeQuestionSetCommand(
    Guid Id,
    string Name,
    string? Description,
    bool IsDefault,
    bool IsActive,
    List<CreateIntakeQuestionRequest>? Questions) : IRequest<IntakeQuestionSetDto>;

public class UpdateIntakeQuestionSetCommandHandler(
    IIntakeQuestionSetRepository repository,
    IIntakeQuestionRepository questionRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateIntakeQuestionSetCommand, IntakeQuestionSetDto>
{
    public async Task<IntakeQuestionSetDto> Handle(
        UpdateIntakeQuestionSetCommand request, CancellationToken cancellationToken)
    {
        var set = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(IntakeQuestionSet), request.Id);

        // Handle default flag
        if (request.IsDefault && !set.IsDefault)
        {
            var currentDefault = await repository.GetDefaultAsync(cancellationToken);
            if (currentDefault != null && currentDefault.Id != request.Id)
            {
                var trackedDefault = await repository.GetByIdAsync(currentDefault.Id, cancellationToken);
                if (trackedDefault != null) trackedDefault.IsDefault = false;
            }
        }

        set.Name = request.Name;
        set.Description = request.Description;
        set.IsDefault = request.IsDefault;
        set.IsActive = request.IsActive;

        // Replace questions: direct DELETE + INSERT (bypasses EF collection tracking issues)
        await questionRepository.DeleteBySetIdAsync(request.Id, cancellationToken);

        var newQuestions = (request.Questions ?? []).Select((q, i) => new IntakeQuestion
        {
            QuestionSetId = request.Id,
            Content = q.Content,
            OrderIndex = i,
            IsRequired = q.IsRequired,
            Placeholder = q.Placeholder,
        }).ToList();

        if (newQuestions.Count > 0)
            await questionRepository.AddRangeAsync(newQuestions, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await repository.GetWithQuestionsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(IntakeQuestionSet), request.Id);

        return new IntakeQuestionSetDto(
            updated.Id, updated.Name, updated.Description, updated.IsActive, updated.IsDefault,
            updated.Questions.OrderBy(q => q.OrderIndex).Select(q => new IntakeQuestionDto(
                q.Id, q.Content, q.OrderIndex, q.IsRequired, q.Placeholder
            )).ToList(),
            updated.CreatedAt);
    }
}

// ── Toggle Active ─────────────────────────────────────

public record ToggleIntakeQuestionSetActiveCommand(Guid Id) : IRequest<ToggleActiveResult>;
public record ToggleActiveResult(Guid Id, bool IsActive);

public class ToggleIntakeQuestionSetActiveCommandHandler(
    IIntakeQuestionSetRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ToggleIntakeQuestionSetActiveCommand, ToggleActiveResult>
{
    public async Task<ToggleActiveResult> Handle(
        ToggleIntakeQuestionSetActiveCommand request, CancellationToken cancellationToken)
    {
        var set = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(IntakeQuestionSet), request.Id);

        set.IsActive = !set.IsActive;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new ToggleActiveResult(set.Id, set.IsActive);
    }
}

// ── Delete ────────────────────────────────────────────

public record DeleteIntakeQuestionSetCommand(Guid Id) : IRequest;

public class DeleteIntakeQuestionSetCommandHandler(
    IIntakeQuestionSetRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteIntakeQuestionSetCommand>
{
    public async Task Handle(
        DeleteIntakeQuestionSetCommand request, CancellationToken cancellationToken)
    {
        var set = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(IntakeQuestionSet), request.Id);

        if (set.IsDefault)
        {
            var allSets = (await repository.GetAllAsync(cancellationToken)).ToList();
            var others = allSets.Where(s => s.Id != request.Id).ToList();
            if (others.Count == 0)
                throw new DomainException("Không thể xóa bộ câu hỏi mặc định duy nhất. Hãy tạo bộ khác trước.");

            var next = others.FirstOrDefault(s => s.IsActive) ?? others.First();
            var trackedNext = await repository.GetByIdAsync(next.Id, cancellationToken);
            if (trackedNext != null) trackedNext.IsDefault = true;
        }

        set.IsDeleted = true;
        set.IsDefault = false;
        set.IsActive = false;

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
