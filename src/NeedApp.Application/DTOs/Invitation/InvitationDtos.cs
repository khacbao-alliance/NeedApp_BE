using NeedApp.Domain.Enums;

namespace NeedApp.Application.DTOs.Invitation;

public record InvitationDto(
    Guid Id,
    string ClientName,
    string? InvitedByName,
    ClientRole Role,
    InvitationStatus Status,
    DateTime CreatedAt
);

public record PendingInvitationDto(
    Guid Id,
    InvitationClientInfo Client,
    InvitationUserInfo InvitedBy,
    ClientRole Role,
    DateTime CreatedAt
);

public record InvitationClientInfo(
    Guid Id,
    string Name,
    string? Description
);

public record InvitationUserInfo(
    string? Name,
    string? Email,
    string? AvatarUrl
);

public record RespondInvitationRequest(bool Accept);
