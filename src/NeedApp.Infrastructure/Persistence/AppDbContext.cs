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
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();
    public DbSet<IntakeQuestionSet> IntakeQuestionSets => Set<IntakeQuestionSet>();
    public DbSet<IntakeQuestion> IntakeQuestions => Set<IntakeQuestion>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<MessageReaction> MessageReactions => Set<MessageReaction>();
    public DbSet<MessageReadReceipt> MessageReadReceipts => Set<MessageReadReceipt>();
    public DbSet<SlaConfig> SlaConfigs => Set<SlaConfig>();
    public DbSet<EmailPreference> EmailPreferences => Set<EmailPreference>();
    public DbSet<MessageEditHistory> MessageEditHistories => Set<MessageEditHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<UserRole>(schema: null, name: "user_role");
        modelBuilder.HasPostgresEnum<RequestStatus>(schema: null, name: "request_status");
        modelBuilder.HasPostgresEnum<RequestPriority>(schema: null, name: "request_priority");
        modelBuilder.HasPostgresEnum<MessageType>(schema: null, name: "message_type");
        modelBuilder.HasPostgresEnum<ClientRole>(schema: null, name: "client_role");
        modelBuilder.HasPostgresEnum<ParticipantRole>(schema: null, name: "participant_role");
        modelBuilder.HasPostgresEnum<NotificationType>(schema: null, name: "notification_type");
        modelBuilder.HasPostgresEnum<AuditAction>(schema: null, name: "audit_action");
        modelBuilder.HasPostgresEnum<InvitationStatus>(schema: null, name: "invitation_status");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
