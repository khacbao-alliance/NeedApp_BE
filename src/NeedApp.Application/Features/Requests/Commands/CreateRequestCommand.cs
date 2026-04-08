using System.Text.Json;
using FluentValidation;
using MediatR;
using NeedApp.Application.DTOs.Message;
using NeedApp.Application.DTOs.Request;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Requests.Commands;

public record CreateRequestCommand(string Title, string? Description, RequestPriority Priority = RequestPriority.Medium) : IRequest<CreateRequestResponse>;

public class CreateRequestCommandValidator : AbstractValidator<CreateRequestCommand>
{
    public CreateRequestCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
    }
}

public class CreateRequestCommandHandler(
    IRequestRepository requestRepository,
    IClientUserRepository clientUserRepository,
    IMessageRepository messageRepository,
    IRequestParticipantRepository participantRepository,
    IIntakeQuestionSetRepository intakeRepo,
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    INotificationService notificationService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateRequestCommand, CreateRequestResponse>
{
    public async Task<CreateRequestResponse> Handle(CreateRequestCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var clientUser = await clientUserRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new DomainException("Please create your client profile first.");

        // Get default intake question set
        var questionSet = await intakeRepo.GetDefaultAsync(cancellationToken);

        var request = new Request
        {
            Title = command.Title,
            Description = command.Description,
            ClientId = clientUser.ClientId,
            Status = questionSet != null ? RequestStatus.Intake : RequestStatus.Pending,
            Priority = command.Priority,
            IntakeQuestionSetId = questionSet?.Id,
            IntakeProgress = 0,
            CreatedBy = userId
        };

        await requestRepository.AddAsync(request, cancellationToken);

        // Add creator as participant
        await participantRepository.AddAsync(new RequestParticipant
        {
            RequestId = request.Id,
            UserId = userId,
            Role = ParticipantRole.Creator
        }, cancellationToken);

        // System message
        await messageRepository.AddAsync(new Message
        {
            RequestId = request.Id,
            Type = MessageType.System,
            Content = $"Request \"{command.Title}\" has been created."
        }, cancellationToken);

        // If we have intake questions, post the first one
        MessageDto? firstQuestion = null;
        if (questionSet?.Questions.Any() == true)
        {
            var first = questionSet.Questions.OrderBy(q => q.OrderIndex).First();
            var msg = new Message
            {
                RequestId = request.Id,
                Type = MessageType.IntakeQuestion,
                Content = first.Content,
                Metadata = JsonSerializer.SerializeToDocument(new
                {
                    questionId = first.Id,
                    orderIndex = first.OrderIndex,
                    isRequired = first.IsRequired,
                    placeholder = first.Placeholder,
                    totalQuestions = questionSet.Questions.Count
                })
            };
            await messageRepository.AddAsync(msg, cancellationToken);

            firstQuestion = new MessageDto(
                msg.Id, msg.Type, msg.Content, null,
                JsonSerializer.Deserialize<object>(msg.Metadata!),
                null, [], msg.CreatedAt);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify all Admin & Staff about new request
        var staffAndAdmins = await userRepository.FindAsync(
            u => u.Role == UserRole.Admin || u.Role == UserRole.Staff, cancellationToken);
        var staffAdminIds = staffAndAdmins.Select(u => u.Id).Where(id => id != userId);
        await notificationService.NotifyMultipleAsync(
            staffAdminIds,
            NotificationType.NewRequest,
            "Yêu cầu mới được tạo",
            $"Client đã tạo yêu cầu mới: \"{command.Title}\"",
            request.Id,
            "Request",
            cancellationToken);

        return new CreateRequestResponse(request.Id, request.Title, request.Status, firstQuestion);
    }
}
