using NeedApp.Domain.Enums;

namespace NeedApp.Application.DTOs.User;

public record UserDetailDto(
    Guid Id,
    string Email,
    string? Name,
    UserRole? Role,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
