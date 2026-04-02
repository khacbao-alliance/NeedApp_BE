using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;
using NeedApp.Infrastructure.Persistence;
using NeedApp.Infrastructure.Persistence.Interceptors;
using NeedApp.Infrastructure.Persistence.Repositories;
using NeedApp.Infrastructure.Services;
using NeedApp.Infrastructure.Settings;
using Npgsql;
using Npgsql.NameTranslation;

namespace NeedApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        var nameTranslator = new NpgsqlSnakeCaseNameTranslator();
        dataSourceBuilder.MapEnum<UserRole>("user_role", nameTranslator);
        dataSourceBuilder.MapEnum<RequestStatus>("request_status", nameTranslator);
        dataSourceBuilder.MapEnum<MissingInfoStatus>("missing_info_status", nameTranslator);
        dataSourceBuilder.MapEnum<CommentType>("comment_type", nameTranslator);
        dataSourceBuilder.MapEnum<NotificationType>("notification_type", nameTranslator);
        dataSourceBuilder.MapEnum<AuditAction>("audit_action", nameTranslator);
        var dataSource = dataSourceBuilder.Build();
        services.AddSingleton(dataSource);

        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<AuditInterceptor>();
            options.UseNpgsql(dataSource,
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                .AddInterceptors(interceptor);
        });

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<GoogleSettings>(configuration.GetSection(GoogleSettings.SectionName));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IRequestRepository, RequestRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}
