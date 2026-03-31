namespace NeedApp.Application.DTOs.User;

public record UpdateUserRequest(string FullName, string? PhoneNumber, string? AvatarUrl);
