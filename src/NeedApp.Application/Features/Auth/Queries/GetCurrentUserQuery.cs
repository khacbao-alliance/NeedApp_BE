using MediatR;
using NeedApp.Application.DTOs.User;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Auth.Queries;

public record GetCurrentUserQuery : IRequest<UserDto>;

public class GetCurrentUserQueryHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository) : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var user = await userRepository.GetByIdWithClientAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        UserClientDto? clientDto = null;
        var clientUser = user.ClientUsers.FirstOrDefault();
        if (clientUser is not null)
        {
            var client = clientUser.Client;
            clientDto = new UserClientDto(
                client.Id, client.Name, client.Description,
                client.ContactEmail, client.ContactPhone,
                clientUser.Role);
        }

        return new UserDto(user.Id, user.Email, user.Name, user.Role, user.HasClient, user.AvatarUrl, clientDto);
    }
}
