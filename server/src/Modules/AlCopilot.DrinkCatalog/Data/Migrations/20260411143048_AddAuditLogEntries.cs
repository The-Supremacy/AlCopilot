using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlCopilot.DrinkCatalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_log_entries",
                schema: "drink_catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SubjectType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SubjectKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Actor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_entries_OccurredAtUtc",
                schema: "drink_catalog",
                table: "audit_log_entries",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_entries_SubjectType_SubjectKey",
                schema: "drink_catalog",
                table: "audit_log_entries",
                columns: new[] { "SubjectType", "SubjectKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log_entries",
                schema: "drink_catalog");
        }
    }
}
