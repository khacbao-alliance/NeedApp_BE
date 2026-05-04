using MediatR;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Messages.Commands;

// --- Toggle Reaction ---
public record ToggleReactionCommand(Guid MessageId, string Emoji) : IRequest<ToggleReactionResponse>;
public record ToggleReactionResponse(bool Added, string Emoji, int Count);

public class ToggleReactionCommandHandler(
    IRepository<Message> messageRepository,
    IRepository<MessageReaction> reactionRepository,
    ICurrentUserService currentUserService,
    IChatHubService chatHubService,
    IUnitOfWork unitOfWork) : IRequestHandler<ToggleReactionCommand, ToggleReactionResponse>
{
    public async Task<ToggleReactionResponse> Handle(ToggleReactionCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var message = await messageRepository.GetByIdAsync(command.MessageId, cancellationToken)
            ?? throw new NotFoundException(nameof(Message), command.MessageId);

        // Check if user already reacted with this emoji
        var existing = (await reactionRepository.FindAsync(
            r => r.MessageId == command.MessageId && r.UserId == userId && r.Emoji == command.Emoji,
            cancellationToken)).FirstOrDefault();

        bool added;
        if (existing != null)
        {
            // Remove reaction (toggle off)
            reactionRepository.Remove(existing);
            added = false;
        }
        else
        {
            // Add reaction (toggle on)
            await reactionRepository.AddAsync(new MessageReaction
            {
                MessageId = command.MessageId,
                UserId = userId,
                Emoji = command.Emoji
            }, cancellationToken);
            added = true;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Get updated reaction data for this emoji on this message
        var reactions = await reactionRepository.FindAsync(
            r => r.MessageId == command.MessageId && r.Emoji == command.Emoji,
            cancellationToken);
        var reactionList = reactions.ToList();

        // Broadcast reaction update to all participants via SignalR
        await chatHubService.SendReactionToggled(
            message.RequestId,
            command.MessageId,
            command.Emoji,
            reactionList.Count,
            reactionList.Select(r => r.UserId).ToList());

        return new ToggleReactionResponse(added, command.Emoji, reactionList.Count);
    }
}

// --- Mark Read ---
public record MarkReadCommand(Guid RequestId) : IRequest;

public class MarkReadCommandHandler(
    IRepository<MessageReadReceipt> receiptRepo,
    IChatHubService chatHubService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkReadCommand>
{
    public async Task Handle(MarkReadCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var existing = (await receiptRepo.FindAsync(
            r => r.RequestId == command.RequestId && r.UserId == userId,
            cancellationToken)).FirstOrDefault();

        var now = DateTime.UtcNow;

        if (existing != null)
        {
            existing.LastReadAt = now;
            receiptRepo.Update(existing);
        }
        else
        {
            await receiptRepo.AddAsync(new MessageReadReceipt
            {
                RequestId = command.RequestId,
                UserId = userId,
                LastReadAt = now
            }, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Push real-time event so other participants see "seen" indicator immediately
        await chatHubService.SendMessageRead(command.RequestId, userId, now);
    }
}
