using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NeedApp.Domain.Enums;

#nullable disable

namespace NeedApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSlaConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sla_configs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    priority = table.Column<RequestPriority>(type: "request_priority", nullable: false),
                    deadline_hours = table.Column<double>(type: "double precision", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sla_configs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_sla_configs_priority",
                table: "sla_configs",
                column: "priority",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sla_configs");
        }
    }
}
