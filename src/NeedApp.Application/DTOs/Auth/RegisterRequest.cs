using NeedApp.Domain.Enums;

namespace NeedApp.Application.DTOs.Auth;

public record RegisterRequest(string Email, string Password, string? Name);
