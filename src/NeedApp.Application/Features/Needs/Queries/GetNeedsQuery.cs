using MediatR;
using NeedApp.Application.Common.Models;
using NeedApp.Application.DTOs.Need;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Needs.Queries;

public record GetNeedsQuery(int Page = 1, int PageSize = 10) : IRequest<PaginatedResult<NeedDto>>;

public class GetNeedsQueryHandler(INeedRepository needRepository)
    : IRequestHandler<GetNeedsQuery, PaginatedResult<NeedDto>>
{
    public async Task<PaginatedResult<NeedDto>> Handle(GetNeedsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await needRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(n => new NeedDto(
            n.Id, n.Title, n.Description, n.Location, n.Budget,
            n.Status.ToString(), n.UserId,
            n.User?.FullName ?? string.Empty,
            n.CategoryId, n.Category?.Name,
            n.CreatedAt, n.UpdatedAt
        ));

        return PaginatedResult<NeedDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }
}
