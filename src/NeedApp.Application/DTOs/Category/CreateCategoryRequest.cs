namespace NeedApp.Application.DTOs.Category;

public record CreateCategoryRequest(string Name, string? Description, string? IconUrl);
