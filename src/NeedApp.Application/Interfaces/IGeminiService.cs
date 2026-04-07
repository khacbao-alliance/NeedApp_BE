namespace NeedApp.Application.Interfaces;

public interface IGeminiService
{
    Task<string?> SummarizeConversationAsync(string conversationText, CancellationToken cancellationToken = default);
}
