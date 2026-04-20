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

        // Handle intake answer flow
        if (command.Type == MessageType.IntakeAnswer && request.Status == RequestStatus.Intake && request.IntakeQuestionSetId.HasValue)
        {
            request.IntakeProgress++;

            var questionSet = await intakeRepo.GetWithQuestionsAsync(request.IntakeQuestionSetId.Value, cancellationToken);
            if (questionSet != null)
            {
                var questions = questionSet.Questions.OrderBy(q => q.OrderIndex).ToList();
                if (request.IntakeProgress < questions.Count)
                {
                    // Post next question
                    var next = questions[request.IntakeProgress];
                    var nextMsg = new Message
                    {
                        RequestId = request.Id,
                        Type = MessageType.IntakeQuestion,
                        Content = next.Content,
                        Metadata = JsonSerializer.SerializeToDocument(new
                        {
                            questionId = next.Id,
                            orderIndex = next.OrderIndex,
                            isRequired = next.IsRequired,
                            placeholder = next.Placeholder,
                            totalQuestions = questions.Count,
                            currentQuestion = request.IntakeProgress + 1
                        })
                    };
                    await messageRepository.AddAsync(nextMsg, cancellationToken);
                }
                else
                {
                    // All questions answered → status changes to Pending
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

        var messageDto = new MessageDto(
            message.Id, message.Type, message.Content,
            senderDto, null, message.ReplyToId, [], message.CreatedAt);

        // Push via SignalR for real-time chat (not intake — intake is sequential/REST)
        if (command.Type == MessageType.Text || command.Type == MessageType.File)
        {
            await chatHubService.SendMessageToRequest(command.RequestId, messageDto);

            // Notify other participants about new message
            var participants = await participantRepository.GetByRequestIdAsync(command.RequestId, cancellationToken);
            var otherUserIds = participants.Where(p => p.UserId != userId).Select(p => p.UserId);
            await notificationService.NotifyMultipleAsync(
                otherUserIds,
                Domain.Enums.NotificationType.NewMessage,
                $"Tin nhắn mới trong \"{request.Title}\"",
                command.Content?.Length > 100 ? command.Content[..100] + "..." : command.Content ?? "",
                command.RequestId,
                "Request",
                new NewMessageMetadata(
                    request.Title,
                    command.Content?.Length > 100 ? command.Content[..100] + "..." : command.Content ?? ""
                ),
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
