using FluentValidation;
using MediatR;
using NeedApp.Application.Common.Models;
using NeedApp.Application.DTOs.Need;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Interfaces;
using Mapster;

namespace NeedApp.Application.Features.Needs.Commands;

public record CreateNeedCommand(string Title, string Description, Guid UserId, Guid? CategoryId, string? Location, decimal? Budget) : IRequest<Result<NeedDto>>;

public class CreateNeedCommandValidator : AbstractValidator<CreateNeedCommand>
{
    public CreateNeedCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Budget).GreaterThan(0).When(x => x.Budget.HasValue);
    }
}

public class CreateNeedCommandHandler(INeedRepository needRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateNeedCommand, Result<NeedDto>>
{
    public async Task<Result<NeedDto>> Handle(CreateNeedCommand request, CancellationToken cancellationToken)
    {
        var need = Need.Create(request.Title, request.Description, request.UserId, request.CategoryId, request.Location, request.Budget);
        await needRepository.AddAsync(need, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new NeedDto(need.Id, need.Title, need.Description, need.Location, need.Budget,
            need.Status.ToString(), need.UserId, string.Empty, need.CategoryId, null, need.CreatedAt, need.UpdatedAt);

        return Result<NeedDto>.Success(dto);
    }
}
