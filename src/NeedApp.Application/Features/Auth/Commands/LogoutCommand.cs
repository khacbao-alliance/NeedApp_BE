using FluentValidation;
using MediatR;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Auth.Commands;

public record LogoutCommand(string RefreshToken) : IRequest;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken)
            ?? throw new NotFoundException("RefreshToken", request.RefreshToken);

        if (token.IsRevoked) return;

        token.RevokedAt = DateTime.UtcNow;
        refreshTokenRepository.Update(token);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
