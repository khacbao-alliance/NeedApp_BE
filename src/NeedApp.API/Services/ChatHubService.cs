using Microsoft.AspNetCore.SignalR;
using NeedApp.API.Hubs;
using NeedApp.Application.DTOs.Message;
using NeedApp.Application.Interfaces;

namespace NeedApp.API.Services;

/// <summary>
/// Implementation of IChatHubService using SignalR.
/// Pushes real-time events to connected clients in request groups.
/// </summary>
public class ChatHubService(IHubContext<ChatHub> hubContext) : IChatHubService
{
    public async Task SendMessageToRequest(Guid requestId, MessageDto message)
    {
        await hubContext.Clients
            .Group(ChatHub.GetGroupName(requestId))
            .SendAsync("NewMessage", message);
    }

    public async Task SendRequestStatusChanged(Guid requestId, string newStatus)
    {
        await hubContext.Clients
            .Group(ChatHub.GetGroupName(requestId))
            .SendAsync("RequestStatusChanged", new { requestId, status = newStatus });
    }

    public async Task SendMessageDeleted(Guid requestId, Guid messageId)
    {
        await hubContext.Clients
            .Group(ChatHub.GetGroupName(requestId))
            .SendAsync("MessageDeleted", new { requestId, messageId });
    }

    public async Task SendTypingIndicator(Guid requestId, Guid userId, string? userName)
    {
        await hubContext.Clients
            .Group(ChatHub.GetGroupName(requestId))
            .SendAsync("UserTyping", new { requestId, userId, userName });
    }

    public async Task SendMessageRead(Guid requestId, Guid userId, DateTime lastReadAt)
    {
        await hubContext.Clients
            .Group(ChatHub.GetGroupName(requestId))
            .SendAsync("MessageRead", new { requestId, userId, lastReadAt });
    }

    public async Task SendMessageEdited(Guid requestId, MessageDto message)
    {
        await hubContext.Clients
            .Group(ChatHub.GetGroupName(requestId))
            .SendAsync("MessageEdited", message);
    }

    public async Task SendMessagePinned(Guid requestId, Guid messageId, bool isPinned)
    {
        await hubContext.Clients
            .Group(ChatHub.GetGroupName(requestId))
            .SendAsync("MessagePinned", new { requestId, messageId, isPinned });
    }
}
