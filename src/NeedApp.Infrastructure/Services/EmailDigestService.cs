using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Services;

/// <summary>
/// Background service that sends daily/weekly digest emails.
/// Checks every hour; sends Daily at 8 AM UTC, Weekly on Monday 8 AM UTC.
/// </summary>
public class EmailDigestService(
    IServiceScopeFactory scopeFactory,
    ILogger<EmailDigestService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EmailDigestService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;

                // Daily digest at 8 AM UTC (±30 min window)
                if (now.Hour == 8)
                {
                    await SendDigestsAsync(DigestFrequency.Daily, "hàng ngày", stoppingToken);
                }

                // Weekly digest on Monday at 8 AM UTC
                if (now.Hour == 8 && now.DayOfWeek == DayOfWeek.Monday)
                {
                    await SendDigestsAsync(DigestFrequency.Weekly, "hàng tuần", stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in EmailDigestService");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task SendDigestsAsync(DigestFrequency frequency, string periodLabel, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var emailPrefRepo = scope.ServiceProvider.GetRequiredService<IEmailPreferenceRepository>();
        var requestRepo = scope.ServiceProvider.GetRequiredService<IRequestRepository>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var prefs = await emailPrefRepo.GetByDigestFrequencyAsync(frequency, cancellationToken);

        if (prefs.Count == 0) return;

        logger.LogInformation("Sending {Frequency} digest to {Count} users", frequency, prefs.Count);

        foreach (var pref in prefs)
        {
            // Skip if already sent within this window (prevent duplicates if service restarts)
            if (pref.LastDigestSentAt.HasValue)
            {
                var hoursSinceLast = (DateTime.UtcNow - pref.LastDigestSentAt.Value).TotalHours;
                if (frequency == DigestFrequency.Daily && hoursSinceLast < 20) continue;
                if (frequency == DigestFrequency.Weekly && hoursSinceLast < 144) continue; // ~6 days
            }

            try
            {
                // Get requests assigned to this user (or belonging to their client)
                var (requests, _) = await requestRepo.GetPagedAsync(
                    page: 1,
                    pageSize: 50,
                    search: null,
                    status: null,
                    priority: null,
                    currentUserId: pref.UserId,
                    currentUserRole: pref.User.Role,
                    currentClientId: null,
                    assignedTo: pref.User.Role != UserRole.Client ? pref.UserId : null,
                    cancellationToken: cancellationToken);

                var requestList = requests.ToList();

                // Filter to active requests only
                var activeRequests = requestList
                    .Where(r => r.Status != RequestStatus.Done && r.Status != RequestStatus.Cancelled)
                    .ToList();

                if (activeRequests.Count == 0) continue;

                var items = activeRequests.Select(r => new DigestItem
                {
                    Title = r.Title,
                    Status = r.Status.ToString(),
                    Priority = r.Priority.ToString(),
                    IsOverdue = r.IsOverdue,
                    RequestId = r.Id
                }).ToList();

                await emailService.SendDigestEmailAsync(
                    pref.User.Email, pref.User.Name, periodLabel, items, cancellationToken);

                // Update timestamp
                pref.LastDigestSentAt = DateTime.UtcNow;
                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Digest ({Period}) sent to user {UserId} with {Count} items",
                    periodLabel, pref.UserId, items.Count);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send digest to user {UserId}", pref.UserId);
            }
        }
    }
}
