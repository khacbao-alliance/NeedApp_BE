using MediatR;
using NeedApp.Application.DTOs.Invitation;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Invitations.Queries;

public record GetPendingInvitationsQuery : IRequest<IEnumerable<PendingInvitationDto>>;

public class GetPendingInvitationsQueryHandler(
    IInvitationRepository invitationRepository,
    ICurrentUserService currentUserService) : IRequestHandler<GetPendingInvitationsQuery, IEnumerable<PendingInvitationDto>>
{
    public async Task<IEnumerable<PendingInvitationDto>> Handle(GetPendingInvitationsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var invitations = await invitationRepository.GetPendingByUserIdAsync(userId, cancellationToken);

        return invitations.Select(i => new PendingInvitationDto(
            i.Id,
            new InvitationClientInfo(i.Client.Id, i.Client.Name, i.Client.Description),
            new InvitationUserInfo(i.InvitedByUser.Name, i.InvitedByUser.Email, i.InvitedByUser.AvatarUrl),
            i.Role,
            i.CreatedAt
        ));
    }
}
