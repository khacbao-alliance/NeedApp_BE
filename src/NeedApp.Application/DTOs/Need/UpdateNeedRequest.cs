namespace NeedApp.Application.DTOs.Need;

public record UpdateNeedRequest(
    string Title,
    string Description,
    string? Location,
    decimal? Budget
);
