using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeedApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifications : Migration
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
                .OldAnnotation("Npgsql:Enum:invitation_status", "pending,accepted,declined")
                .OldAnnotation("Npgsql:Enum:message_type", "text,file,system,missing_info,intake_question,intake_answer")
                .OldAnnotation("Npgsql:Enum:notification_type", "new_message,missing_info,status_change,assignment")
                .OldAnnotation("Npgsql:Enum:participant_role", "creator,assignee,observer")
                .OldAnnotation("Npgsql:Enum:request_priority", "low,medium,high,urgent")
                .OldAnnotation("Npgsql:Enum:request_status", "draft,intake,pending,missing_info,in_progress,done,cancelled")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,staff,client");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
