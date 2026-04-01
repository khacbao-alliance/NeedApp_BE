using NeedApp.Domain.Entities;

namespace NeedApp.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
