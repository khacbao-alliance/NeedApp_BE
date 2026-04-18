using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Services;

/// <summary>
/// Background service that checks for overdue requests every hour
/// and sends email alerts to assigned staff (max 1 per request per day).
/// </summary>
public class OverdueAlertService(
    IServiceScopeFactory scopeFactory,
    ILogger<OverdueAlertService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OverdueAlertService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckOverdueRequestsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in OverdueAlertService");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task CheckOverdueRequestsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var requestRepo = scope.ServiceProvider.GetRequiredService<IRequestRepository>();
        var emailPrefRepo = scope.ServiceProvider.GetRequiredService<IEmailPreferenceRepository>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

        // Get all overdue requests (using the existing filter capability)
        var (overdueRequests, _) = await requestRepo.GetPagedAsync(
            page: 1,
            pageSize: 200,
            search: null,
            status: null,
            priority: null,
            currentUserId: null,
            currentUserRole: UserRole.Admin,  // admin sees all
            currentClientId: null,
            isOverdue: true,
            cancellationToken: cancellationToken);

        var requests = overdueRequests.ToList();
        if (requests.Count == 0) return;

        logger.LogInformation("Found {Count} overdue requests for alert check", requests.Count);

        // Check which users have overdue email enabled
        var assignedUserIds = requests
            .Where(r => r.AssignedTo.HasValue)
            .Select(r => r.AssignedTo!.Value)
            .Distinct()
            .ToList();

        var prefs = await emailPrefRepo.GetByUserIdsAsync(assignedUserIds, cancellationToken);
        var prefMap = prefs.ToDictionary(p => p.UserId);

        // Get existing overdue notifications to avoid sending duplicates (max 1/request/day)
        foreach (var request in requests)
        {
            if (!request.AssignedTo.HasValue || !request.DueDate.HasValue) continue;

            var userId = request.AssignedTo.Value;

            // Check preference
            var pref = prefMap.GetValueOrDefault(userId);
            if (pref != null && !pref.OnOverdue) continue;

            // Check if we already sent an alert for this request today
            var existingNotifications = await notificationRepo.GetByUserAndReferenceAsync(
                userId, request.Id, "OverdueAlert", cancellationToken);
            if (existingNotifications != null && existingNotifications.CreatedAt.Date == DateTime.UtcNow.Date)
                continue;

            try
            {
                var user = await userRepo.GetByIdAsync(userId, cancellationToken);
                if (user == null) continue;

                await emailService.SendOverdueAlertEmailAsync(
                    user.Email, user.Name, request.Title, request.DueDate.Value, request.Id, cancellationToken);

                // Save a notification record to track that we sent the alert
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var notification = new Domain.Entities.Notification
                {
                    UserId = userId,
                    Type = NotificationType.StatusChange,
                    Title = $"⚠️ Yêu cầu quá hạn: {request.Title}",
                    Content = $"Yêu cầu \"{request.Title}\" đã quá hạn deadline {request.DueDate.Value:dd/MM/yyyy HH:mm}",
                    ReferenceId = request.Id,
                    ReferenceType = "OverdueAlert",
                    IsRead = false
                };
                await notificationRepo.AddAsync(notification, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Overdue alert sent for request {RequestId} to user {UserId}", request.Id, userId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send overdue alert for request {RequestId}", request.Id);
            }
        }
    }
}
