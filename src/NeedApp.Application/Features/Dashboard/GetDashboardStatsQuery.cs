using MediatR;
using NeedApp.Application.DTOs.Dashboard;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Dashboard;

public record GetDashboardStatsQuery(int Days = 30) : IRequest<DashboardStatsDto>;

public class GetDashboardStatsQueryHandler(
    IRequestRepository requestRepository,
    IUserRepository userRepository,
    IClientRepository clientRepository)
    : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery query, CancellationToken cancellationToken)
    {
        // Get all requests (non-deleted, via global query filter)
        var allRequests = (await requestRepository.GetAllReadOnlyAsync(cancellationToken)).ToList();

        // Basic counts
        var total = allRequests.Count;
        var intakeCount = allRequests.Count(r => r.Status == RequestStatus.Intake);
        var pendingCount = allRequests.Count(r => r.Status == RequestStatus.Pending);
        var inProgressCount = allRequests.Count(r => r.Status == RequestStatus.InProgress);
        var doneCount = allRequests.Count(r => r.Status == RequestStatus.Done);
        var cancelledCount = allRequests.Count(r => r.Status == RequestStatus.Cancelled);
        var missingInfoCount = allRequests.Count(r => r.Status == RequestStatus.MissingInfo);
        var unassignedCount = allRequests.Count(r => !r.AssignedTo.HasValue
            && r.Status != RequestStatus.Done
            && r.Status != RequestStatus.Cancelled
            && r.Status != RequestStatus.Intake
            && r.Status != RequestStatus.Draft);
        var overdueCount = allRequests.Count(r => r.IsOverdue);

        // Total users & clients
        var allUsers = (await userRepository.GetAllReadOnlyAsync(cancellationToken)).ToList();
        var totalUsers = allUsers.Count;
        var totalClients = (await clientRepository.GetAllReadOnlyAsync(cancellationToken)).Count();

        // Average resolution time (hours): for requests that are Done
        var doneRequests = allRequests.Where(r => r.Status == RequestStatus.Done && r.UpdatedAt.HasValue).ToList();
        var avgResolutionHours = doneRequests.Count > 0
            ? doneRequests.Average(r => (r.UpdatedAt!.Value - r.CreatedAt).TotalHours)
            : 0.0;

        // SLA Compliance Rate: % of Done requests that were completed before DueDate
        var doneWithDueDate = doneRequests.Where(r => r.DueDate.HasValue).ToList();
        var slaComplianceRate = doneWithDueDate.Count > 0
            ? (double)doneWithDueDate.Count(r => r.UpdatedAt!.Value <= r.DueDate!.Value) / doneWithDueDate.Count * 100
            : 100.0; // No data = 100% compliance

        // Status breakdown
        var statusBreakdown = allRequests
            .GroupBy(r => r.Status)
            .Select(g => new StatusCountDto(g.Key.ToString(), g.Count()))
            .OrderByDescending(s => s.Count)
            .ToList();

        // Priority breakdown
        var priorityBreakdown = allRequests
            .Where(r => r.Status != RequestStatus.Done && r.Status != RequestStatus.Cancelled)
            .GroupBy(r => r.Priority)
            .Select(g => new PriorityCountDto(g.Key.ToString(), g.Count()))
            .OrderByDescending(p => p.Count)
            .ToList();

        // Daily trend (last N days)
        var cutoffDate = DateTime.UtcNow.AddDays(-query.Days).Date;
        var dailyTrend = Enumerable.Range(0, query.Days)
            .Select(i =>
            {
                var date = cutoffDate.AddDays(i);
                var nextDate = date.AddDays(1);
                var created = allRequests.Count(r => r.CreatedAt >= date && r.CreatedAt < nextDate);
                var completed = allRequests.Count(r =>
                    r.Status == RequestStatus.Done
                    && r.UpdatedAt.HasValue
                    && r.UpdatedAt.Value >= date
                    && r.UpdatedAt.Value < nextDate);
                return new DailyCountDto(date.ToString("yyyy-MM-dd"), created, completed);
            })
            .ToList();

        // Staff performance: group by assigned staff
        var staffPerformance = allRequests
            .Where(r => r.AssignedTo.HasValue)
            .GroupBy(r => r.AssignedTo!.Value)
            .Select(g =>
            {
                var staffUser = allUsers.FirstOrDefault(u => u.Id == g.Key);
                var assignedCount = g.Count();
                var completedCount = g.Count(r => r.Status == RequestStatus.Done);
                var staffDone = g.Where(r => r.Status == RequestStatus.Done && r.UpdatedAt.HasValue).ToList();
                var staffAvgHours = staffDone.Count > 0
                    ? staffDone.Average(r => (r.UpdatedAt!.Value - r.CreatedAt).TotalHours)
                    : 0.0;
                return new StaffPerformanceDto(
                    g.Key,
                    staffUser?.Name,
                    staffUser?.AvatarUrl,
                    assignedCount,
                    completedCount,
                    Math.Round(staffAvgHours, 1)
                );
            })
            .OrderByDescending(s => s.CompletedCount)
            .ThenBy(s => s.AvgResolutionHours)
            .ToList();

        return new DashboardStatsDto(
            total, intakeCount, pendingCount, inProgressCount,
            doneCount, cancelledCount, missingInfoCount,
            unassignedCount, overdueCount,
            totalUsers, totalClients,
            Math.Round(avgResolutionHours, 1),
            Math.Round(slaComplianceRate, 1),
            statusBreakdown, priorityBreakdown,
            dailyTrend, staffPerformance
        );
    }
}
