using MediatR;
using NeedApp.Application.DTOs.IntakeQuestion;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.IntakeQuestions.Queries;

public record GetIntakeQuestionSetsQuery : IRequest<IEnumerable<IntakeQuestionSetDto>>;

public class GetIntakeQuestionSetsQueryHandler(
    IIntakeQuestionSetRepository repository)
    : IRequestHandler<GetIntakeQuestionSetsQuery, IEnumerable<IntakeQuestionSetDto>>
{
    public async Task<IEnumerable<IntakeQuestionSetDto>> Handle(
        GetIntakeQuestionSetsQuery request, CancellationToken cancellationToken)
    {
        var sets = await repository.GetAllAsync(cancellationToken);
        return sets.Select(s => new IntakeQuestionSetDto(
            s.Id, s.Name, s.Description, s.IsActive, s.IsDefault, [], s.CreatedAt));
    }
}

public record GetIntakeQuestionSetByIdQuery(Guid Id) : IRequest<IntakeQuestionSetDto>;

public class GetIntakeQuestionSetByIdQueryHandler(
    IIntakeQuestionSetRepository repository)
    : IRequestHandler<GetIntakeQuestionSetByIdQuery, IntakeQuestionSetDto>
{
    public async Task<IntakeQuestionSetDto> Handle(
        GetIntakeQuestionSetByIdQuery request, CancellationToken cancellationToken)
    {
        var set = await repository.GetWithQuestionsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(IntakeQuestionSet), request.Id);

        return new IntakeQuestionSetDto(
            set.Id, set.Name, set.Description, set.IsActive, set.IsDefault,
            set.Questions.OrderBy(q => q.OrderIndex).Select(q => new IntakeQuestionDto(
                q.Id, q.Content, q.OrderIndex, q.IsRequired, q.Placeholder
            )).ToList(),
            set.CreatedAt);
    }
}
