using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeedApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageEditHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:audit_action", "insert,update,delete")
                .Annotation("Npgsql:Enum:client_role", "owner,member")
                .Annotation("Npgsql:Enum:invitation_status", "pending,accepted,declined")
                .Annotation("Npgsql:Enum:message_type", "text,file,system,missing_info,intake_question,intake_answer")
                .Annotation("Npgsql:Enum:notification_type", "new_message,missing_info,status_change,assignment,new_request,invitation")
                .Annotation("Npgsql:Enum:participant_role", "creator,assignee,observer")
                .Annotation("Npgsql:Enum:request_priority", "low,medium,high,urgent")
                .Annotation("Npgsql:Enum:request_status", "draft,intake,pending,missing_info,in_progress,done,cancelled")
                .Annotation("Npgsql:Enum:user_role", "admin,staff,client")
                .OldAnnotation("Npgsql:Enum:audit_action", "insert,update,delete")
                .OldAnnotation("Npgsql:Enum:client_role", "owner,member")
                .OldAnnotation("Npgsql:Enum:digest_frequency", "none,daily,weekly")
                .OldAnnotation("Npgsql:Enum:invitation_status", "pending,accepted,declined")
                .OldAnnotation("Npgsql:Enum:message_type", "text,file,system,missing_info,intake_question,intake_answer")
                .OldAnnotation("Npgsql:Enum:notification_type", "new_message,missing_info,status_change,assignment,new_request,invitation")
                .OldAnnotation("Npgsql:Enum:participant_role", "creator,assignee,observer")
                .OldAnnotation("Npgsql:Enum:request_priority", "low,medium,high,urgent")
                .OldAnnotation("Npgsql:Enum:request_status", "draft,intake,pending,missing_info,in_progress,done,cancelled")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,staff,client");

            // NOTE: edited_at, is_edited, is_pinned columns already exist on the messages table
            // from migration AddMessageEditAndPin — only create the new history table here.

            migrationBuilder.CreateTable(
                name: "message_edit_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_content = table.Column<string>(type: "text", nullable: false),
                    edited_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    edited_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_edit_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_message_edit_history_messages_message_id",
                        column: x => x.message_id,
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_message_edit_history_users_edited_by",
                        column: x => x.edited_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "idx_message_edit_history_message_edited",
                table: "message_edit_history",
                columns: new[] { "message_id", "edited_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_message_edit_history_edited_by",
                table: "message_edit_history",
                column: "edited_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "message_edit_history");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:audit_action", "insert,update,delete")
                .Annotation("Npgsql:Enum:client_role", "owner,member")
                .Annotation("Npgsql:Enum:digest_frequency", "none,daily,weekly")
                .Annotation("Npgsql:Enum:invitation_status", "pending,accepted,declined")
                .Annotation("Npgsql:Enum:message_type", "text,file,system,missing_info,intake_question,intake_answer")
                .Annotation("Npgsql:Enum:notification_type", "new_message,missing_info,status_change,assignment,new_request,invitation")
                .Annotation("Npgsql:Enum:participant_role", "creator,assignee,observer")
                .Annotation("Npgsql:Enum:request_priority", "low,medium,high,urgent")
                .Annotation("Npgsql:Enum:request_status", "draft,intake,pending,missing_info,in_progress,done,cancelled")
                .Annotation("Npgsql:Enum:user_role", "admin,staff,client")
                .OldAnnotation("Npgsql:Enum:audit_action", "insert,update,delete")
                .OldAnnotation("Npgsql:Enum:client_role", "owner,member")
                .OldAnnotation("Npgsql:Enum:invitation_status", "pending,accepted,declined")
                .OldAnnotation("Npgsql:Enum:message_type", "text,file,system,missing_info,intake_question,intake_answer")
                .OldAnnotation("Npgsql:Enum:notification_type", "new_message,missing_info,status_change,assignment,new_request,invitation")
                .OldAnnotation("Npgsql:Enum:participant_role", "creator,assignee,observer")
                .OldAnnotation("Npgsql:Enum:request_priority", "low,medium,high,urgent")
                .OldAnnotation("Npgsql:Enum:request_status", "draft,intake,pending,missing_info,in_progress,done,cancelled")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,staff,client");
        }
    }
}
