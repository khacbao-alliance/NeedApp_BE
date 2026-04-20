using MediatR;
using NeedApp.Application.Common.Models;
using NeedApp.Application.DTOs.Request;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Requests.Queries;

public record GetRequestsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    RequestStatus? Status = null,
    RequestPriority? Priority = null,
    // ── Advanced filters ──
    Guid? AssignedTo = null,
    Guid? ClientId = null,
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null,
    bool? IsOverdue = null,
    string? SortBy = null
) : IRequest<PaginatedResult<RequestDto>>;

public class GetRequestsQueryHandler(
    IRequestRepository requestRepository,
    IMessageRepository messageRepository,
    IClientUserRepository clientUserRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetRequestsQuery, PaginatedResult<RequestDto>>
{
    public async Task<PaginatedResult<RequestDto>> Handle(GetRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var userRole = currentUserService.UserRole;

        // Resolve ClientId for Client-role users so GetPagedAsync can filter by company
        Guid? currentClientId = null;
        if (userRole == UserRole.Client && userId.HasValue)
        {
            var clientUser = await clientUserRepository.GetByUserIdAsync(userId.Value, cancellationToken);
            currentClientId = clientUser?.ClientId;
        }

        var (items, totalCount) = await requestRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.Status,
            request.Priority,
            userId,
            userRole,
            currentClientId,
            request.AssignedTo,
            request.ClientId,
            request.DateFrom,
            request.DateTo,
            request.IsOverdue,
            request.SortBy,
            cancellationToken);

        var requestList = items.ToList();

        // Batch fetch message counts in a single query instead of loading all messages
        var messageCounts = await messageRepository.GetCountsByRequestIdsAsync(
            requestList.Select(r => r.Id), cancellationToken);

        var dtos = requestList.Select(r => new RequestDto(
            r.Id,
            r.Title,
            r.Description,
            r.Status,
            r.Priority,
            r.Client != null ? new RequestClientDto(r.Client.Id, r.Client.Name) : null,
            r.AssignedUser != null ? new RequestUserDto(r.AssignedUser.Id, r.AssignedUser.Name, r.AssignedUser.AvatarUrl) : null,
            r.Participants.Any(p => p.Role == ParticipantRole.Creator)
                ? new RequestUserDto(
                    r.Participants.First(p => p.Role == ParticipantRole.Creator).UserId,
                    r.Participants.First(p => p.Role == ParticipantRole.Creator).User?.Name,
                    r.Participants.First(p => p.Role == ParticipantRole.Creator).User?.AvatarUrl)
                : null,
            messageCounts.GetValueOrDefault(r.Id, 0),
            r.Client != null && !r.Client.IsDeleted,
            r.CreatedAt,
            r.UpdatedAt,
            r.DueDate,
            r.IsOverdue
        ));

        return PaginatedResult<RequestDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }
}
