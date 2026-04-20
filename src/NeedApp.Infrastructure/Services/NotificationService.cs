using Microsoft.Extensions.Logging;
using NeedApp.Application.DTOs.Notification;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;
using System.Text.Json;

namespace NeedApp.Infrastructure.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    INotificationHubService notificationHubService,
    IEmailService emailService,
    IUserRepository userRepository,
    IEmailPreferenceRepository emailPreferenceRepository,
    IUnitOfWork unitOfWork,
    ILogger<NotificationService> logger) : INotificationService
{
    // Only these types trigger an email (subject to user preferences)
    private static readonly HashSet<NotificationType> EmailTypes =
    [
        NotificationType.StatusChange,
        NotificationType.MissingInfo,
        NotificationType.Assignment,
        NotificationType.Invitation,
        NotificationType.NewRequest
    ];

    public async Task NotifyAsync(
        Guid userId,
        NotificationType type,
        string title,
        string content,
        Guid? referenceId = null,
        string? referenceType = null,
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var metadataJson = metadata != null ? JsonSerializer.Serialize(metadata) : null;

        // 1. Save to DB
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Content = content,
            Metadata = metadataJson,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            IsRead = false
        };
        await notificationRepository.AddAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new NotificationDto(
            notification.Id, type, title, content, metadataJson,
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

        // 3. Send email — check user preferences first
        if (EmailTypes.Contains(type))
        {
            try
            {
                var user = await userRepository.GetByIdAsync(userId, cancellationToken);
                if (user != null && await IsEmailEnabledForType(userId, type, cancellationToken))
                {
                    // Use specialized template for Assignment
                    if (type == NotificationType.Assignment && referenceId.HasValue)
                    {
                        await emailService.SendRequestAssignedEmailAsync(
                            user.Email, user.Name, title, referenceId.Value, cancellationToken);
                    }
                    else
                    {
                        await emailService.SendNotificationEmailAsync(
                            user.Email, user.Name, title, content, cancellationToken);
                    }
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
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var distinctUserIds = userIds.Distinct().ToList();
        if (distinctUserIds.Count == 0) return;

        var metadataJson = metadata != null ? JsonSerializer.Serialize(metadata) : null;

        // 1. Batch insert all notifications in a single round-trip
        var notifications = distinctUserIds.Select(userId => new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Content = content,
            Metadata = metadataJson,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            IsRead = false
        }).ToList();

        await notificationRepository.AddRangeAsync(notifications, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 2. Push real-time SignalR updates — batch unread counts in one query
        var unreadCounts = await notificationRepository.GetUnreadCountsByUserIdsAsync(
            distinctUserIds, cancellationToken);

        foreach (var notification in notifications)
        {
            try
            {
                var dto = new NotificationDto(
                    notification.Id, type, title, content, metadataJson,
                    referenceId, referenceType, false, notification.CreatedAt);
                await notificationHubService.SendNotificationToUser(notification.UserId, dto);
                var unreadCount = unreadCounts.GetValueOrDefault(notification.UserId, 0);
                await notificationHubService.SendUnreadCountToUser(notification.UserId, unreadCount);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to push notification via SignalR to user {UserId}", notification.UserId);
            }
        }

        // 3. Send emails — check preferences, use specialized templates
        if (EmailTypes.Contains(type))
        {
            try
            {
                var users = await userRepository.FindAsync(u => distinctUserIds.Contains(u.Id), cancellationToken);
                var preferences = await emailPreferenceRepository.GetByUserIdsAsync(distinctUserIds, cancellationToken);
                var prefMap = preferences.ToDictionary(p => p.UserId);

                var emailTasks = users.Select(async user =>
                {
                    try
                    {
                        if (!IsEmailEnabledForType(prefMap.GetValueOrDefault(user.Id), type))
                            return;

                        if (type == NotificationType.Assignment && referenceId.HasValue)
                        {
                            await emailService.SendRequestAssignedEmailAsync(
                                user.Email, user.Name, title, referenceId.Value, cancellationToken);
                        }
                        else
                        {
                            await emailService.SendNotificationEmailAsync(user.Email, user.Name, title, content, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to send notification email to user {UserId}", user.Id);
                    }
                });
                await Task.WhenAll(emailTasks);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to load users for notification emails");
            }
        }
    }

    /// <summary>Check if a user has enabled email for a specific notification type (async version for single user).</summary>
    private async Task<bool> IsEmailEnabledForType(Guid userId, NotificationType type, CancellationToken cancellationToken)
    {
        var pref = await emailPreferenceRepository.GetByUserIdAsync(userId, cancellationToken);
        return IsEmailEnabledForType(pref, type);
    }

    /// <summary>Check preference entity (sync, used in batch). Null pref = all enabled (defaults).</summary>
    private static bool IsEmailEnabledForType(EmailPreference? pref, NotificationType type)
    {
        // No preference record means all defaults = all enabled
        if (pref == null) return true;

        return type switch
        {
            NotificationType.Assignment => pref.OnAssignment,
            NotificationType.StatusChange => pref.OnStatusChange,
            NotificationType.NewRequest => pref.OnNewRequest,
            NotificationType.MissingInfo => pref.OnStatusChange, // treat as status change
            NotificationType.Invitation => true, // always send invitations
            _ => true
        };
    }
}
