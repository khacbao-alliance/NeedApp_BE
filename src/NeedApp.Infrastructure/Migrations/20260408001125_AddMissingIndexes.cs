using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeedApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_requests_assigned_to",
                table: "requests",
                newName: "idx_requests_assigned_to");

            migrationBuilder.RenameIndex(
                name: "idx_notifications_user_unread",
                table: "notifications",
                newName: "idx_notifications_user_all");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "idx_requests_assigned_to",
                table: "requests",
                newName: "IX_requests_assigned_to");

            migrationBuilder.RenameIndex(
                name: "idx_notifications_user_all",
                table: "notifications",
                newName: "idx_notifications_user_unread");
        }
    }
}
