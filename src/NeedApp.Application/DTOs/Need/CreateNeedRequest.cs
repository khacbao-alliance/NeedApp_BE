namespace NeedApp.Application.DTOs.Need;

public record CreateNeedRequest(
    string Title,
    string Description,
    Guid? CategoryId,
    string? Location,
    decimal? Budget
);
