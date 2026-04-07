using MediatR;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Notifications.Commands;

// ─── Mark Single Notification as Read ───────────────────────────────────────

public record MarkNotificationReadCommand(Guid NotificationId) : IRequest;

public class MarkNotificationReadCommandHandler(
    INotificationRepository notificationRepository,
    ICurrentUserService currentUserService) : IRequestHandler<MarkNotificationReadCommand>
{
    public async Task Handle(MarkNotificationReadCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var notification = await notificationRepository.GetByIdAsync(command.NotificationId, cancellationToken)
            ?? throw new NotFoundException("Notification", command.NotificationId);

        if (notification.UserId != userId)
            throw new UnauthorizedException("You can only mark your own notifications.");

        await notificationRepository.MarkAsReadAsync(command.NotificationId, cancellationToken);
    }
}

// ─── Mark All Notifications as Read ─────────────────────────────────────────

public record MarkAllNotificationsReadCommand : IRequest;

public class MarkAllNotificationsReadCommandHandler(
    INotificationRepository notificationRepository,
    ICurrentUserService currentUserService) : IRequestHandler<MarkAllNotificationsReadCommand>
{
    public async Task Handle(MarkAllNotificationsReadCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        await notificationRepository.MarkAllAsReadAsync(userId, cancellationToken);
    }
}
