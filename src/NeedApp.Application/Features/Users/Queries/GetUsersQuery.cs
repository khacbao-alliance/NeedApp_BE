using MediatR;
using NeedApp.Application.Common.Models;
using NeedApp.Application.DTOs.User;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Users.Queries;

public record GetUsersQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    UserRole? Role = null
) : IRequest<PaginatedResult<UserDetailDto>>;

public class GetUsersQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUsersQuery, PaginatedResult<UserDetailDto>>
{
    public async Task<PaginatedResult<UserDetailDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await userRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.Role,
            cancellationToken);

        var dtos = items.Select(u => new UserDetailDto(u.Id, u.Email, u.Name, u.Role, u.CreatedAt, u.UpdatedAt));

        return PaginatedResult<UserDetailDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }
}
