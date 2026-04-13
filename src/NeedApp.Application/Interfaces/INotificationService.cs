using NeedApp.Domain.Enums;

namespace NeedApp.Application.Interfaces;

/// <summary>
/// Central service for creating notifications, pushing real-time via SignalR,
/// and sending email for critical event types.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Create a notification for a single user.
    /// Saves to DB, pushes via SignalR, and optionally sends email for critical types.
    /// </summary>
    Task NotifyAsync(
        Guid userId,
        NotificationType type,
        string title,
        string content,
        Guid? referenceId = null,
        string? referenceType = null,
        object? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create notifications for multiple users at once.
    /// </summary>
    Task NotifyMultipleAsync(
        IEnumerable<Guid> userIds,
        NotificationType type,
        string title,
        string content,
        Guid? referenceId = null,
        string? referenceType = null,
        object? metadata = null,
        CancellationToken cancellationToken = default);
}
