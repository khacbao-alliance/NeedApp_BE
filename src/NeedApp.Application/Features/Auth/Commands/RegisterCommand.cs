using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using NeedApp.Application.DTOs.Auth;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Auth.Commands;

public record RegisterCommand(string Email, string Password, string? Name) : IRequest<AuthResponse>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Name).MaximumLength(100).When(x => x.Name is not null);
    }
}

public class RegisterCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    IEmailService emailService,
    IUnitOfWork unitOfWork,
    ILogger<RegisterCommandHandler> logger) : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new DomainException($"Email '{request.Email}' is already registered.");

        var user = new User
        {
            Email = request.Email,
            Name = request.Name,
            Role = UserRole.Client,
            PasswordHash = passwordHasher.Hash(request.Password),
            HasClient = false
        };

        await userRepository.AddAsync(user, cancellationToken);

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

        // Send welcome email (fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await emailService.SendWelcomeEmailAsync(user.Email, user.Name, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
            }
        }, CancellationToken.None);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshTokenValue,
            ExpiresAt: refreshToken.ExpiresAt,
            UserId: user.Id,
            Email: user.Email,
            Name: user.Name,
            Role: user.Role,
            HasClient: false,
            AvatarUrl: null
        );
    }
}
