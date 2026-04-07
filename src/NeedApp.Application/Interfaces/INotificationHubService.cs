using NeedApp.Application.DTOs.Notification;

namespace NeedApp.Application.Interfaces;

/// <summary>
/// Abstraction for pushing real-time notifications to connected clients via SignalR.
/// </summary>
public interface INotificationHubService
{
    /// <summary>
    /// Push a new notification to a specific user.
    /// </summary>
    Task SendNotificationToUser(Guid userId, NotificationDto notification);

    /// <summary>
    /// Push updated unread count to a specific user.
    /// </summary>
    Task SendUnreadCountToUser(Guid userId, int count);
}
