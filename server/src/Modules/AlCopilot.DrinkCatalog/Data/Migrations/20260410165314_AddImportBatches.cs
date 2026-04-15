using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.DrinkCatalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImportBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportBatches",
                schema: "drink_catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StrategyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceFingerprint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Provenance = table.Column<string>(type: "jsonb", nullable: false),
                    Diagnostics = table.Column<string>(type: "jsonb", nullable: false),
                    DecisionAuditTrail = table.Column<string>(type: "jsonb", nullable: false),
                    PreviewSummary = table.Column<string>(type: "jsonb", nullable: true),
                    ApplySummary = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ValidatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PreviewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AppliedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastUpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportBatches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_CreatedAtUtc",
                schema: "drink_catalog",
                table: "ImportBatches",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_StrategyKey_SourceFingerprint_Status",
                schema: "drink_catalog",
                table: "ImportBatches",
                columns: new[] { "StrategyKey", "SourceFingerprint", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportBatches",
                schema: "drink_catalog");
        }
    }
}
