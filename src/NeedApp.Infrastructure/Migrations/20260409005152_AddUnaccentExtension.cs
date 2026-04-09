using Microsoft.EntityFrameworkCore.Migrations;
using NeedApp.Domain.Enums;

#nullable disable

namespace NeedApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnaccentExtension : Migration
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
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,")
                .OldAnnotation("Npgsql:Enum:audit_action", "insert,update,delete")
                .OldAnnotation("Npgsql:Enum:client_role", "owner,member")
                .OldAnnotation("Npgsql:Enum:invitation_status", "pending,accepted,declined")
                .OldAnnotation("Npgsql:Enum:message_type", "text,file,system,missing_info,intake_question,intake_answer")
                .OldAnnotation("Npgsql:Enum:notification_type", "new_message,missing_info,status_change,assignment,new_request,invitation")
                .OldAnnotation("Npgsql:Enum:participant_role", "creator,assignee,observer")
                .OldAnnotation("Npgsql:Enum:request_priority", "low,medium,high,urgent")
                .OldAnnotation("Npgsql:Enum:request_status", "draft,intake,pending,missing_info,in_progress,done,cancelled")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,staff,client");

            migrationBuilder.AlterColumn<UserRole>(
                name: "role",
                table: "users",
                type: "user_role",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<RequestStatus>(
                name: "status",
                table: "requests",
                type: "request_status",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<RequestPriority>(
                name: "priority",
                table: "requests",
                type: "request_priority",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<ParticipantRole>(
                name: "role",
                table: "request_participants",
                type: "participant_role",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<NotificationType>(
                name: "type",
                table: "notifications",
                type: "notification_type",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<MessageType>(
                name: "type",
                table: "messages",
                type: "message_type",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<InvitationStatus>(
                name: "status",
                table: "invitations",
                type: "invitation_status",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<ClientRole>(
                name: "role",
                table: "invitations",
                type: "client_role",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<ClientRole>(
                name: "role",
                table: "client_users",
                type: "client_role",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<AuditAction>(
                name: "action",
                table: "audit_logs",
                type: "audit_action",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                .OldAnnotation("Npgsql:Enum:invitation_status", "pending,accepted,declined")
                .OldAnnotation("Npgsql:Enum:message_type", "text,file,system,missing_info,intake_question,intake_answer")
                .OldAnnotation("Npgsql:Enum:notification_type", "new_message,missing_info,status_change,assignment,new_request,invitation")
                .OldAnnotation("Npgsql:Enum:participant_role", "creator,assignee,observer")
                .OldAnnotation("Npgsql:Enum:request_priority", "low,medium,high,urgent")
                .OldAnnotation("Npgsql:Enum:request_status", "draft,intake,pending,missing_info,in_progress,done,cancelled")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,staff,client")
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.AlterColumn<int>(
                name: "role",
                table: "users",
                type: "integer",
                nullable: true,
                oldClrType: typeof(UserRole),
                oldType: "user_role",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "requests",
                type: "integer",
                nullable: false,
                oldClrType: typeof(RequestStatus),
                oldType: "request_status");

            migrationBuilder.AlterColumn<int>(
                name: "priority",
                table: "requests",
                type: "integer",
                nullable: false,
                oldClrType: typeof(RequestPriority),
                oldType: "request_priority");

            migrationBuilder.AlterColumn<int>(
                name: "role",
                table: "request_participants",
                type: "integer",
                nullable: false,
                oldClrType: typeof(ParticipantRole),
                oldType: "participant_role");

            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "notifications",
                type: "integer",
                nullable: false,
                oldClrType: typeof(NotificationType),
                oldType: "notification_type");

            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "messages",
                type: "integer",
                nullable: false,
                oldClrType: typeof(MessageType),
                oldType: "message_type");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "invitations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(InvitationStatus),
                oldType: "invitation_status");

            migrationBuilder.AlterColumn<int>(
                name: "role",
                table: "invitations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(ClientRole),
                oldType: "client_role");

            migrationBuilder.AlterColumn<int>(
                name: "role",
                table: "client_users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(ClientRole),
                oldType: "client_role");

            migrationBuilder.AlterColumn<int>(
                name: "action",
                table: "audit_logs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(AuditAction),
                oldType: "audit_action",
                oldNullable: true);
        }
    }
}
