using NeedApp.Domain.Enums;

namespace NeedApp.Application.DTOs.User;

public record UserDto(Guid Id, string Email, string? Name, UserRole? Role);
