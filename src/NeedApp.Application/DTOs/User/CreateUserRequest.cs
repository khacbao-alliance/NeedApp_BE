using NeedApp.Domain.Enums;

namespace NeedApp.Application.DTOs.User;

public record CreateUserRequest(
    string Email,
    string Password,
    string? Name,
    UserRole Role
);
