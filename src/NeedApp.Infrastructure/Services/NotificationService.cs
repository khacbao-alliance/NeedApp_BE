using Microsoft.Extensions.Logging;
using NeedApp.Application.DTOs.Notification;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    INotificationHubService notificationHubService,
    IEmailService emailService,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<NotificationService> logger) : INotificationService
{
    // Only these types trigger an email
    private static readonly HashSet<NotificationType> EmailTypes =
    [
        NotificationType.StatusChange,
        NotificationType.MissingInfo,
        NotificationType.Assignment,
        NotificationType.Invitation
    ];

    public async Task NotifyAsync(
        Guid userId,
        NotificationType type,
        string title,
        string content,
        Guid? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Save to DB
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Content = content,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            IsRead = false
        };
        await notificationRepository.AddAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new NotificationDto(
            notification.Id, type, title, content,
            referenceId, referenceType, false, notification.CreatedAt);

        // 2. Push via SignalR (real-time)
        try
        {
            await notificationHubService.SendNotificationToUser(userId, dto);
            var unreadCount = await notificationRepository.GetUnreadCountAsync(userId, cancellationToken);
            await notificationHubService.SendUnreadCountToUser(userId, unreadCount);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to push notification via SignalR to user {UserId}", userId);
        }

        // 3. Send email for critical types
        if (EmailTypes.Contains(type))
        {
            try
            {
                var user = await userRepository.GetByIdAsync(userId, cancellationToken);
                if (user != null)
                {
                    await emailService.SendNotificationEmailAsync(
                        user.Email, user.Name, title, content, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send notification email to user {UserId}", userId);
            }
        }
    }

    public async Task NotifyMultipleAsync(
        IEnumerable<Guid> userIds,
        NotificationType type,
        string title,
        string content,
        Guid? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var userId in userIds.Distinct())
        {
            await NotifyAsync(userId, type, title, content, referenceId, referenceType, cancellationToken);
        }
    }
}
