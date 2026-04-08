using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NeedApp.Domain.Enums;
using Npgsql;
using Npgsql.NameTranslation;

namespace NeedApp.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var nameTranslator = new NpgsqlSnakeCaseNameTranslator();
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(
            "Host=localhost;Database=needapp_design;Username=postgres;Password=postgres");

        dataSourceBuilder.MapEnum<UserRole>("user_role", nameTranslator);
        dataSourceBuilder.MapEnum<RequestStatus>("request_status", nameTranslator);
        dataSourceBuilder.MapEnum<RequestPriority>("request_priority", nameTranslator);
        dataSourceBuilder.MapEnum<MessageType>("message_type", nameTranslator);
        dataSourceBuilder.MapEnum<ClientRole>("client_role", nameTranslator);
        dataSourceBuilder.MapEnum<ParticipantRole>("participant_role", nameTranslator);
        dataSourceBuilder.MapEnum<NotificationType>("notification_type", nameTranslator);
        dataSourceBuilder.MapEnum<AuditAction>("audit_action", nameTranslator);
        dataSourceBuilder.MapEnum<InvitationStatus>("invitation_status", nameTranslator);

        var dataSource = dataSourceBuilder.Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(dataSource,
            b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

        return new AppDbContext(optionsBuilder.Options);
    }
}
