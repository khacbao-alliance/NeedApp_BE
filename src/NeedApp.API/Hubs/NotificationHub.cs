using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NeedApp.API.Hubs;

/// <summary>
/// SignalR Hub for real-time notification delivery.
/// Each user joins their own group "user_{userId}" on connect.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(userId.Value));
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(userId.Value));
        }
        await base.OnDisconnectedAsync(exception);
    }

    private Guid? GetUserId()
    {
        var value = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }

    public static string GetGroupName(Guid userId) => $"user_{userId}";
}
