using MediatR;
using NeedApp.Application.DTOs.User;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Users.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<UserDetailDto>;

public class GetUserByIdQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUserByIdQuery, UserDetailDto>
{
    public async Task<UserDetailDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.Id);

        return new UserDetailDto(user.Id, user.Email, user.Name, user.Role, user.CreatedAt, user.UpdatedAt);
    }
}
