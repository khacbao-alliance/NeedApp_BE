using FluentValidation;
using MediatR;
using NeedApp.Application.Common.Models;
using NeedApp.Application.DTOs.Category;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Categories.Commands;

public record CreateCategoryCommand(string Name, string? Description, string? IconUrl) : IRequest<Result<CategoryDto>>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateCategoryCommandHandler(IRepository<Category> categoryRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Category.Create(request.Name, request.Description, request.IconUrl);
        await categoryRepository.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new CategoryDto(category.Id, category.Name, category.Description, category.IconUrl, category.IsActive, category.CreatedAt);
        return Result<CategoryDto>.Success(dto);
    }
}
