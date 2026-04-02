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
    DateTime CreatedAt
);

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

public record MessageListResponse(
    List<MessageDto> Items,
    string? NextCursor,
    bool HasMore
);
