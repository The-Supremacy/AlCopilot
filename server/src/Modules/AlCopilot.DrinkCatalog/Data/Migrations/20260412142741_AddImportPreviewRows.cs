using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.DrinkCatalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImportPreviewRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviewRows",
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
                name: "PreviewRows",
                schema: "drink_catalog",
                table: "ImportBatches");
        }
    }
}
