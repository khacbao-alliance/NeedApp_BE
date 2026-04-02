using FluentValidation;
using MediatR;
using NeedApp.Application.DTOs.Client;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Clients.Commands;

public record CreateClientCommand(string Name, string? Description, string? ContactEmail, string? ContactPhone) : IRequest<ClientDto>;

public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => x.ContactEmail is not null);
    }
}

public class CreateClientCommandHandler(
    IClientRepository clientRepository,
    IClientUserRepository clientUserRepository,
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateClientCommand, ClientDto>
{
    public async Task<ClientDto> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        if (user.HasClient)
            throw new DomainException("You already have a client profile.");

        var client = new Client
        {
            Name = request.Name,
            Description = request.Description,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            CreatedBy = userId
        };

        await clientRepository.AddAsync(client, cancellationToken);

        var clientUser = new ClientUser
        {
            ClientId = client.Id,
            UserId = userId,
            Role = ClientRole.Owner,
            CreatedBy = userId
        };

        await clientUserRepository.AddAsync(clientUser, cancellationToken);

        user.HasClient = true;
        userRepository.Update(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClientDto(client.Id, client.Name, client.Description, client.ContactEmail, client.ContactPhone, client.CreatedAt);
    }
}
