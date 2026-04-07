using Microsoft.AspNetCore.SignalR;
using NeedApp.API.Hubs;
using NeedApp.Application.DTOs.Notification;
using NeedApp.Application.Interfaces;

namespace NeedApp.API.Services;

/// <summary>
/// Implementation of INotificationHubService using SignalR.
/// </summary>
public class NotificationHubService(IHubContext<NotificationHub> hubContext) : INotificationHubService
{
    public async Task SendNotificationToUser(Guid userId, NotificationDto notification)
    {
        await hubContext.Clients
            .Group(NotificationHub.GetGroupName(userId))
            .SendAsync("NewNotification", notification);
    }

    public async Task SendUnreadCountToUser(Guid userId, int count)
    {
        await hubContext.Clients
            .Group(NotificationHub.GetGroupName(userId))
            .SendAsync("UnreadCountChanged", new { count });
    }
}
