using MediatR;
using NeedApp.Application.Common.Models;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Needs.Commands;

public record DeleteNeedCommand(Guid Id, Guid RequestingUserId) : IRequest<Result>;

public class DeleteNeedCommandHandler(INeedRepository needRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteNeedCommand, Result>
{
    public async Task<Result> Handle(DeleteNeedCommand request, CancellationToken cancellationToken)
    {
        var need = await needRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Need), request.Id);

        if (need.UserId != request.RequestingUserId)
            throw new UnauthorizedException();

        await needRepository.DeleteAsync(need, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
