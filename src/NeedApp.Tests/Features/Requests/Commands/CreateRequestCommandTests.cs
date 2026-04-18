using FluentAssertions;
using NSubstitute;
using NeedApp.Application.Features.Requests.Commands;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;
using Xunit;

namespace NeedApp.Tests.Features.Requests.Commands;

public class CreateRequestCommandTests
{
    private static (
        IRequestRepository requests,
        IClientUserRepository clientUsers,
        IMessageRepository messages,
        IRequestParticipantRepository participants,
        IIntakeQuestionSetRepository intakeRepo,
        ICurrentUserService currentUser,
        IUserRepository users,
        INotificationService notifications,
        ISlaConfigRepository slaConfigs,
        IUnitOfWork unitOfWork,
        CreateRequestCommandHandler handler
    ) BuildSut(Guid actorId)
    {
        var requests     = Substitute.For<IRequestRepository>();
        var clientUsers  = Substitute.For<IClientUserRepository>();
        var messages     = Substitute.For<IMessageRepository>();
        var participants = Substitute.For<IRequestParticipantRepository>();
        var intakeRepo   = Substitute.For<IIntakeQuestionSetRepository>();
        var currentUser  = Substitute.For<ICurrentUserService>();
        var users        = Substitute.For<IUserRepository>();
        var notifs       = Substitute.For<INotificationService>();
        var slaConfigs   = Substitute.For<ISlaConfigRepository>();
        var unitOfWork   = Substitute.For<IUnitOfWork>();

        currentUser.UserId.Returns(actorId);

        var handler = new CreateRequestCommandHandler(
            requests, clientUsers, messages, participants,
            intakeRepo, currentUser, users, notifs, slaConfigs, unitOfWork);

        return (requests, clientUsers, messages, participants, intakeRepo,
                currentUser, users, notifs, slaConfigs, unitOfWork, handler);
    }

    // ── Test 1: No intake → status Pending ───────────────────────

    [Fact]
    public async Task Handle_NoIntakeQuestions_CreatesPendingRequest()
    {
        var userId   = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        var (requests, clientUsers, _, _, intakeRepo,
             _, users, notifications, _, _, handler) = BuildSut(userId);

        clientUsers.GetByUserIdAsync(userId, default)
            .Returns(new ClientUser { UserId = userId, ClientId = clientId });
        intakeRepo.GetDefaultAsync(default).Returns((IntakeQuestionSet?)null);
        users.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<User, bool>>>(), default)
             .Returns([]);

        var result = await handler.Handle(
            new CreateRequestCommand("My Request", "Description"), default);

        result.Should().NotBeNull();
        result.Status.Should().Be(RequestStatus.Pending);
        result.FirstQuestion.Should().BeNull();
    }

    // ── Test 2: With intake → status Intake, returns first question ─

    [Fact]
    public async Task Handle_WithIntakeQuestions_CreatesIntakeRequest()
    {
        var userId   = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        var (requests, clientUsers, _, _, intakeRepo,
             _, users, _, _, _, handler) = BuildSut(userId);

        var questionSet = new IntakeQuestionSet
        {
            Id = Guid.NewGuid(),
            Questions =
            [
                new IntakeQuestion { Id = Guid.NewGuid(), Content = "Q1?", OrderIndex = 0, IsRequired = true },
                new IntakeQuestion { Id = Guid.NewGuid(), Content = "Q2?", OrderIndex = 1, IsRequired = false },
            ]
        };

        clientUsers.GetByUserIdAsync(userId, default)
            .Returns(new ClientUser { UserId = userId, ClientId = clientId });
        intakeRepo.GetDefaultAsync(default).Returns(questionSet);
        users.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<User, bool>>>(), default)
             .Returns([]);

        var result = await handler.Handle(
            new CreateRequestCommand("My Request", null), default);

        result.Status.Should().Be(RequestStatus.Intake);
        result.FirstQuestion.Should().NotBeNull();
        result.FirstQuestion!.Content.Should().Be("Q1?");
    }

    // ── Test 3: No client → throws DomainException ────────────────

    [Fact]
    public async Task Handle_NoClientProfile_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        var (_, clientUsers, _, _, _, _, _, _, _, _, handler) = BuildSut(userId);

        clientUsers.GetByUserIdAsync(userId, default).Returns((ClientUser?)null);

        Func<Task> act = () => handler.Handle(
            new CreateRequestCommand("My Request", null), default);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*client profile*");
    }

    // ── Test 4: Staff & Admin get notified ────────────────────────

    [Fact]
    public async Task Handle_ValidRequest_NotifiesStaffAndAdmins()
    {
        var userId   = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var staff1   = new User { Id = Guid.NewGuid(), Role = UserRole.Staff };
        var admin1   = new User { Id = Guid.NewGuid(), Role = UserRole.Admin };

        var (_, clientUsers, _, _, intakeRepo,
             _, users, notifications, _, _, handler) = BuildSut(userId);

        clientUsers.GetByUserIdAsync(userId, default)
            .Returns(new ClientUser { UserId = userId, ClientId = clientId });
        intakeRepo.GetDefaultAsync(default).Returns((IntakeQuestionSet?)null);
        users.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<User, bool>>>(), default)
             .Returns([staff1, admin1]);

        await handler.Handle(
            new CreateRequestCommand("My Request", null), default);

        await notifications.Received(1).NotifyMultipleAsync(
            Arg.Is<IEnumerable<Guid>>(ids => ids.Count() == 2),
            NotificationType.NewRequest,
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Guid?>(),
            "Request",
            Arg.Any<object>(),
            default);
    }
}
