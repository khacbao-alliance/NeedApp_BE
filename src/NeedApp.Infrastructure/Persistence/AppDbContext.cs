using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;

namespace NeedApp.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ClientUser> ClientUsers => Set<ClientUser>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<RequestParticipant> RequestParticipants => Set<RequestParticipant>();
    public DbSet<MissingInformation> MissingInformations => Set<MissingInformation>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<RequestFile> Files => Set<RequestFile>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<UserRole>(schema: null, name: "user_role");
        modelBuilder.HasPostgresEnum<RequestStatus>(schema: null, name: "request_status");
        modelBuilder.HasPostgresEnum<MissingInfoStatus>(schema: null, name: "missing_info_status");
        modelBuilder.HasPostgresEnum<CommentType>(schema: null, name: "comment_type");
        modelBuilder.HasPostgresEnum<NotificationType>(schema: null, name: "notification_type");
        modelBuilder.HasPostgresEnum<AuditAction>(schema: null, name: "audit_action");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
