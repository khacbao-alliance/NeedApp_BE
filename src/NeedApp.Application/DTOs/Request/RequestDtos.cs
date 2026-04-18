using NeedApp.Domain.Enums;
using NeedApp.Application.DTOs.Message;

namespace NeedApp.Application.DTOs.Request;

public record CreateRequestRequest(
    string Title,
    string? Description = null,
    RequestPriority Priority = RequestPriority.Medium,
    DateTime? DueDate = null
);

public record RequestDto(
    Guid Id,
    string Title,
    string? Description,
    RequestStatus Status,
    RequestPriority Priority,
    RequestClientDto? Client,
    RequestUserDto? AssignedUser,
    RequestUserDto? CreatedByUser,
    int MessageCount,
    bool IsClientActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DueDate,
    bool IsOverdue
);

public record CreateRequestResponse(
    Guid RequestId,
    string Title,
    RequestStatus Status,
    MessageDto? FirstQuestion
);

public record RequestClientDto(Guid Id, string Name);
public record RequestUserDto(Guid Id, string? Name, string? AvatarUrl);

public record UpdateStatusRequest(RequestStatus Status);
public record AssignRequestRequest(Guid StaffUserId);
