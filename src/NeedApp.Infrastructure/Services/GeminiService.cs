using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeedApp.Application.Interfaces;
using NeedApp.Infrastructure.Settings;

namespace NeedApp.Infrastructure.Services;

public class GeminiService(
    IOptions<GeminiSettings> options,
    ILogger<GeminiService> logger) : IGeminiService
{
    private readonly GeminiSettings _settings = options.Value;

    private const string SystemPrompt = """
        You are an assistant that summarizes customer service conversations.
        Given a conversation transcript, provide a concise, structured summary in Vietnamese.

        Your summary should include:
        1. Tổng quan: A brief 2-3 sentence overview of what the conversation is about.
        2. Yêu cầu chính: The main requirements or issues raised by the client.
        3. Thông tin đã thu thập: Key information gathered during the conversation.
        4. Trạng thái hiện tại: Current status and any pending actions.
        5. Ghi chú quan trọng: Any important notes or follow-ups needed.

        Keep the summary factual, professional, and under 500 words.
        Do NOT use markdown headers — use plain text with line breaks between sections.
        """;

    public async Task<string?> SummarizeConversationAsync(string conversationText, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = new Client(apiKey: _settings.ApiKey);

            var config = new GenerateContentConfig
            {
                SystemInstruction = new Content { Parts = [new Part { Text = SystemPrompt }] },
                Temperature = 0.3f,
                MaxOutputTokens = 1024
            };

            var response = await client.Models.GenerateContentAsync(
                model: _settings.Model,
                contents: new Content
                {
                    Parts = [new Part { Text = $"Hãy tóm tắt cuộc hội thoại sau:\n\n{conversationText}" }],
                    Role = "user"
                },
                config: config
            );

            return response.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to call Gemini API for conversation summarization");
            return null;
        }
    }
}
