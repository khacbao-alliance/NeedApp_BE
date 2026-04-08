using FluentValidation;
using MediatR;
using NeedApp.Application.DTOs.Client;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Clients.Commands;

public record UpdateClientCommand(
    Guid ClientId,
    string? Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone) : IRequest<ClientDto>;

public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().MaximumLength(255)
            .When(x => x.Name is not null);
        RuleFor(x => x.ContactEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
    }
}

public class UpdateClientCommandHandler(
    IClientRepository clientRepository,
    IClientUserRepository clientUserRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateClientCommand, ClientDto>
{
    public async Task<ClientDto> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var client = await clientRepository.GetByIdAsync(request.ClientId, cancellationToken)
            ?? throw new NotFoundException("Client", request.ClientId);

        // Check that the current user is the Owner of this client
        var membership = await clientUserRepository.GetByUserAndClientIdAsync(userId, request.ClientId, cancellationToken)
            ?? throw new UnauthorizedException("You are not a member of this client.");

        if (membership.Role != Domain.Enums.ClientRole.Owner)
            throw new UnauthorizedException("Only the Owner can update client information.");

        if (request.Name is not null) client.Name = request.Name.Trim();
        if (request.Description is not null) client.Description = request.Description.Trim();
        if (request.ContactEmail is not null) client.ContactEmail = request.ContactEmail.Trim();
        if (request.ContactPhone is not null) client.ContactPhone = request.ContactPhone.Trim();

        clientRepository.Update(client);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClientDto(client.Id, client.Name, client.Description, client.ContactEmail, client.ContactPhone, client.CreatedAt);
    }
}
