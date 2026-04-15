using NeedApp.Application.DTOs.Message;

namespace NeedApp.Application.Interfaces;

/// <summary>
/// Abstraction for pushing real-time messages to connected clients via SignalR.
/// Only used for chat after intake is complete (staff ↔ client communication).
/// </summary>
public interface IChatHubService
{
    /// <summary>
    /// Send a new message to all participants of a request.
    /// </summary>
    Task SendMessageToRequest(Guid requestId, MessageDto message);

    /// <summary>
    /// Notify participants that request status has changed.
    /// </summary>
    Task SendRequestStatusChanged(Guid requestId, string newStatus);

    /// <summary>
    /// Notify participants that a message was deleted.
    /// </summary>
    Task SendMessageDeleted(Guid requestId, Guid messageId);

    /// <summary>
    /// Notify that someone is typing in a request thread.
    /// </summary>
    Task SendTypingIndicator(Guid requestId, Guid userId, string? userName);

    /// <summary>
    /// Notify all participants that a user has read up to a given timestamp.
    /// </summary>
    Task SendMessageRead(Guid requestId, Guid userId, DateTime lastReadAt);
}
