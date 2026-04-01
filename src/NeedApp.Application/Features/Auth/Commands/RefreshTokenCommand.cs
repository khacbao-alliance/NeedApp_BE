using FluentValidation;
using MediatR;
using NeedApp.Application.DTOs.Auth;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    IJwtService jwtService,
    IUnitOfWork unitOfWork) : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existing = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken)
            ?? throw new UnauthorizedException("Invalid refresh token.");

        if (!existing.IsActive)
            throw new UnauthorizedException("Refresh token has expired or been revoked.");

        var user = await userRepository.GetByIdAsync(existing.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), existing.UserId);

        // Revoke old token
        existing.RevokedAt = DateTime.UtcNow;
        refreshTokenRepository.Update(existing);

        // Issue new tokens
        var accessToken = jwtService.GenerateAccessToken(user);
        var newRefreshTokenValue = jwtService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: newRefreshTokenValue,
            ExpiresAt: newRefreshToken.ExpiresAt,
            UserId: user.Id,
            Email: user.Email,
            Name: user.Name,
            Role: user.Role
        );
    }
}
