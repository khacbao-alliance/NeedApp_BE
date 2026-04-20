using MediatR;
using NeedApp.Application.DTOs.SlaConfig;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.SlaConfig;

public record GetSlaConfigsQuery : IRequest<IEnumerable<SlaConfigDto>>;

public class GetSlaConfigsQueryHandler(
    ISlaConfigRepository slaConfigRepository)
    : IRequestHandler<GetSlaConfigsQuery, IEnumerable<SlaConfigDto>>
{
    public async Task<IEnumerable<SlaConfigDto>> Handle(GetSlaConfigsQuery request, CancellationToken cancellationToken)
    {
        var configs = await slaConfigRepository.GetAllConfigsAsync(cancellationToken);
        return configs.Select(c => new SlaConfigDto(
            c.Id,
            c.Priority.ToString(),
            c.DeadlineHours,
            c.Description
        ));
    }
}
