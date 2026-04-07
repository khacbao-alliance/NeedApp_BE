using MediatR;
using NeedApp.Application.DTOs.Notification;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Notifications.Queries;

// ─── Get Notifications (paginated) ──────────────────────────────────────────

public record GetNotificationsQuery(int Page = 1, int PageSize = 20) : IRequest<IEnumerable<NotificationDto>>;

public class GetNotificationsQueryHandler(
    INotificationRepository notificationRepository,
    ICurrentUserService currentUserService) : IRequestHandler<GetNotificationsQuery, IEnumerable<NotificationDto>>
{
    public async Task<IEnumerable<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var notifications = await notificationRepository.GetByUserIdAsync(userId, request.Page, request.PageSize, cancellationToken);

        return notifications.Select(n => new NotificationDto(
            n.Id, n.Type, n.Title, n.Content,
            n.ReferenceId, n.ReferenceType, n.IsRead, n.CreatedAt
        ));
    }
}

// ─── Get Unread Count ───────────────────────────────────────────────────────

public record GetUnreadCountQuery : IRequest<UnreadCountDto>;

public class GetUnreadCountQueryHandler(
    INotificationRepository notificationRepository,
    ICurrentUserService currentUserService) : IRequestHandler<GetUnreadCountQuery, UnreadCountDto>
{
    public async Task<UnreadCountDto> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var count = await notificationRepository.GetUnreadCountAsync(userId, cancellationToken);
        return new UnreadCountDto(count);
    }
}
