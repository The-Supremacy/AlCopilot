using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.DrinkCatalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameImportPreviewToReviewAndLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreviewedAtUtc",
                schema: "drink_catalog",
                table: "ImportBatches",
                newName: "ReviewedAtUtc");

            migrationBuilder.RenameColumn(
                name: "PreviewSummary",
                schema: "drink_catalog",
                table: "ImportBatches",
                newName: "ReviewSummary");

            migrationBuilder.RenameColumn(
                name: "PreviewRows",
                schema: "drink_catalog",
                table: "ImportBatches",
                newName: "ReviewRows");

            migrationBuilder.RenameColumn(
                name: "PreviewConflicts",
                schema: "drink_catalog",
                table: "ImportBatches",
                newName: "ReviewConflicts");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAtUtc",
                schema: "drink_catalog",
                table: "ImportBatches",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                schema: "drink_catalog",
                table: "ImportBatches");

            migrationBuilder.RenameColumn(
                name: "ReviewedAtUtc",
                schema: "drink_catalog",
                table: "ImportBatches",
                newName: "PreviewedAtUtc");

            migrationBuilder.RenameColumn(
                name: "ReviewSummary",
                schema: "drink_catalog",
                table: "ImportBatches",
                newName: "PreviewSummary");

            migrationBuilder.RenameColumn(
                name: "ReviewRows",
                schema: "drink_catalog",
                table: "ImportBatches",
                newName: "PreviewRows");

            migrationBuilder.RenameColumn(
                name: "ReviewConflicts",
                schema: "drink_catalog",
                table: "ImportBatches",
                newName: "PreviewConflicts");
        }
    }
}
