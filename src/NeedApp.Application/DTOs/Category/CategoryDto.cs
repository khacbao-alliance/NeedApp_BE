namespace NeedApp.Application.DTOs.Category;

public record CategoryDto(Guid Id, string Name, string? Description, string? IconUrl, bool IsActive, DateTime CreatedAt);
