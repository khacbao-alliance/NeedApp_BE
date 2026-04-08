using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using NeedApp.Application.DTOs.Message;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Messages.Queries;

public record GetConversationSummaryQuery(Guid RequestId) : IRequest<ConversationSummaryDto>;

public class GetConversationSummaryQueryHandler(
    IRequestRepository requestRepository,
    IMessageRepository messageRepository,
    IClientUserRepository clientUserRepository,
    ICurrentUserService currentUserService,
    IGeminiService geminiService,
    IMemoryCache cache) : IRequestHandler<GetConversationSummaryQuery, ConversationSummaryDto>
{
    private static readonly TimeSpan AiSummaryCacheDuration = TimeSpan.FromMinutes(10);
    public async Task<ConversationSummaryDto> Handle(GetConversationSummaryQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");
        var userRole = currentUserService.UserRole;

        var request = await requestRepository.GetWithDetailsAsync(query.RequestId, cancellationToken)
            ?? throw new NotFoundException("Request", query.RequestId);

        // Client can only view summary of requests belonging to their company
        if (userRole == UserRole.Client)
        {
            var clientUser = await clientUserRepository.GetByUserIdAsync(userId, cancellationToken);
            if (clientUser == null || request.ClientId != clientUser.ClientId)
                throw new NotFoundException("Request", query.RequestId);
        }

        var messages = await messageRepository.GetAllByRequestIdAsync(query.RequestId, cancellationToken);

        // --- Build Overview ---
        var overview = BuildOverview(messages);

        // --- Build Intake Summary ---
        var intakeSummary = BuildIntakeSummary(messages);

        // --- Build Missing Info Requests ---
        var missingInfoRequests = BuildMissingInfoSummary(messages);

        // --- Build Conversation Highlights ---
        var conversationHighlights = BuildConversationHighlights(messages);

        // --- Build Attachments ---
        var attachments = BuildAttachments(messages);

        // --- AI Summary via Gemini (cached 10 minutes) ---
        string? aiSummary = null;
        if (messages.Count > 0)
        {
            var cacheKey = $"ai_summary_{query.RequestId}";
            if (!cache.TryGetValue(cacheKey, out aiSummary))
            {
                var transcript = BuildTranscript(messages);
                aiSummary = await geminiService.SummarizeConversationAsync(transcript, cancellationToken);
                cache.Set(cacheKey, aiSummary, AiSummaryCacheDuration);
            }
        }

        return new ConversationSummaryDto(
            request.Id,
            request.Title,
            request.Status,
            overview,
            intakeSummary,
            missingInfoRequests,
            conversationHighlights,
            attachments,
            aiSummary,
            DateTime.UtcNow
        );
    }

    private static ConversationOverviewDto BuildOverview(List<Domain.Entities.Message> messages)
    {
        var participants = messages
            .Where(m => m.SenderId.HasValue && m.Sender != null)
            .GroupBy(m => m.SenderId!.Value)
            .Select(g =>
            {
                var sender = g.First().Sender!;
                return new ParticipantSummaryDto(sender.Id, sender.Name, sender.Role, g.Count());
            })
            .ToList();

        return new ConversationOverviewDto(
            TotalMessages: messages.Count,
            TotalTextMessages: messages.Count(m => m.Type == MessageType.Text),
            TotalSystemMessages: messages.Count(m => m.Type == MessageType.System),
            TotalFilesSent: messages.Sum(m => m.Files.Count),
            Participants: participants,
            FirstMessageAt: messages.FirstOrDefault()?.CreatedAt,
            LastMessageAt: messages.LastOrDefault()?.CreatedAt
        );
    }

    private static IntakeSummaryDto? BuildIntakeSummary(List<Domain.Entities.Message> messages)
    {
        var intakeQuestions = messages.Where(m => m.Type == MessageType.IntakeQuestion).ToList();
        var intakeAnswers = messages.Where(m => m.Type == MessageType.IntakeAnswer).ToList();

        if (intakeQuestions.Count == 0) return null;

        var qaList = new List<IntakeQaDto>();
        for (var i = 0; i < intakeQuestions.Count; i++)
        {
            var questionContent = intakeQuestions[i].Content;
            var answerContent = i < intakeAnswers.Count ? intakeAnswers[i].Content : null;
            qaList.Add(new IntakeQaDto(questionContent ?? "", answerContent));
        }

        return new IntakeSummaryDto(
            TotalQuestions: intakeQuestions.Count,
            AnsweredQuestions: intakeAnswers.Count,
            QuestionsAndAnswers: qaList
        );
    }

    private static List<MissingInfoSummaryDto> BuildMissingInfoSummary(List<Domain.Entities.Message> messages)
    {
        var missingInfoMessages = messages.Where(m => m.Type == MessageType.MissingInfo).ToList();

        return missingInfoMessages.Select(m =>
        {
            var questions = new List<string>();
            if (m.Metadata != null)
            {
                try
                {
                    var root = m.Metadata.RootElement;
                    if (root.TryGetProperty("questions", out var qArray))
                    {
                        foreach (var q in qArray.EnumerateArray())
                        {
                            if (q.TryGetProperty("question", out var qText))
                                questions.Add(qText.GetString() ?? "");
                        }
                    }
                }
                catch { /* metadata parse failure - skip */ }
            }

            // Check if any subsequent messages exist that might be answers
            // Simple heuristic: if there are text messages from a client after this missing info message
            var missingInfoIdx = messages.IndexOf(m);
            var hasSubsequentClientMessages = messages
                .Skip(missingInfoIdx + 1)
                .Any(msg => msg.Type == MessageType.Text && msg.Sender?.Role == UserRole.Client);

            return new MissingInfoSummaryDto(
                RequestedBy: m.Sender?.Name,
                RequestedAt: m.CreatedAt,
                Content: m.Content,
                Questions: questions,
                IsResolved: hasSubsequentClientMessages
            );
        }).ToList();
    }

    private static List<ConversationHighlightDto> BuildConversationHighlights(List<Domain.Entities.Message> messages)
    {
        return messages
            .Where(m => m.Type == MessageType.Text && m.SenderId.HasValue && m.Sender != null)
            .GroupBy(m => m.SenderId!.Value)
            .Select(g =>
            {
                var sender = g.First().Sender!;
                // Take the 5 most recent text messages per sender
                var recentMessages = g
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(5)
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new MessageHighlightDto(m.Content, m.CreatedAt))
                    .ToList();

                return new ConversationHighlightDto(sender.Name, sender.Role, recentMessages);
            })
            .ToList();
    }

    private static List<AttachmentSummaryDto> BuildAttachments(List<Domain.Entities.Message> messages)
    {
        return messages
            .SelectMany(m => m.Files.Select(f => new { File = f, Message = m }))
            .Select(x => new AttachmentSummaryDto(
                x.File.Id,
                x.File.FileName,
                x.File.ContentType,
                x.File.FileSize,
                x.Message.Sender?.Name,
                x.Message.CreatedAt
            ))
            .ToList();
    }

    private static string BuildTranscript(List<Domain.Entities.Message> messages)
    {
        var lines = messages
            .Where(m => !string.IsNullOrEmpty(m.Content))
            .Select(m =>
            {
                var senderName = m.Sender?.Name ?? "System";
                var role = m.Sender?.Role?.ToString() ?? "System";
                var typeLabel = m.Type switch
                {
                    MessageType.IntakeQuestion => " [Câu hỏi khảo sát]",
                    MessageType.IntakeAnswer => " [Trả lời khảo sát]",
                    MessageType.MissingInfo => " [Yêu cầu bổ sung thông tin]",
                    MessageType.System => " [Hệ thống]",
                    MessageType.File => " [Gửi file]",
                    _ => ""
                };
                return $"[{m.CreatedAt:HH:mm dd/MM}] {senderName} ({role}){typeLabel}: {m.Content}";
            });

        return string.Join("\n", lines);
    }
}
