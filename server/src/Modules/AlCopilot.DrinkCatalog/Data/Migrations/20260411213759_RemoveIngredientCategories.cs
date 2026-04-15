using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.DrinkCatalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIngredientCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredients_IngredientCategories_IngredientCategoryId",
                schema: "drink_catalog",
                table: "Ingredients");

            migrationBuilder.DropTable(
                name: "IngredientCategories",
                schema: "drink_catalog");

            migrationBuilder.DropIndex(
                name: "IX_Ingredients_IngredientCategoryId",
                schema: "drink_catalog",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "IngredientCategoryId",
                schema: "drink_catalog",
                table: "Ingredients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IngredientCategoryId",
                schema: "drink_catalog",
                table: "Ingredients",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "IngredientCategories",
                schema: "drink_catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_IngredientCategoryId",
                schema: "drink_catalog",
                table: "Ingredients",
                column: "IngredientCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientCategories_Name",
                schema: "drink_catalog",
                table: "IngredientCategories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredients_IngredientCategories_IngredientCategoryId",
                schema: "drink_catalog",
                table: "Ingredients",
                column: "IngredientCategoryId",
                principalSchema: "drink_catalog",
                principalTable: "IngredientCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
