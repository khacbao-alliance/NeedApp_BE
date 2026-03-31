using FluentValidation;
using MediatR;
using NeedApp.Application.Common.Models;
using NeedApp.Application.DTOs.Need;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Needs.Commands;

public record UpdateNeedCommand(Guid Id, string Title, string Description, string? Location, decimal? Budget, Guid RequestingUserId) : IRequest<Result<NeedDto>>;

public class UpdateNeedCommandValidator : AbstractValidator<UpdateNeedCommand>
{
    public UpdateNeedCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Budget).GreaterThan(0).When(x => x.Budget.HasValue);
    }
}

public class UpdateNeedCommandHandler(INeedRepository needRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateNeedCommand, Result<NeedDto>>
{
    public async Task<Result<NeedDto>> Handle(UpdateNeedCommand request, CancellationToken cancellationToken)
    {
        var need = await needRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Need), request.Id);

        if (need.UserId != request.RequestingUserId)
            throw new UnauthorizedException();

        need.Update(request.Title, request.Description, request.Location, request.Budget);
        await needRepository.UpdateAsync(need, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new NeedDto(need.Id, need.Title, need.Description, need.Location, need.Budget,
            need.Status.ToString(), need.UserId, string.Empty, need.CategoryId, null, need.CreatedAt, need.UpdatedAt);

        return Result<NeedDto>.Success(dto);
    }
}
