using MediatR;
using NeedApp.Application.DTOs.Need;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Needs.Queries;

public record GetNeedByIdQuery(Guid Id) : IRequest<NeedDto>;

public class GetNeedByIdQueryHandler(INeedRepository needRepository)
    : IRequestHandler<GetNeedByIdQuery, NeedDto>
{
    public async Task<NeedDto> Handle(GetNeedByIdQuery request, CancellationToken cancellationToken)
    {
        var need = await needRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Need), request.Id);

        return new NeedDto(
            need.Id, need.Title, need.Description, need.Location, need.Budget,
            need.Status.ToString(), need.UserId,
            need.User?.FullName ?? string.Empty,
            need.CategoryId, need.Category?.Name,
            need.CreatedAt, need.UpdatedAt
        );
    }
}
