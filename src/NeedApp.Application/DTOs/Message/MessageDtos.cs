using NeedApp.Domain.Enums;

namespace NeedApp.Application.DTOs.Message;

public record MessageDto(
    Guid Id,
    MessageType Type,
    string? Content,
    MessageSenderDto? Sender,
    object? Metadata,
    Guid? ReplyToId,
    List<FileAttachmentDto> Files,
    DateTime CreatedAt,
    List<ReactionSummaryDto>? Reactions = null
);

public record ReactionSummaryDto(string Emoji, int Count, List<Guid> UserIds);

public record MessageSenderDto(
    Guid Id,
    string? Name,
    UserRole? Role,
    string? AvatarUrl
);

public record FileAttachmentDto(
    Guid Id,
    string FileName,
    string Url,
    string? ContentType,
    long? FileSize
);

public record SendMessageRequest(
    string Content,
    MessageType Type = MessageType.Text,
    Guid? ReplyToId = null
);

public record SendMissingInfoRequest(
    string Content,
    List<string> Questions
);

/// <summary>Read receipt: user X has read all messages up to LastReadAt.</summary>
public record ReadReceiptDto(Guid UserId, DateTime LastReadAt);

public record MessageListResponse(
    List<MessageDto> Items,
    string? NextCursor,
    bool HasMore,
    List<ReadReceiptDto>? Readers = null
);

