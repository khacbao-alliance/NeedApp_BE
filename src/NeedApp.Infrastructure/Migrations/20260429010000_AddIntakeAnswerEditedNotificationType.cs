using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeedApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIntakeAnswerEditedNotificationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PostgreSQL enums are immutable — we must add the new value via raw SQL.
            // ALTER TYPE ... ADD VALUE is transactional in Postgres 12+.
            migrationBuilder.Sql("ALTER TYPE notification_type ADD VALUE IF NOT EXISTS 'intake_answer_edited';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Postgres does not support removing enum values.
            // A full recreate would be needed; leave as no-op for safety.
        }
    }
}
