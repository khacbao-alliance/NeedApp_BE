using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NeedApp.Application.DTOs.Request;
using NeedApp.Application.Features.Requests.Commands;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;
using Xunit;

namespace NeedApp.Tests.Features.Requests.Commands;

public class AssignRequestCommandTests
{
    // ── Shared helpers ────────────────────────────────────────────

    private static (
        IRequestRepository requests,
        IUserRepository users,
        IRequestParticipantRepository participants,
        IMessageRepository messages,
        ICurrentUserService currentUser,
        IChatHubService chatHub,
        INotificationService notifications,
        IUnitOfWork unitOfWork,
        AssignRequestCommandHandler handler
    ) BuildSut(Guid actorId, UserRole actorRole)
    {
        var requests      = Substitute.For<IRequestRepository>();
        var users         = Substitute.For<IUserRepository>();
        var participants  = Substitute.For<IRequestParticipantRepository>();
        var messages      = Substitute.For<IMessageRepository>();
        var currentUser   = Substitute.For<ICurrentUserService>();
        var chatHub       = Substitute.For<IChatHubService>();
        var notifications = Substitute.For<INotificationService>();
        var unitOfWork    = Substitute.For<IUnitOfWork>();

        currentUser.UserId.Returns(actorId);
        currentUser.UserRole.Returns(actorRole);

        var handler = new AssignRequestCommandHandler(
            requests, users, participants, messages,
            currentUser, chatHub, notifications, unitOfWork);

        return (requests, users, participants, messages,
                currentUser, chatHub, notifications, unitOfWork, handler);
    }

    private static Request MakePendingRequest(Guid requestId, Guid clientId) => new()
    {
        Id         = requestId,
        Title      = "Test Request",
        ClientId   = clientId,
        Status     = RequestStatus.Pending,
        Priority   = RequestPriority.Medium,
        CreatedBy  = Guid.NewGuid(),
    };

    private static User MakeStaffUser(Guid staffId) => new()
    {
        Id   = staffId,
        Name = "Jane Staff",
        Role = UserRole.Staff,
    };

    private static Request MakeDetailedRequest(Guid requestId, Guid clientId, Guid staffId) => new()
    {
        Id           = requestId,
        Title        = "Test Request",
        Status       = RequestStatus.InProgress,
        Priority     = RequestPriority.Medium,
        AssignedTo   = staffId,
        Client       = new Client { Id = clientId, Name = "ACME" },
        AssignedUser = new User { Id = staffId, Name = "Jane Staff" },
        Participants = [],
    };

    // ── Test 1: Admin assigns staff successfully ──────────────────

    [Fact]
    public async Task Handle_AdminAssignsStaff_TransitionsToInProgress()
    {
        var adminId    = Guid.NewGuid();
        var staffId    = Guid.NewGuid();
        var requestId  = Guid.NewGuid();
        var clientId   = Guid.NewGuid();

        var (requests, users, participants, messages,
             _, chatHub, notifications, _, handler) = BuildSut(adminId, UserRole.Admin);

        var request     = MakePendingRequest(requestId, clientId);
        var staffUser   = MakeStaffUser(staffId);
        var detailed    = MakeDetailedRequest(requestId, clientId, staffId);

        requests.GetByIdAsync(requestId, default).Returns(request);
        users.GetByIdAsync(staffId, default).Returns(staffUser);
        participants.IsParticipantAsync(requestId, staffId, default).Returns(false);
        requests.GetWithDetailsAsync(requestId, default).Returns(detailed);
        messages.GetCountByRequestIdAsync(requestId, default).Returns(5);

        // Act
        var result = await handler.Handle(
            new AssignRequestCommand(requestId, staffId), default);

        // Assert: status auto-transitioned
        request.Status.Should().Be(RequestStatus.InProgress);

        // Assert: staff notified (admin assigning → NotifyAsync for staff)
        await notifications.Received(1).NotifyAsync(
            staffId,
            NotificationType.Assignment,
            Arg.Any<string>(),
            Arg.Any<string>(),
            requestId,
            "Request",
            Arg.Any<object>(),
            default);

        // Assert: DTO returned correctly
        result.Should().NotBeNull();
        result.Status.Should().Be(RequestStatus.InProgress);
    }

    // ── Test 2: Staff self-assigns → admins notified ─────────────

    [Fact]
    public async Task Handle_StaffSelfAssigns_NotifiesAdmins()
    {
        var staffId   = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var clientId  = Guid.NewGuid();
        var admin1    = new User { Id = Guid.NewGuid(), Role = UserRole.Admin };
        var admin2    = new User { Id = Guid.NewGuid(), Role = UserRole.Admin };

        var (requests, users, participants, messages,
             _, chatHub, notifications, _, handler) = BuildSut(staffId, UserRole.Staff);

        var request   = MakePendingRequest(requestId, clientId);
        var staffUser = MakeStaffUser(staffId);
        var detailed  = MakeDetailedRequest(requestId, clientId, staffId);

        requests.GetByIdAsync(requestId, default).Returns(request);
        users.GetByIdAsync(staffId, default).Returns(staffUser);
        users.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<User, bool>>>(), default)
             .Returns([admin1, admin2]);
        participants.IsParticipantAsync(requestId, staffId, default).Returns(false);
        requests.GetWithDetailsAsync(requestId, default).Returns(detailed);
        messages.GetCountByRequestIdAsync(requestId, default).Returns(0);

        await handler.Handle(new AssignRequestCommand(requestId, staffId), default);

        // Self-assign → NotifyMultipleAsync for admins, NOT NotifyAsync
        await notifications.Received(1).NotifyMultipleAsync(
            Arg.Any<IEnumerable<Guid>>(),
            NotificationType.Assignment,
            Arg.Any<string>(),
            Arg.Any<string>(),
            requestId,
            "Request",
            Arg.Any<object>(),
            default);

        await notifications.DidNotReceive().NotifyAsync(
            Arg.Any<Guid>(),
            NotificationType.Assignment,
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Guid?>(),
            Arg.Any<string?>(),
            Arg.Any<object?>(),
            default);
    }

    // ── Test 3: Cannot assign during Draft ────────────────────────

    [Fact]
    public async Task Handle_RequestInDraft_ThrowsDomainException()
    {
        var adminId   = Guid.NewGuid();
        var staffId   = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        var (requests, users, _, _, _, _, _, _, handler) = BuildSut(adminId, UserRole.Admin);

        var request = MakePendingRequest(requestId, Guid.NewGuid());
        request.Status = RequestStatus.Draft;

        requests.GetByIdAsync(requestId, default).Returns(request);
        users.GetByIdAsync(staffId, default).Returns(MakeStaffUser(staffId));

        Func<Task> act = () => handler.Handle(
            new AssignRequestCommand(requestId, staffId), default);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Draft or Intake*");
    }

    // ── Test 4: Staff cannot assign other staff ───────────────────

    [Fact]
    public async Task Handle_StaffAssignsOther_ThrowsDomainException()
    {
        var staffId      = Guid.NewGuid();
        var otherStaffId = Guid.NewGuid();
        var requestId    = Guid.NewGuid();

        var (requests, users, _, _, _, _, _, _, handler) = BuildSut(staffId, UserRole.Staff);

        var request   = MakePendingRequest(requestId, Guid.NewGuid());
        var otherUser = new User { Id = otherStaffId, Name = "Bob", Role = UserRole.Staff };

        requests.GetByIdAsync(requestId, default).Returns(request);
        users.GetByIdAsync(otherStaffId, default).Returns(otherUser);

        Func<Task> act = () => handler.Handle(
            new AssignRequestCommand(requestId, otherStaffId), default);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*self-assign*");
    }

    // ── Test 5: Target user not found ────────────────────────────

    [Fact]
    public async Task Handle_StaffUserNotFound_ThrowsNotFoundException()
    {
        var adminId   = Guid.NewGuid();
        var staffId   = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        var (requests, users, _, _, _, _, _, _, handler) = BuildSut(adminId, UserRole.Admin);

        requests.GetByIdAsync(requestId, default).Returns(MakePendingRequest(requestId, Guid.NewGuid()));
        users.GetByIdAsync(staffId, default).Returns((User?)null);

        Func<Task> act = () => handler.Handle(
            new AssignRequestCommand(requestId, staffId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
