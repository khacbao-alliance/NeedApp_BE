using System.Text.Json;
using FluentValidation;
using MediatR;
using NeedApp.Application.DTOs.Message;
using NeedApp.Application.DTOs.Notification;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Messages.Commands;

// --- Send Message ---
public record SendMessageCommand(Guid RequestId, string Content, MessageType Type = MessageType.Text, Guid? ReplyToId = null) : IRequest<MessageDto>;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().When(x => x.Type == MessageType.Text || x.Type == MessageType.IntakeAnswer).MaximumLength(10000);
        RuleFor(x => x.Type).IsInEnum();
    }
}

public class SendMessageCommandHandler(
    IRequestRepository requestRepository,
    IMessageRepository messageRepository,
    IRequestParticipantRepository participantRepository,
    IClientUserRepository clientUserRepository,
    IIntakeQuestionSetRepository intakeRepo,
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IChatHubService chatHubService,
    INotificationService notificationService,
    IUnitOfWork unitOfWork) : IRequestHandler<SendMessageCommand, MessageDto>
{
    public async Task<MessageDto> Handle(SendMessageCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");
        var userRole = currentUserService.UserRole;

        var request = await requestRepository.GetByIdAsync(command.RequestId, cancellationToken)
            ?? throw new NotFoundException(nameof(Request), command.RequestId);

        // Access check:
        // - Staff/Admin: must already be a participant (assigned)
        // - Client: must be a member of the Client company that owns the request
        var isParticipant = await participantRepository.IsParticipantAsync(command.RequestId, userId, cancellationToken);

        if (!isParticipant)
        {
            if (userRole == UserRole.Client)
            {
                var clientUser = await clientUserRepository.GetByUserIdAsync(userId, cancellationToken);
                if (clientUser == null || clientUser.ClientId != request.ClientId)
                    throw new ForbiddenException("You are not a member of the client that owns this request.");

                // Auto-join as Observer on first message
                await participantRepository.AddAsync(new RequestParticipant
                {
                    RequestId = command.RequestId,
                    UserId = userId,
                    Role = ParticipantRole.Observer
                }, cancellationToken);
            }
            else
            {
                throw new ForbiddenException("You are not a participant of this request.");
            }
        }

        // Get sender info for response
        var sender = await userRepository.GetByIdAsync(userId, cancellationToken);
        var senderDto = sender != null
            ? new MessageSenderDto(sender.Id, sender.Name, sender.Role, sender.AvatarUrl)
            : new MessageSenderDto(userId, null, null, null);

        var message = new Message
        {
            RequestId = command.RequestId,
            SenderId = userId,
            Type = command.Type,
            Content = command.Content,
            ReplyToId = command.ReplyToId
        };

        await messageRepository.AddAsync(message, cancellationToken);

        request.UpdatedAt = DateTime.UtcNow;
        request.UpdatedBy = userId;
        requestRepository.Update(request);

        // ── Duplicate-answer guard (applies at ANY status) ─────────────────
        // Prevent double-submit: if an IntakeAnswer already exists for this question, discard.
        if (command.Type == MessageType.IntakeAnswer && command.ReplyToId.HasValue)
        {
            var existingForQuestion = await messageRepository.GetByTypeAsync(request.Id, MessageType.IntakeAnswer, cancellationToken);
            var alreadyAnswered = existingForQuestion.Any(a => a.ReplyToId == command.ReplyToId);
            if (alreadyAnswered)
            {
                // Discard the duplicate silently — optimistic UI will reconcile with next poll
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return new MessageDto(message.Id, message.Type, message.Content, senderDto, null, message.ReplyToId, [], message.CreatedAt);
            }
        }
        // ───────────────────────────────────────────────────────────────────

        // Handle intake completion check (only relevant during Intake status)
        if (command.Type == MessageType.IntakeAnswer && request.Status == RequestStatus.Intake && request.IntakeQuestionSetId.HasValue)
        {
            var questionSet = await intakeRepo.GetWithQuestionsAsync(request.IntakeQuestionSetId.Value, cancellationToken);
            if (questionSet != null)
            {
                var requiredQuestions = questionSet.Questions.Where(q => q.IsRequired).ToList();
                
                // Get all existing answers to see if we're done
                var existingAnswers = await messageRepository.GetByTypeAsync(request.Id, MessageType.IntakeAnswer, cancellationToken);
                
                // +1 to account for the current message being added
                request.IntakeProgress = existingAnswers.Count() + 1;

                var answeredQuestionMessageIds = existingAnswers
                    .Where(a => a.ReplyToId.HasValue)
                    .Select(a => a.ReplyToId!.Value)
                    .ToHashSet();

                if (command.ReplyToId.HasValue)
                {
                    answeredQuestionMessageIds.Add(command.ReplyToId.Value);
                }

                var questionMessages = await messageRepository.GetByTypeAsync(request.Id, MessageType.IntakeQuestion, cancellationToken);
                
                bool allRequiredAnswered = true;
                foreach (var rq in requiredQuestions)
                {
                    var qMsg = questionMessages.FirstOrDefault(m => 
                    {
                        if (string.IsNullOrEmpty(m.Metadata?.ToString())) return false;
                        try {
                            var doc = JsonSerializer.Deserialize<JsonElement>(m.Metadata.ToString()!);
                            return doc.TryGetProperty("questionId", out var idProp) && idProp.GetGuid() == rq.Id;
                        } catch { return false; }
                    });

                    if (qMsg != null && !answeredQuestionMessageIds.Contains(qMsg.Id))
                    {
                        allRequiredAnswered = false;
                        break;
                    }
                }

                if (allRequiredAnswered)
                {
                    // All required questions answered → status changes to Pending
                    request.Status = RequestStatus.Pending;
                    var doneMsg = new Message
                    {
                        RequestId = request.Id,
                        Type = MessageType.System,
                        Content = "All intake questions have been answered. Your request is now being reviewed."
                    };
                    await messageRepository.AddAsync(doneMsg, cancellationToken);
                }
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        MessageReplyDto? replyTo = null;
        if (command.ReplyToId.HasValue)
        {
            var replyMsg = await messageRepository.GetByIdAsync(command.ReplyToId.Value, cancellationToken);
            if (replyMsg != null)
            {
                var replySender = replyMsg.SenderId.HasValue
                    ? await userRepository.GetByIdAsync(replyMsg.SenderId.Value, cancellationToken)
                    : null;
                replyTo = new MessageReplyDto(replyMsg.Id, replyMsg.Content, replySender?.Name);
            }
        }

        var messageDto = new MessageDto(
            message.Id, message.Type, message.Content,
            senderDto, null, message.ReplyToId, [], message.CreatedAt,
            ReplyTo: replyTo);

        // Push via SignalR and notify participants for all user-generated messages
        if (command.Type == MessageType.Text || command.Type == MessageType.File || command.Type == MessageType.IntakeAnswer)
        {
            await chatHubService.SendMessageToRequest(command.RequestId, messageDto);

            // Notify other participants about new message / intake answer
            var participants = await participantRepository.GetByRequestIdAsync(command.RequestId, cancellationToken);
            var otherUserIds = participants.Where(p => p.UserId != userId).Select(p => p.UserId);
            var notificationBody = command.Content?.Length > 100 ? command.Content[..100] + "..." : command.Content ?? "";
            var notificationTitle = command.Type == MessageType.IntakeAnswer
                ? $"Câu trả lời mới trong \"{request.Title}\""
                : $"Tin nhắn mới trong \"{request.Title}\"";

            await notificationService.NotifyMultipleAsync(
                otherUserIds,
                Domain.Enums.NotificationType.NewMessage,
                notificationTitle,
                notificationBody,
                command.RequestId,
                "Request",
                new NewMessageMetadata(request.Title, notificationBody),
                cancellationToken);
        }

        // If intake just completed, notify via SignalR that status changed
        if (request.Status == RequestStatus.Pending && command.Type == MessageType.IntakeAnswer)
        {
            await chatHubService.SendRequestStatusChanged(command.RequestId, request.Status.ToString());
        }

        return messageDto;
    }
}

// --- Send Missing Info ---
public record SendMissingInfoCommand(Guid RequestId, string Content, List<string> Questions) : IRequest<MessageDto>;

public class SendMissingInfoCommandValidator : AbstractValidator<SendMissingInfoCommand>
{
    public SendMissingInfoCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.Questions).NotEmpty().Must(q => q.Count <= 10).WithMessage("Maximum 10 questions.");
        RuleForEach(x => x.Questions).NotEmpty().MaximumLength(500);
    }
}

public class SendMissingInfoCommandHandler(
    IRequestRepository requestRepository,
    IMessageRepository messageRepository,
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IChatHubService chatHubService,
    INotificationService notificationService,
    IUnitOfWork unitOfWork) : IRequestHandler<SendMissingInfoCommand, MessageDto>
{
    public async Task<MessageDto> Handle(SendMissingInfoCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var request = await requestRepository.GetByIdAsync(command.RequestId, cancellationToken)
            ?? throw new NotFoundException(nameof(Request), command.RequestId);

        var sender = await userRepository.GetByIdAsync(userId, cancellationToken);
        var senderDto = sender != null
            ? new MessageSenderDto(sender.Id, sender.Name, sender.Role, sender.AvatarUrl)
            : new MessageSenderDto(userId, null, null, null);

        var questions = command.Questions.Select((q, i) => new { id = $"q{i + 1}", question = q, answered = false });
        var metadata = JsonSerializer.SerializeToDocument(new { questions });

        var message = new Message
        {
            RequestId = command.RequestId,
            SenderId = userId,
            Type = MessageType.MissingInfo,
            Content = command.Content,
            Metadata = metadata
        };

        await messageRepository.AddAsync(message, cancellationToken);

        request.Status = RequestStatus.MissingInfo;
        request.UpdatedAt = DateTime.UtcNow;
        request.UpdatedBy = userId;
        requestRepository.Update(request);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var messageDto = new MessageDto(
            message.Id, message.Type, message.Content,
            senderDto,
            JsonSerializer.Deserialize<object>(metadata),
            null, [], message.CreatedAt);

        // Push MissingInfo via SignalR so client sees it in real-time
        await chatHubService.SendMessageToRequest(command.RequestId, messageDto);
        await chatHubService.SendRequestStatusChanged(command.RequestId, request.Status.ToString());

        // Notify request creator about missing info (critical — sends email)
        if (request.CreatedBy.HasValue)
        {
            await notificationService.NotifyAsync(
                request.CreatedBy.Value,
                Domain.Enums.NotificationType.MissingInfo,
                "Yêu cầu bổ sung thông tin",
                $"Request \"{request.Title}\" cần bổ sung thông tin: {command.Content}",
                command.RequestId,
                "Request",
                new MissingInfoMetadata(request.Title, command.Content),
                cancellationToken);
        }

        return messageDto;
    }
}

// --- Edit Message ---
public record EditMessageCommand(Guid RequestId, Guid MessageId, string Content) : IRequest<MessageDto>;

public class EditMessageCommandValidator : AbstractValidator<EditMessageCommand>
{
    public EditMessageCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(10000);
    }
}

public class EditMessageCommandHandler(
    IMessageRepository messageRepository,
    IMessageEditHistoryRepository historyRepository,
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IChatHubService chatHubService,
    INotificationService notificationService,
    IRequestRepository requestRepository,
    IRequestParticipantRepository participantRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<EditMessageCommand, MessageDto>
{
    private static readonly TimeSpan EditWindow = TimeSpan.FromMinutes(15);

    public async Task<MessageDto> Handle(EditMessageCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var message = await messageRepository.GetByIdAsync(command.MessageId, cancellationToken)
            ?? throw new NotFoundException(nameof(Message), command.MessageId);

        if (message.RequestId != command.RequestId)
            throw new DomainException("Message does not belong to this request.");

        if (message.SenderId != userId)
            throw new ForbiddenException("You can only edit your own messages.");

        if (message.Type != MessageType.Text && message.Type != MessageType.IntakeAnswer)
            throw new DomainException("Only text messages and intake answers can be edited.");

        if (message.IsDeleted)
            throw new DomainException("Cannot edit a deleted message.");

        // Enforce 15-minute edit window only for regular Text messages.
        // IntakeAnswer messages can be edited at any time during the intake phase.
        if (message.Type == MessageType.Text && DateTime.UtcNow - message.CreatedAt > EditWindow)
            throw new DomainException("Messages can only be edited within 15 minutes of sending.");

        // Snapshot the current content into edit history before overwriting
        var historyEntry = new MessageEditHistory
        {
            MessageId = message.Id,
            PreviousContent = message.Content ?? string.Empty,
            EditedAt = DateTime.UtcNow,
            EditedBy = userId
        };
        await historyRepository.AddAsync(historyEntry, cancellationToken);

        message.Content = command.Content;
        message.IsEdited = true;
        message.EditedAt = DateTime.UtcNow;

        messageRepository.Update(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var sender = await userRepository.GetByIdAsync(userId, cancellationToken);
        var senderDto = sender != null
            ? new MessageSenderDto(sender.Id, sender.Name, sender.Role, sender.AvatarUrl)
            : new MessageSenderDto(userId, null, null, null);

        var messageDto = new MessageDto(
            message.Id, message.Type, message.Content, senderDto, null,
            message.ReplyToId, [], message.CreatedAt, null, null,
            message.IsEdited, message.EditedAt, message.IsPinned);

        await chatHubService.SendMessageEdited(command.RequestId, messageDto);

        // ── Notify other participants when an IntakeAnswer is edited ──────
        if (message.Type == MessageType.IntakeAnswer)
        {
            var request = await requestRepository.GetByIdAsync(command.RequestId, cancellationToken);
            if (request != null)
            {
                var participants = await participantRepository.GetByRequestIdAsync(command.RequestId, cancellationToken);
                var otherUserIds = participants.Where(p => p.UserId != userId).Select(p => p.UserId);

                // Build a short preview of the question being answered
                string questionPreview = "";
                if (message.ReplyToId.HasValue)
                {
                    var questionMsg = await messageRepository.GetByIdAsync(message.ReplyToId.Value, cancellationToken);
                    if (questionMsg != null)
                    {
                        questionPreview = questionMsg.Content?.Length > 60
                            ? questionMsg.Content[..60] + "..."
                            : questionMsg.Content ?? "";
                    }
                }

                var editorName = sender?.Name ?? "Client";
                var notificationTitle = $"Câu trả lời intake đã được sửa trong \"{request.Title}\"";
                var notificationBody = command.Content.Length > 100 ? command.Content[..100] + "..." : command.Content;

                await notificationService.NotifyMultipleAsync(
                    otherUserIds,
                    Domain.Enums.NotificationType.IntakeAnswerEdited,
                    notificationTitle,
                    notificationBody,
                    command.RequestId,
                    "Request",
                    new IntakeAnswerEditedMetadata(request.Title, questionPreview, editorName),
                    cancellationToken);
            }
        }
        // ──────────────────────────────────────────────────────────────────

        return messageDto;
    }
}


// --- Pin Message ---
public record PinMessageCommand(Guid RequestId, Guid MessageId) : IRequest<MessageDto>;

public class PinMessageCommandHandler(
    IMessageRepository messageRepository,
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IChatHubService chatHubService,
    IUnitOfWork unitOfWork) : IRequestHandler<PinMessageCommand, MessageDto>
{
    public async Task<MessageDto> Handle(PinMessageCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var message = await messageRepository.GetByIdAsync(command.MessageId, cancellationToken)
            ?? throw new NotFoundException(nameof(Message), command.MessageId);

        if (message.RequestId != command.RequestId)
            throw new DomainException("Message does not belong to this request.");

        if (message.IsDeleted)
            throw new DomainException("Cannot pin a deleted message.");

        message.IsPinned = !message.IsPinned;

        messageRepository.Update(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var sender = message.Sender != null
            ? new MessageSenderDto(message.Sender.Id, message.Sender.Name, message.Sender.Role, message.Sender.AvatarUrl)
            : (message.SenderId.HasValue ? new MessageSenderDto(message.SenderId.Value, null, null, null) : null);

        var messageDto = new MessageDto(
            message.Id, message.Type, message.Content, sender, null,
            message.ReplyToId, [], message.CreatedAt, null, null,
            message.IsEdited, message.EditedAt, message.IsPinned);

        await chatHubService.SendMessagePinned(command.RequestId, command.MessageId, message.IsPinned);

        return messageDto;
    }
}

// --- Delete Message ---
public record DeleteMessageCommand(Guid RequestId, Guid MessageId) : IRequest;

public class DeleteMessageCommandHandler(
    IMessageRepository messageRepository,
    ICurrentUserService currentUserService,
    IChatHubService chatHubService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteMessageCommand>
{
    public async Task Handle(DeleteMessageCommand command, CancellationToken cancellationToken)
    {
        var message = await messageRepository.GetByIdAsync(command.MessageId, cancellationToken)
            ?? throw new NotFoundException(nameof(Message), command.MessageId);

        if (message.RequestId != command.RequestId)
            throw new DomainException("Message does not belong to this request.");

        var userId = currentUserService.UserId;
        if (message.SenderId != userId)
            throw new ForbiddenException("You can only delete your own messages.");

        message.IsDeleted = true;
        messageRepository.Update(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify others that message was deleted
        await chatHubService.SendMessageDeleted(command.RequestId, command.MessageId);
    }
}
