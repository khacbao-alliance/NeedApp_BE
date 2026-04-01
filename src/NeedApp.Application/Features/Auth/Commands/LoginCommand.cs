using FluentValidation;
using MediatR;
using NeedApp.Application.DTOs.Auth;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    IUnitOfWork unitOfWork) : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (user.PasswordHash is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshTokenValue = jwtService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshTokenValue,
            ExpiresAt: refreshToken.ExpiresAt,
            UserId: user.Id,
            Email: user.Email,
            Name: user.Name,
            Role: user.Role
        );
    }
}
