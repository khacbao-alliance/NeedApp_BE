using FluentValidation;
using MediatR;
using NeedApp.Application.DTOs.Client;
using NeedApp.Application.DTOs.Invitation;
using NeedApp.Application.DTOs.Notification;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Clients.Commands;

// ─── Invite Member ─────────────────────────────────────────────────────────

public record InviteClientMemberCommand(Guid ClientId, string Email, ClientRole Role = ClientRole.Member)
    : IRequest<InvitationDto>;

public class InviteClientMemberCommandValidator : AbstractValidator<InviteClientMemberCommand>
{
    public InviteClientMemberCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Role).IsInEnum();
    }
}

public class InviteClientMemberCommandHandler(
    IClientUserRepository clientUserRepository,
    IInvitationRepository invitationRepository,
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    INotificationService notificationService,
    IUnitOfWork unitOfWork) : IRequestHandler<InviteClientMemberCommand, InvitationDto>
{
    public async Task<InvitationDto> Handle(InviteClientMemberCommand command, CancellationToken cancellationToken)
    {
        var requesterId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        // Only the Owner of this client can invite members
        var requesterMembership = await clientUserRepository.GetByUserAndClientIdAsync(requesterId, command.ClientId, cancellationToken)
            ?? throw new UnauthorizedException("You are not a member of this client.");

        if (requesterMembership.Role != ClientRole.Owner)
            throw new UnauthorizedException("Only the client Owner can invite new members.");

        // Find the user to invite by email
        var invitee = await userRepository.GetByEmailAsync(command.Email, cancellationToken)
            ?? throw new NotFoundException("User", command.Email);

        // Cannot invite a user who already belongs to a client
        var existingMembership = await clientUserRepository.GetByUserIdAsync(invitee.Id, cancellationToken);
        if (existingMembership != null)
        {
            if (existingMembership.ClientId == command.ClientId)
                throw new DomainException($"'{command.Email}' is already a member of this client.");
            else
                throw new DomainException($"'{command.Email}' already belongs to another client and cannot be invited.");
        }

        // Check for existing pending invitation
        var existingInvitation = await invitationRepository.GetPendingByUserAndClientIdAsync(invitee.Id, command.ClientId, cancellationToken);
        if (existingInvitation != null)
            throw new DomainException($"A pending invitation already exists for '{command.Email}'.");

        var invitation = new Invitation
        {
            ClientId = command.ClientId,
            InvitedUserId = invitee.Id,
            InvitedByUserId = requesterId,
            Role = command.Role,
            Status = InvitationStatus.Pending
        };

        await invitationRepository.AddAsync(invitation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var requesterName = (await userRepository.GetByIdAsync(requesterId, cancellationToken))?.Name ?? "";
        var clientName = requesterMembership.Client?.Name ?? "";

        // Notify the invited user (critical — sends email)
        await notificationService.NotifyAsync(
            invitee.Id,
            Domain.Enums.NotificationType.Invitation,
            "Bạn được mời tham gia tổ chức",
            $"{requesterName} đã mời bạn tham gia \"{clientName}\" với vai trò {command.Role}.",
            invitation.Id,
            "Invitation",
            new InvitationMetadata(clientName, requesterName, command.Role.ToString()),
            cancellationToken);

        return new InvitationDto(
            invitation.Id,
            clientName,
            invitee.Name,
            command.Role,
            InvitationStatus.Pending,
            invitation.CreatedAt
        );
    }
}

// ─── Remove Member ──────────────────────────────────────────────────────────

public record RemoveClientMemberCommand(Guid ClientId, Guid UserId) : IRequest;

public class RemoveClientMemberCommandHandler(
    IClientUserRepository clientUserRepository,
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<RemoveClientMemberCommand>
{
    public async Task Handle(RemoveClientMemberCommand command, CancellationToken cancellationToken)
    {
        var requesterId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        // Only the Owner can remove members
        var requesterMembership = await clientUserRepository.GetByUserAndClientIdAsync(requesterId, command.ClientId, cancellationToken)
            ?? throw new UnauthorizedException("You are not a member of this client.");

        if (requesterMembership.Role != ClientRole.Owner)
            throw new UnauthorizedException("Only the client Owner can remove members.");

        // Cannot remove yourself (Owner cannot remove themselves)
        if (command.UserId == requesterId)
            throw new DomainException("You cannot remove yourself from the client.");

        var target = await clientUserRepository.GetByUserAndClientIdAsync(command.UserId, command.ClientId, cancellationToken)
            ?? throw new NotFoundException("Member", command.UserId);

        target.IsDeleted = true;
        clientUserRepository.Update(target);

        // Reset the kicked user's HasClient flag so they are redirected to setup-client
        var kickedUser = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (kickedUser is not null)
        {
            kickedUser.HasClient = false;
            userRepository.Update(kickedUser);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
