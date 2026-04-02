using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NeedApp.Domain.Interfaces;

namespace NeedApp.API.Hubs;

/// <summary>
/// SignalR Hub for real-time chat communication.
/// Clients join/leave request groups to receive messages in real-time.
/// </summary>
[Authorize]
public class ChatHub(IRequestParticipantRepository participantRepository) : Hub
{
    /// <summary>
    /// Client calls this to join a request's chat room.
    /// Only participants of the request can join.
    /// </summary>
    public async Task JoinRequest(Guid requestId)
    {
        var userId = GetUserId();
        if (userId == null) return;

        var isParticipant = await participantRepository.IsParticipantAsync(requestId, userId.Value);
        if (!isParticipant)
        {
            await Clients.Caller.SendAsync("Error", "You are not a participant of this request.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(requestId));
        await Clients.Caller.SendAsync("JoinedRequest", requestId);
    }

    /// <summary>
    /// Client calls this to leave a request's chat room.
    /// </summary>
    public async Task LeaveRequest(Guid requestId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(requestId));
        await Clients.Caller.SendAsync("LeftRequest", requestId);
    }

    /// <summary>
    /// Client sends typing indicator.
    /// </summary>
    public async Task SendTyping(Guid requestId)
    {
        var userId = GetUserId();
        var userName = Context.User?.FindFirstValue(ClaimTypes.Name);
        if (userId == null) return;

        await Clients.OthersInGroup(GetGroupName(requestId))
            .SendAsync("UserTyping", new { userId, userName, requestId });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    private Guid? GetUserId()
    {
        var value = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }

    public static string GetGroupName(Guid requestId) => $"request_{requestId}";
}
