using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeedApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailPreferences_users_UserId",
                table: "EmailPreferences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailPreferences",
                table: "EmailPreferences");

            migrationBuilder.RenameTable(
                name: "EmailPreferences",
                newName: "email_preferences");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "email_preferences",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "email_preferences",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "OnStatusChange",
                table: "email_preferences",
                newName: "on_status_change");

            migrationBuilder.RenameColumn(
                name: "OnOverdue",
                table: "email_preferences",
                newName: "on_overdue");

            migrationBuilder.RenameColumn(
                name: "OnNewRequest",
                table: "email_preferences",
                newName: "on_new_request");

            migrationBuilder.RenameColumn(
                name: "OnAssignment",
                table: "email_preferences",
                newName: "on_assignment");

            migrationBuilder.RenameColumn(
                name: "LastDigestSentAt",
                table: "email_preferences",
                newName: "last_digest_sent_at");

            migrationBuilder.RenameColumn(
                name: "DigestFrequency",
                table: "email_preferences",
                newName: "digest_frequency");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "email_preferences",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_EmailPreferences_UserId",
                table: "email_preferences",
                newName: "IX_email_preferences_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_email_preferences",
                table: "email_preferences",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_email_preferences_users_user_id",
                table: "email_preferences",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_email_preferences_users_user_id",
                table: "email_preferences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_email_preferences",
                table: "email_preferences");

            migrationBuilder.RenameTable(
                name: "email_preferences",
                newName: "EmailPreferences");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "EmailPreferences",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "EmailPreferences",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "on_status_change",
                table: "EmailPreferences",
                newName: "OnStatusChange");

            migrationBuilder.RenameColumn(
                name: "on_overdue",
                table: "EmailPreferences",
                newName: "OnOverdue");

            migrationBuilder.RenameColumn(
                name: "on_new_request",
                table: "EmailPreferences",
                newName: "OnNewRequest");

            migrationBuilder.RenameColumn(
                name: "on_assignment",
                table: "EmailPreferences",
                newName: "OnAssignment");

            migrationBuilder.RenameColumn(
                name: "last_digest_sent_at",
                table: "EmailPreferences",
                newName: "LastDigestSentAt");

            migrationBuilder.RenameColumn(
                name: "digest_frequency",
                table: "EmailPreferences",
                newName: "DigestFrequency");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "EmailPreferences",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_email_preferences_user_id",
                table: "EmailPreferences",
                newName: "IX_EmailPreferences_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailPreferences",
                table: "EmailPreferences",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailPreferences_users_UserId",
                table: "EmailPreferences",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
