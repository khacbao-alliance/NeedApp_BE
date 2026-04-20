using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeedApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageEditAndPin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_edited",
                table: "messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "edited_at",
                table: "messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_pinned",
                table: "messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "idx_messages_is_pinned",
                table: "messages",
                column: "is_pinned",
                filter: "is_pinned = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_messages_is_pinned",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "is_edited",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "edited_at",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "is_pinned",
                table: "messages");
        }
    }
}
