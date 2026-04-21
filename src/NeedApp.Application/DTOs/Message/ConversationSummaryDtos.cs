using NeedApp.Domain.Enums;

namespace NeedApp.Application.DTOs.Message;

public record ConversationSummaryDto(
    Guid RequestId,
    string RequestTitle,
    RequestStatus RequestStatus,
    ConversationOverviewDto Overview,
    IntakeSummaryDto? IntakeSummary,
    List<MissingInfoSummaryDto> MissingInfoRequests,
    List<AttachmentSummaryDto> Attachments,
    string? AiSummary,
    DateTime GeneratedAt
);

public record ConversationOverviewDto(
    int TotalMessages,
    int TotalTextMessages,
    int TotalSystemMessages,
    int TotalFilesSent,
    List<ParticipantSummaryDto> Participants,
    DateTime? FirstMessageAt,
    DateTime? LastMessageAt
);

public record ParticipantSummaryDto(
    Guid Id,
    string? Name,
    UserRole? Role,
    int MessageCount
);

public record IntakeSummaryDto(
    int TotalQuestions,
    int AnsweredQuestions,
    List<IntakeQaDto> QuestionsAndAnswers
);

public record IntakeQaDto(
    string Question,
    string? Answer
);

public record MissingInfoSummaryDto(
    string? RequestedBy,
    DateTime RequestedAt,
    string? Content,
    List<string> Questions,
    bool IsResolved
);

public record AttachmentSummaryDto(
    Guid Id,
    string FileName,
    string FileUrl,
    string? ContentType,
    long? FileSize,
    string? UploadedBy,
    DateTime UploadedAt
);
