using MediatR;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Clients.Commands;

/// <summary>
/// A Member (non-owner) leaves their client organization.
/// Owners cannot leave — they must delete the client or transfer ownership first.
/// </summary>
public record LeaveClientCommand : IRequest;

public class LeaveClientCommandHandler(
    IClientUserRepository clientUserRepository,
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<LeaveClientCommand>
{
    public async Task Handle(LeaveClientCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        // Locate the caller's membership
        var membership = await clientUserRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new DomainException("You are not a member of any client.");

        // Owners must delete the client instead of leaving
        if (membership.Role == ClientRole.Owner)
            throw new DomainException(
                "As the Owner you cannot leave the organization. " +
                "Please delete it instead (or transfer ownership first).");

        // Soft-delete the ClientUser record
        membership.IsDeleted = true;
        clientUserRepository.Update(membership);

        // Reset HasClient flag on the user
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is not null)
        {
            user.HasClient = false;
            userRepository.Update(user);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
