using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NeedApp.Domain.Enums;
using Npgsql;
using Npgsql.NameTranslation;

namespace NeedApp.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by EF Core CLI tools (migrations, scaffolding).
/// Reads connection string from appsettings.Local.json (gitignored) → appsettings.json → env var.
/// To run migrations: cd to repo root, then `dotnet ef database update --project ...`
/// No manual env var setup needed as long as appsettings.Local.json is present.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Walk up from Infrastructure project to find the API project's appsettings
        var basePath = FindApiProjectPath();

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)   // gitignored — holds real credentials
            .AddEnvironmentVariables()                                // fallback for CI/CD
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. " +
                "Add appsettings.Local.json to NeedApp.API with ConnectionStrings:DefaultConnection.");

        var nameTranslator = new NpgsqlSnakeCaseNameTranslator();
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

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

    /// <summary>
    /// Locates the NeedApp.API directory relative to this assembly's location,
    /// regardless of where dotnet-ef is invoked from.
    /// </summary>
    private static string FindApiProjectPath()
    {
        // When run via `dotnet ef --startup-project src/NeedApp.API`, the startup
        // project directory is the working directory. Otherwise search upward.
        var candidates = new[]
        {
            Directory.GetCurrentDirectory(),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..","src", "NeedApp.API"),
            Path.Combine(AppContext.BaseDirectory, "..", "NeedApp.API"),
        };

        foreach (var candidate in candidates)
        {
            var full = Path.GetFullPath(candidate);
            if (File.Exists(Path.Combine(full, "appsettings.json")))
                return full;
        }

        return Directory.GetCurrentDirectory();
    }
}
