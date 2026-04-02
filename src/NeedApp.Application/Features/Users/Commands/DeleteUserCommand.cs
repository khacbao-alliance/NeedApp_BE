using MediatR;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Users.Commands;

public record DeleteUserCommand(Guid Id) : IRequest;

public class DeleteUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.Id);

        user.IsDeleted = true;
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
