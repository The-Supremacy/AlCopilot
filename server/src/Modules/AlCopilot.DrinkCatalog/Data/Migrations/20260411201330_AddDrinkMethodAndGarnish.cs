using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.DrinkCatalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDrinkMethodAndGarnish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Garnish",
                schema: "drink_catalog",
                table: "Drinks",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Method",
                schema: "drink_catalog",
                table: "Drinks",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Garnish",
                schema: "drink_catalog",
                table: "Drinks");

            migrationBuilder.DropColumn(
                name: "Method",
                schema: "drink_catalog",
                table: "Drinks");
        }
    }
}
