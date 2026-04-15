using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.DrinkCatalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtendImportBatchesForWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImportContent",
                schema: "drink_catalog",
                table: "ImportBatches",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreviewConflicts",
                schema: "drink_catalog",
                table: "ImportBatches",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImportContent",
                schema: "drink_catalog",
                table: "ImportBatches");

            migrationBuilder.DropColumn(
                name: "PreviewConflicts",
                schema: "drink_catalog",
                table: "ImportBatches");
        }
    }
}
