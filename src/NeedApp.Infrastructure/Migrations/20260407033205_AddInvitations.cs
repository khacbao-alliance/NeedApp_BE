using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NeedApp.Domain.Enums;

#nullable disable

namespace NeedApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:audit_action", "insert,update,delete")
                .Annotation("Npgsql:Enum:client_role", "owner,member")
                .Annotation("Npgsql:Enum:invitation_status", "pending,accepted,declined")
                .Annotation("Npgsql:Enum:message_type", "text,file,system,missing_info,intake_question,intake_answer")
                .Annotation("Npgsql:Enum:notification_type", "new_message,missing_info,status_change,assignment")
                .Annotation("Npgsql:Enum:participant_role", "creator,assignee,observer")
                .Annotation("Npgsql:Enum:request_priority", "low,medium,high,urgent")
                .Annotation("Npgsql:Enum:request_status", "draft,intake,pending,missing_info,in_progress,done,cancelled")
                .Annotation("Npgsql:Enum:user_role", "admin,staff,client")
                .OldAnnotation("Npgsql:Enum:audit_action", "insert,update,delete")
                .OldAnnotation("Npgsql:Enum:client_role", "owner,member")
                .OldAnnotation("Npgsql:Enum:message_type", "text,file,system,missing_info,intake_question,intake_answer")
                .OldAnnotation("Npgsql:Enum:notification_type", "new_message,missing_info,status_change,assignment")
                .OldAnnotation("Npgsql:Enum:participant_role", "creator,assignee,observer")
                .OldAnnotation("Npgsql:Enum:request_priority", "low,medium,high,urgent")
                .OldAnnotation("Npgsql:Enum:request_status", "draft,intake,pending,missing_info,in_progress,done,cancelled")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,staff,client");

            migrationBuilder.CreateTable(
                name: "invitations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<ClientRole>(type: "client_role", nullable: false),
                    status = table.Column<InvitationStatus>(type: "invitation_status", nullable: false),
                    responded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitations", x => x.id);
                    table.ForeignKey(
                        name: "FK_invitations_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invitations_users_invited_by_user_id",
                        column: x => x.invited_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invitations_users_invited_user_id",
                        column: x => x.invited_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_invitations_unique_pending",
                table: "invitations",
                columns: new[] { "client_id", "invited_user_id", "status" },
                unique: true,
                filter: "status = 'pending'");

            migrationBuilder.CreateIndex(
                name: "idx_invitations_user",
                table: "invitations",
                column: "invited_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_invited_by_user_id",
                table: "invitations",
                column: "invited_by_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invitations");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:audit_action", "insert,update,delete")
                .Annotation("Npgsql:Enum:client_role", "owner,member")
                .Annotation("Npgsql:Enum:message_type", "text,file,system,missing_info,intake_question,intake_answer")
                .Annotation("Npgsql:Enum:notification_type", "new_message,missing_info,status_change,assignment")
                .Annotation("Npgsql:Enum:participant_role", "creator,assignee,observer")
                .Annotation("Npgsql:Enum:request_priority", "low,medium,high,urgent")
                .Annotation("Npgsql:Enum:request_status", "draft,intake,pending,missing_info,in_progress,done,cancelled")
                .Annotation("Npgsql:Enum:user_role", "admin,staff,client")
                .OldAnnotation("Npgsql:Enum:audit_action", "insert,update,delete")
                .OldAnnotation("Npgsql:Enum:client_role", "owner,member")
                .OldAnnotation("Npgsql:Enum:invitation_status", "pending,accepted,declined")
                .OldAnnotation("Npgsql:Enum:message_type", "text,file,system,missing_info,intake_question,intake_answer")
                .OldAnnotation("Npgsql:Enum:notification_type", "new_message,missing_info,status_change,assignment")
                .OldAnnotation("Npgsql:Enum:participant_role", "creator,assignee,observer")
                .OldAnnotation("Npgsql:Enum:request_priority", "low,medium,high,urgent")
                .OldAnnotation("Npgsql:Enum:request_status", "draft,intake,pending,missing_info,in_progress,done,cancelled")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,staff,client");
        }
    }
}
