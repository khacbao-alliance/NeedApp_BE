using NeedApp.Domain.Enums;

namespace NeedApp.Application.DTOs.SlaConfig;

public record SlaConfigDto(
    Guid Id,
    string Priority,
    double DeadlineHours,
    string? Description
);

public class UpdateSlaConfigsRequest
{
    public List<SlaConfigItemRequest> Configs { get; set; } = [];
}

public class SlaConfigItemRequest
{
    public RequestPriority Priority { get; set; }
    public double DeadlineHours { get; set; }
    public string? Description { get; set; }
}
