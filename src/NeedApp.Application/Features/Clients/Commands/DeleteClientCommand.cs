using MediatR;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Clients.Commands;

/// <summary>
/// Owner deletes the entire client organization.
/// All members lose their client membership (HasClient = false).
/// </summary>
public record DeleteClientCommand : IRequest;

public class DeleteClientCommandHandler(
    IClientRepository clientRepository,
    IClientUserRepository clientUserRepository,
    IRequestRepository requestRepository,
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteClientCommand>
{
    public async Task Handle(DeleteClientCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        // Locate the caller's membership
        var membership = await clientUserRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new DomainException("You are not a member of any client.");

        if (membership.Role != ClientRole.Owner)
            throw new UnauthorizedException("Only the Owner can delete the client organization.");

        var clientId = membership.ClientId;

        // Load client (tracked) for deletion
        var client = await clientRepository.GetByIdAsync(clientId, cancellationToken)
            ?? throw new NotFoundException("Client", clientId);

        // Load all members of this client
        var allMembers = (await clientUserRepository.GetByClientIdAsync(clientId, cancellationToken)).ToList();
        var memberUserIds = allMembers.Select(m => m.UserId).ToList();

        // Soft-delete all ClientUser records
        foreach (var m in allMembers)
        {
            m.IsDeleted = true;
            clientUserRepository.Update(m);
        }

        // Set HasClient = false for all users
        foreach (var memberId in memberUserIds)
        {
            var user = await userRepository.GetByIdAsync(memberId, cancellationToken);
            if (user is not null)
            {
                user.HasClient = false;
                userRepository.Update(user);
            }
        }

        // Auto-cancel all open requests belonging to this client
        var openRequests = (await requestRepository.GetByClientIdAsync(clientId, cancellationToken))
            .Where(r => r.Status != RequestStatus.Done && r.Status != RequestStatus.Cancelled)
            .ToList();
        foreach (var req in openRequests)
        {
            // Re-fetch tracked entity (GetByClientIdAsync uses AsNoTracking)
            var tracked = await requestRepository.GetByIdAsync(req.Id, cancellationToken);
            if (tracked is not null)
            {
                tracked.Status = RequestStatus.Cancelled;
                requestRepository.Update(tracked);
            }
        }

        // Soft-delete the client itself
        client.IsDeleted = true;
        clientRepository.Update(client);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
