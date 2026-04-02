using NeedApp.Domain.Enums;

namespace NeedApp.Application.DTOs.User;

public record UpdateUserRequest(
    string? Name,
    UserRole? Role
);
