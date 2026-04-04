using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlCopilot.DrinkCatalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "drink_catalog");

            migrationBuilder.CreateTable(
                name: "domain_events",
                schema: "drink_catalog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventType = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Sequence = table.Column<long>(type: "bigint", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domain_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Drinks",
                schema: "drink_catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IngredientCategories",
                schema: "drink_catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                schema: "drink_catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                schema: "drink_catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IngredientCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    NotableBrands = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingredients_IngredientCategories_IngredientCategoryId",
                        column: x => x.IngredientCategoryId,
                        principalSchema: "drink_catalog",
                        principalTable: "IngredientCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DrinkTag",
                schema: "drink_catalog",
                columns: table => new
                {
                    DrinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrinkTag", x => new { x.DrinkId, x.TagId });
                    table.ForeignKey(
                        name: "FK_DrinkTag_Drinks_DrinkId",
                        column: x => x.DrinkId,
                        principalSchema: "drink_catalog",
                        principalTable: "Drinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrinkTag_Tags_TagId",
                        column: x => x.TagId,
                        principalSchema: "drink_catalog",
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeEntries",
                schema: "drink_catalog",
                columns: table => new
                {
                    DrinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecommendedBrand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeEntries", x => new { x.DrinkId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_RecipeEntries_Drinks_DrinkId",
                        column: x => x.DrinkId,
                        principalSchema: "drink_catalog",
                        principalTable: "Drinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeEntries_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalSchema: "drink_catalog",
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_domain_events_IsPublished",
                schema: "drink_catalog",
                table: "domain_events",
                column: "IsPublished",
                filter: "\"IsPublished\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Drinks_Name",
                schema: "drink_catalog",
                table: "Drinks",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrinkTag_TagId",
                schema: "drink_catalog",
                table: "DrinkTag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientCategories_Name",
                schema: "drink_catalog",
                table: "IngredientCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_IngredientCategoryId",
                schema: "drink_catalog",
                table: "Ingredients",
                column: "IngredientCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_Name",
                schema: "drink_catalog",
                table: "Ingredients",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeEntries_IngredientId",
                schema: "drink_catalog",
                table: "RecipeEntries",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                schema: "drink_catalog",
                table: "Tags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domain_events",
                schema: "drink_catalog");

            migrationBuilder.DropTable(
                name: "DrinkTag",
                schema: "drink_catalog");

            migrationBuilder.DropTable(
                name: "RecipeEntries",
                schema: "drink_catalog");

            migrationBuilder.DropTable(
                name: "Tags",
                schema: "drink_catalog");

            migrationBuilder.DropTable(
                name: "Drinks",
                schema: "drink_catalog");

            migrationBuilder.DropTable(
                name: "Ingredients",
                schema: "drink_catalog");

            migrationBuilder.DropTable(
                name: "IngredientCategories",
                schema: "drink_catalog");
        }
    }
}
