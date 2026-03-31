using FluentValidation;
using MediatR;
using NeedApp.Application.Common.Models;
using NeedApp.Application.DTOs.Auth;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Auth.Commands;

public record RegisterCommand(string FullName, string Email, string Password, string ConfirmPassword) : IRequest<Result<AuthResponse>>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
}

public class RegisterCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtService jwtService)
    : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (!await userRepository.IsEmailUniqueAsync(request.Email, cancellationToken))
            return Result<AuthResponse>.Failure("Email is already registered.");

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.FullName, request.Email, passwordHash);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var token = jwtService.GenerateToken(user);
        var response = new AuthResponse(
            token,
            "Bearer",
            DateTime.UtcNow.AddHours(24),
            new UserInfo(user.Id, user.FullName, user.Email, user.Role.ToString())
        );

        return Result<AuthResponse>.Success(response);
    }
}
