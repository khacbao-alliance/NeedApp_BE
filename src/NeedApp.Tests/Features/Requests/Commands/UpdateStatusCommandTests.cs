using FluentAssertions;
using NSubstitute;
using NeedApp.Application.Features.Requests.Commands;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;
using Xunit;
using NeedApp.Application.DTOs.Message;

namespace NeedApp.Tests.Features.Requests.Commands;

public class UpdateRequestStatusCommandTests
{
    private static (
        IRequestRepository requests,
        IMessageRepository messages,
        ICurrentUserService currentUser,
        INotificationService notifications,
        IUnitOfWork unitOfWork,
        UpdateRequestStatusCommandHandler handler
    ) BuildSut(Guid actorId)
    {
        var requests      = Substitute.For<IRequestRepository>();
        var messages      = Substitute.For<IMessageRepository>();
        var currentUser   = Substitute.For<ICurrentUserService>();
        var chatHub       = Substitute.For<IChatHubService>();
        var notifications = Substitute.For<INotificationService>();
        var unitOfWork    = Substitute.For<IUnitOfWork>();

        currentUser.UserId.Returns(actorId);

        var handler = new UpdateRequestStatusCommandHandler(
            requests, messages, currentUser, chatHub, notifications, unitOfWork);

        return (requests, messages, currentUser, notifications, unitOfWork, handler);
    }

    private static Request MakeRequest(Guid requestId, RequestStatus status, Guid? createdBy = null) => new()
    {
        Id          = requestId,
        Title       = "Test Request",
        Status      = status,
        Priority    = RequestPriority.Medium,
        CreatedBy   = createdBy ?? Guid.NewGuid(),
        Client      = new Client { Id = Guid.NewGuid(), Name = "ACME" },
        Participants = [],
    };

    // ── Test 1: Valid transition succeeds ─────────────────────────

    [Theory]
    [InlineData(RequestStatus.Pending,    RequestStatus.InProgress)]
    [InlineData(RequestStatus.InProgress, RequestStatus.Done)]
    [InlineData(RequestStatus.InProgress, RequestStatus.MissingInfo)]
    [InlineData(RequestStatus.MissingInfo, RequestStatus.InProgress)]
    [InlineData(RequestStatus.Done,       RequestStatus.InProgress)] // Re-open
    public async Task Handle_ValidTransition_UpdatesStatus(
        RequestStatus fromStatus, RequestStatus toStatus)
    {
        var actorId   = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var creatorId = Guid.NewGuid(); // Different from actor → will notify

        var (requests, messages, _, notifications, _, handler) = BuildSut(actorId);

        var request   = MakeRequest(requestId, fromStatus, creatorId);
        var detailed  = MakeRequest(requestId, toStatus, creatorId);
        detailed.Client = new Client { Id = Guid.NewGuid(), Name = "ACME" };

        requests.GetByIdAsync(requestId, default).Returns(request);
        requests.GetWithDetailsAsync(requestId, default).Returns(detailed);
        messages.GetCountByRequestIdAsync(requestId, default).Returns(0);

        var result = await handler.Handle(
            new UpdateRequestStatusCommand(requestId, toStatus), default);

        request.Status.Should().Be(toStatus);
        result.Should().NotBeNull();
    }

    // ── Test 2: Invalid transition throws ─────────────────────────

    [Theory]
    [InlineData(RequestStatus.Pending, RequestStatus.Done)]
    [InlineData(RequestStatus.Done,    RequestStatus.Pending)]
    public async Task Handle_InvalidTransition_ThrowsDomainException(
        RequestStatus fromStatus, RequestStatus toStatus)
    {
        var actorId   = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        var (requests, _, _, _, _, handler) = BuildSut(actorId);
        requests.GetByIdAsync(requestId, default).Returns(MakeRequest(requestId, fromStatus));

        Func<Task> act = () => handler.Handle(
            new UpdateRequestStatusCommand(requestId, toStatus), default);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Cannot transition*");
    }

    // ── Test 3: Creator gets notified ────────────────────────────

    [Fact]
    public async Task Handle_CreatorDifferentFromActor_SendsNotification()
    {
        var actorId   = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        var (requests, messages, _, notifications, _, handler) = BuildSut(actorId);

        var request  = MakeRequest(requestId, RequestStatus.Pending, creatorId);
        var detailed = MakeRequest(requestId, RequestStatus.InProgress, creatorId);

        requests.GetByIdAsync(requestId, default).Returns(request);
        requests.GetWithDetailsAsync(requestId, default).Returns(detailed);
        messages.GetCountByRequestIdAsync(requestId, default).Returns(0);

        await handler.Handle(
            new UpdateRequestStatusCommand(requestId, RequestStatus.InProgress), default);

        await notifications.Received(1).NotifyAsync(
            creatorId,
            NotificationType.StatusChange,
            Arg.Any<string>(),
            Arg.Any<string>(),
            requestId,
            "Request",
            Arg.Any<object>(),
            default);
    }

    // ── Test 4: Actor == Creator → no notification ────────────────

    [Fact]
    public async Task Handle_ActorIsCreator_DoesNotSendNotification()
    {
        var actorId   = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        var (requests, messages, _, notifications, _, handler) = BuildSut(actorId);

        // creator == actor
        var request  = MakeRequest(requestId, RequestStatus.Pending, actorId);
        var detailed = MakeRequest(requestId, RequestStatus.InProgress, actorId);

        requests.GetByIdAsync(requestId, default).Returns(request);
        requests.GetWithDetailsAsync(requestId, default).Returns(detailed);
        messages.GetCountByRequestIdAsync(requestId, default).Returns(0);

        await handler.Handle(
            new UpdateRequestStatusCommand(requestId, RequestStatus.InProgress), default);

        await notifications.DidNotReceive().NotifyAsync(
            Arg.Any<Guid>(),
            Arg.Any<NotificationType>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Guid?>(),
            Arg.Any<string?>(),
            Arg.Any<object?>(),
            default);
    }
}
