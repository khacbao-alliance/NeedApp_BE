using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeedApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixNotificationIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Restore filtered index to its correct name
            migrationBuilder.RenameIndex(
                name: "idx_notifications_user_all",
                table: "notifications",
                newName: "idx_notifications_user_unread");

            // Add unfiltered index for paginated "all notifications" queries
            migrationBuilder.Sql(
                "CREATE INDEX idx_notifications_user_all ON notifications (user_id, created_at DESC);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_notifications_user_all;");

            migrationBuilder.RenameIndex(
                name: "idx_notifications_user_unread",
                table: "notifications",
                newName: "idx_notifications_user_all");
        }
    }
}
