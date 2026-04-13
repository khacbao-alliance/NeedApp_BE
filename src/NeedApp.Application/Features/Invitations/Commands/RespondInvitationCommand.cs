using MediatR;
using NeedApp.Application.DTOs.Invitation;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Invitations.Commands;

public record RespondInvitationCommand(Guid InvitationId, bool Accept) : IRequest<InvitationDto>;

public class RespondInvitationCommandHandler(
    IInvitationRepository invitationRepository,
    IClientUserRepository clientUserRepository,
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<RespondInvitationCommand, InvitationDto>
{
    public async Task<InvitationDto> Handle(RespondInvitationCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var invitation = await invitationRepository.GetByIdForUpdateAsync(command.InvitationId, cancellationToken)
            ?? throw new NotFoundException("Invitation", command.InvitationId);

        // Only the invited user can respond
        if (invitation.InvitedUserId != userId)
            throw new UnauthorizedException("You are not the invited user.");

        if (invitation.Status != InvitationStatus.Pending)
            throw new DomainException("This invitation has already been responded to.");

        if (command.Accept)
        {
            // Check if user already belongs to an active client
            var existingMembership = await clientUserRepository.GetByUserIdAsync(userId, cancellationToken);
            if (existingMembership != null)
                throw new DomainException("You already belong to a client. Please leave your current client first.");

            // Check for a previously soft-deleted membership (user was kicked before)
            var deletedMembership = await clientUserRepository.GetByUserAndClientIdIncludeDeletedAsync(
                userId, invitation.ClientId, cancellationToken);

            if (deletedMembership != null)
            {
                // Reactivate the existing record
                deletedMembership.IsDeleted = false;
                deletedMembership.Role = invitation.Role;
                deletedMembership.CreatedBy = invitation.InvitedByUserId;
                clientUserRepository.Update(deletedMembership);
            }
            else
            {
                // Create new ClientUser membership
                var clientUser = new ClientUser
                {
                    ClientId = invitation.ClientId,
                    UserId = userId,
                    Role = invitation.Role,
                    CreatedBy = invitation.InvitedByUserId
                };
                await clientUserRepository.AddAsync(clientUser, cancellationToken);
            }

            // Update user's HasClient flag
            var user = await userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is not null)
            {
                user.HasClient = true;
                userRepository.Update(user);
            }

            invitation.Status = InvitationStatus.Accepted;
        }
        else
        {
            invitation.Status = InvitationStatus.Declined;
        }

        invitation.RespondedAt = DateTime.UtcNow;
        invitationRepository.Update(invitation);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new InvitationDto(
            invitation.Id,
            invitation.Client.Name,
            invitation.InvitedByUser.Name,
            invitation.Role,
            invitation.Status,
            invitation.CreatedAt
        );
    }
}
