using FluentValidation;
using MediatR;
using NeedApp.Application.DTOs.User;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Users.Commands;

public record UpdateUserCommand(
    Guid Id,
    string? Name,
    UserRole? Role
) : IRequest<UserDetailDto>;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).MaximumLength(100).When(x => x.Name is not null);
        RuleFor(x => x.Role).IsInEnum().When(x => x.Role is not null);
    }
}

public class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserCommand, UserDetailDto>
{
    public async Task<UserDetailDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.Id);

        if (request.Name is not null)
            user.Name = request.Name;

        if (request.Role is not null)
            user.Role = request.Role;

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserDetailDto(user.Id, user.Email, user.Name, user.Role, user.HasClient, user.AvatarUrl, user.CreatedAt, user.UpdatedAt);
    }
}
