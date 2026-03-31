namespace NeedApp.Application.DTOs.User;

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? AvatarUrl,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);
