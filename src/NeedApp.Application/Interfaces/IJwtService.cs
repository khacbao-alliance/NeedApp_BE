using NeedApp.Domain.Entities;

namespace NeedApp.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    string? ValidateToken(string token);
}
