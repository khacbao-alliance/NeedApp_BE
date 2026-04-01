using NeedApp.Domain.Enums;

namespace NeedApp.Application.DTOs.Auth;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    Guid UserId,
    string Email,
    string? Name,
    UserRole? Role
);
