namespace NeedApp.Application.DTOs.Need;

public record NeedDto(
    Guid Id,
    string Title,
    string Description,
    string? Location,
    decimal? Budget,
    string Status,
    Guid UserId,
    string UserFullName,
    Guid? CategoryId,
    string? CategoryName,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
