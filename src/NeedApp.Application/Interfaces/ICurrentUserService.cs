using NeedApp.Domain.Enums;

namespace NeedApp.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    UserRole? UserRole { get; }
}
