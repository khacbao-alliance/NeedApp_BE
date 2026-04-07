using MediatR;
using NeedApp.Application.DTOs.Client;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Clients.Queries;

public record GetClientMembersQuery(Guid ClientId) : IRequest<List<ClientMemberDto>>;

public class GetClientMembersQueryHandler(
    IClientUserRepository clientUserRepository,
    ICurrentUserService currentUserService) : IRequestHandler<GetClientMembersQuery, List<ClientMemberDto>>
{
    public async Task<List<ClientMemberDto>> Handle(GetClientMembersQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");
        var userRole = currentUserService.UserRole;

        // Client users can only view members of their own client
        // Staff/Admin can view any client's members
        if (userRole == UserRole.Client)
        {
            var requesterMembership = await clientUserRepository.GetByUserAndClientIdAsync(userId, query.ClientId, cancellationToken);
            if (requesterMembership == null)
                throw new NotFoundException("Client", query.ClientId);
        }

        var members = await clientUserRepository.GetByClientIdAsync(query.ClientId, cancellationToken);

        return members.Select(cu => new ClientMemberDto(
            cu.UserId,
            cu.User?.Name,
            cu.User?.Email,
            cu.Role,
            cu.User?.AvatarUrl,
            cu.CreatedAt
        )).ToList();
    }
}
