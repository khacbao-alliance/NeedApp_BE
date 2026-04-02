using FluentValidation;
using MediatR;
using NeedApp.Application.DTOs.User;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Users.Commands;

public record CreateUserCommand(
    string Email,
    string Password,
    string? Name,
    UserRole Role
) : IRequest<UserDetailDto>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Name).MaximumLength(100).When(x => x.Name is not null);
        RuleFor(x => x.Role).IsInEnum();
    }
}

public class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateUserCommand, UserDetailDto>
{
    public async Task<UserDetailDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new DomainException($"Email '{request.Email}' is already registered.");

        var user = new User
        {
            Email = request.Email,
            Name = request.Name,
            Role = request.Role,
            PasswordHash = passwordHasher.Hash(request.Password)
        };

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserDetailDto(user.Id, user.Email, user.Name, user.Role, user.CreatedAt, user.UpdatedAt);
    }
}
