using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.DrinkCatalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveImportBatchSourceFingerprint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImportBatches_StrategyKey_SourceFingerprint_Status",
                schema: "drink_catalog",
                table: "ImportBatches");

            migrationBuilder.DropColumn(
                name: "SourceFingerprint",
                schema: "drink_catalog",
                table: "ImportBatches");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceFingerprint",
                schema: "drink_catalog",
                table: "ImportBatches",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_StrategyKey_SourceFingerprint_Status",
                schema: "drink_catalog",
                table: "ImportBatches",
                columns: new[] { "StrategyKey", "SourceFingerprint", "Status" });
        }
    }
}
