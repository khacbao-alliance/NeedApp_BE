namespace NeedApp.Application.DTOs.Dashboard;

public record DashboardStatsDto(
    int TotalRequests,
    int IntakeCount,
    int PendingCount,
    int InProgressCount,
    int DoneCount,
    int CancelledCount,
    int MissingInfoCount,
    int UnassignedCount,
    int OverdueCount,
    int TotalUsers,
    int TotalClients,
    double AvgResolutionHours,
    double SlaComplianceRate,
    IEnumerable<StatusCountDto> StatusBreakdown,
    IEnumerable<PriorityCountDto> PriorityBreakdown,
    IEnumerable<DailyCountDto> DailyTrend,
    IEnumerable<StaffPerformanceDto> StaffPerformance
);

public record StatusCountDto(string Status, int Count);
public record PriorityCountDto(string Priority, int Count);
public record DailyCountDto(string Date, int Created, int Completed);
public record StaffPerformanceDto(
    Guid UserId,
    string? Name,
    string? AvatarUrl,
    int AssignedCount,
    int CompletedCount,
    double AvgResolutionHours
);
