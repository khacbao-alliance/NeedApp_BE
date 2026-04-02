using FluentValidation;
using MediatR;
using NeedApp.Application.DTOs.Auth;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Auth.Commands;

public record GoogleLoginCommand(string IdToken) : IRequest<AuthResponse>;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty();
    }
}

public class GoogleLoginCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtService jwtService,
    IGoogleAuthService googleAuthService,
    IUnitOfWork unitOfWork) : IRequestHandler<GoogleLoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var payload = await googleAuthService.VerifyIdTokenAsync(request.IdToken);

        var user = await userRepository.GetByEmailAsync(payload.Email, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Email = payload.Email,
                Name = payload.Name,
                GoogleId = payload.Subject,
                Role = UserRole.Client,
                HasClient = false,
                AvatarUrl = payload.Picture
            };
            await userRepository.AddAsync(user, cancellationToken);
        }
        else if (user.GoogleId is null)
        {
            user.GoogleId = payload.Subject;
            if (user.AvatarUrl is null && payload.Picture is not null)
                user.AvatarUrl = payload.Picture;
            userRepository.Update(user);
        }

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
            Role: user.Role,
            HasClient: user.HasClient,
            AvatarUrl: user.AvatarUrl
        );
    }
}
