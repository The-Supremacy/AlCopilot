using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.DrinkCatalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveImportReviewConflicts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewConflicts",
                schema: "drink_catalog",
                table: "ImportBatches");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewConflicts",
                schema: "drink_catalog",
                table: "ImportBatches",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }
    }
}
