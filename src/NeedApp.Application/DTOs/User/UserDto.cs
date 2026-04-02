using NeedApp.Domain.Enums;

namespace NeedApp.Application.DTOs.User;

public record UserClientDto(
    Guid Id,
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone,
    ClientRole Role
);

public record UserDto(Guid Id, string Email, string? Name, UserRole? Role, bool HasClient, string? AvatarUrl, UserClientDto? Client = null);
