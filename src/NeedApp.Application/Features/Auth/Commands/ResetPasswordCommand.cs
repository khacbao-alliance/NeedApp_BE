using FluentValidation;
using MediatR;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Auth.Commands;

public record ResetPasswordCommand(string Email, string OtpCode, string NewPassword) : IRequest<Unit>;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.OtpCode).NotEmpty().Length(6).Matches(@"^\d{6}$").WithMessage("OTP must be a 6-digit number.");
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
    }
}

public class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IRequestHandler<ResetPasswordCommand, Unit>
{
    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new DomainException("Invalid OTP code or email.");

        var resetToken = await passwordResetTokenRepository.GetValidTokenAsync(
            request.OtpCode, user.Id, cancellationToken)
            ?? throw new DomainException("Invalid or expired OTP code.");

        // Mark token as used
        resetToken.IsUsed = true;
        passwordResetTokenRepository.Update(resetToken);

        // Update password
        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        userRepository.Update(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
