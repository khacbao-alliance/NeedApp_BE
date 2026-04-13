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
public class ChatHub(
    IRequestParticipantRepository participantRepository,
    IRequestRepository requestRepository,
    IClientUserRepository clientUserRepository) : Hub
{
    /// <summary>
    /// Client calls this to join a request's chat room.
    /// Access is allowed if the user is:
    ///   1. Admin or Staff (can view any request), OR
    ///   2. A Client user belonging to the same company as the request, OR
    ///   3. Already a participant of the request.
    /// </summary>
    public async Task JoinRequest(Guid requestId)
    {
        var userId = GetUserId();
        if (userId == null) return;

        var role = Context.User?.FindFirstValue(ClaimTypes.Role);

        // Admin/Staff can join any request room
        if (role is "Admin" or "Staff")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(requestId));
            await Clients.Caller.SendAsync("JoinedRequest", requestId);
            return;
        }

        // Check if user is already a participant
        var isParticipant = await participantRepository.IsParticipantAsync(requestId, userId.Value);
        if (isParticipant)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(requestId));
            await Clients.Caller.SendAsync("JoinedRequest", requestId);
            return;
        }

        // Client user: check if they belong to the same company as the request
        var request = await requestRepository.GetByIdAsync(requestId);
        if (request != null)
        {
            var clientUser = await clientUserRepository.GetByUserIdAsync(userId.Value);
            if (clientUser != null && clientUser.ClientId == request.ClientId)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(requestId));
                await Clients.Caller.SendAsync("JoinedRequest", requestId);
                return;
            }
        }

        await Clients.Caller.SendAsync("Error", "You are not authorized to join this request.");
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
