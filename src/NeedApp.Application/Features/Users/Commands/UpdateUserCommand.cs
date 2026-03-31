using FluentValidation;
using MediatR;
using NeedApp.Application.Common.Models;
using NeedApp.Application.DTOs.User;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Users.Commands;

public record UpdateUserCommand(Guid Id, string FullName, string? PhoneNumber, string? AvatarUrl) : IRequest<Result<UserDto>>;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).MaximumLength(20).When(x => x.PhoneNumber != null);
    }
}

public class UpdateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        user.Update(request.FullName, request.PhoneNumber, request.AvatarUrl);
        await userRepository.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new UserDto(user.Id, user.FullName, user.Email, user.PhoneNumber, user.AvatarUrl,
            user.Role.ToString(), user.IsActive, user.CreatedAt);

        return Result<UserDto>.Success(dto);
    }
}
