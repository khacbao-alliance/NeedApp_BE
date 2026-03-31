using MediatR;
using NeedApp.Application.DTOs.Category;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Categories.Queries;

public record GetCategoriesQuery : IRequest<IEnumerable<CategoryDto>>;

public class GetCategoriesQueryHandler(IRepository<Domain.Entities.Category> categoryRepository)
    : IRequestHandler<GetCategoriesQuery, IEnumerable<CategoryDto>>
{
    public async Task<IEnumerable<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        return categories.Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.IconUrl, c.IsActive, c.CreatedAt));
    }
}
