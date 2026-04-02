using NeedApp.Domain.Enums;

namespace NeedApp.Application.DTOs.Client;

public record CreateClientRequest(
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone
);

public record UpdateClientRequest(
    string? Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone
);

public record ClientDto(
    Guid Id,
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone,
    DateTime CreatedAt
);

public record ClientMemberDto(
    Guid UserId,
    string? Name,
    string? Email,
    ClientRole Role,
    string? AvatarUrl,
    DateTime JoinedAt
);

public record AddMemberRequest(
    string Email,
    ClientRole Role = ClientRole.Member
);
