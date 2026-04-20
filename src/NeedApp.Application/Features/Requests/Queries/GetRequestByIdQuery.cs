using MediatR;
using NeedApp.Application.DTOs.Request;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Requests.Queries;

public record GetRequestByIdQuery(Guid Id) : IRequest<RequestDto>;

public class GetRequestByIdQueryHandler(
    IRequestRepository requestRepository,
    IMessageRepository messageRepository,
    IClientUserRepository clientUserRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetRequestByIdQuery, RequestDto>
{
    public async Task<RequestDto> Handle(GetRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var userRole = currentUserService.UserRole;

        var r = await requestRepository.GetWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Request", request.Id);

        // Client can only view requests belonging to their company (ClientId match)
        if (userRole == UserRole.Client && userId.HasValue)
        {
            var clientUser = await clientUserRepository.GetByUserIdAsync(userId.Value, cancellationToken);
            if (clientUser == null)
                throw new ForbiddenException("You do not belong to any client company.");
            if (r.ClientId != clientUser.ClientId)
                throw new ForbiddenException("You do not have access to this request.");
        }

        var creator = r.Participants.FirstOrDefault(p => p.Role == ParticipantRole.Creator);
        var messageCount = await messageRepository.GetCountByRequestIdAsync(r.Id, cancellationToken);

        return new RequestDto(
            r.Id,
            r.Title,
            r.Description,
            r.Status,
            r.Priority,
            r.Client != null ? new RequestClientDto(r.Client.Id, r.Client.Name) : null,
            r.AssignedUser != null ? new RequestUserDto(r.AssignedUser.Id, r.AssignedUser.Name, r.AssignedUser.AvatarUrl) : null,
            creator != null ? new RequestUserDto(creator.UserId, creator.User?.Name, creator.User?.AvatarUrl) : null,
            messageCount,
            r.Client != null && !r.Client.IsDeleted,
            r.CreatedAt,
            r.UpdatedAt,
            r.DueDate,
            r.IsOverdue
        );
    }
}
