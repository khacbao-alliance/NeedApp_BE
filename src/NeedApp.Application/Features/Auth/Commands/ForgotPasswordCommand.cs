using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Auth.Commands;

public record ForgotPasswordCommand(string Email, string RecaptchaToken) : IRequest<Unit>;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IEmailService emailService,
    IRecaptchaService recaptchaService,
    IUnitOfWork unitOfWork,
    ILogger<ForgotPasswordCommandHandler> logger) : IRequestHandler<ForgotPasswordCommand, Unit>
{
    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Email does not exist in the system — throw so the FE can show an error
        if (user is null)
        {
            logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
            throw new NotFoundException("Email này không tồn tại trong hệ thống.");
        }

        // Verify ReCAPTCHA
        var isRecaptchaValid = await recaptchaService.VerifyTokenAsync(request.RecaptchaToken, cancellationToken);
        if (!isRecaptchaValid)
        {
            throw new DomainException("Xác thực reCAPTCHA không thành công. Vui lòng thử lại.");
        }

        // Invalidate all existing tokens for this user

        await passwordResetTokenRepository.InvalidateAllUserTokensAsync(user.Id, cancellationToken);

        // Generate 6-digit OTP
        var otpCode = Random.Shared.Next(100000, 999999).ToString();

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            OtpCode = otpCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false
        };

        await passwordResetTokenRepository.AddAsync(resetToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Send email (fire-and-forget to not block response)
        _ = Task.Run(async () =>
        {
            try
            {
                await emailService.SendPasswordResetOtpAsync(user.Email, user.Name, otpCode, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
            }
        }, CancellationToken.None);

        return Unit.Value;
    }
}
