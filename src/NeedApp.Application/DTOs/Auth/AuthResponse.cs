namespace NeedApp.Application.DTOs.Auth;

public record AuthResponse(string AccessToken, string TokenType, DateTime ExpiresAt, UserInfo User);
public record UserInfo(Guid Id, string FullName, string Email, string Role);
