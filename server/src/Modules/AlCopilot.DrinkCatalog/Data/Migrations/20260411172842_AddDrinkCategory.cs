using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.DrinkCatalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDrinkCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "drink_catalog",
                table: "Drinks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                schema: "drink_catalog",
                table: "Drinks");
        }
    }
}
