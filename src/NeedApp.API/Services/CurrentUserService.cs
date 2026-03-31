using System.Security.Claims;
using NeedApp.Application.Interfaces;

namespace NeedApp.API.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var sub = _user?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _user?.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email => _user?.FindFirstValue(ClaimTypes.Email)
        ?? _user?.FindFirstValue("email");

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated ?? false;
}
