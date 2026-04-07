using NeedApp.Domain.Enums;

namespace NeedApp.Application.DTOs.Notification;

public record NotificationDto(
    Guid Id,
    NotificationType Type,
    string? Title,
    string? Content,
    Guid? ReferenceId,
    string? ReferenceType,
    bool IsRead,
    DateTime CreatedAt
);

public record UnreadCountDto(int Count);
