using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.DrinkCatalog.Data.Migrations;

public partial class AddPgTrgmSupportForFuzzyLookup : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_Drinks_Name_Trgm"
            ON drink_catalog."Drinks"
            USING gin ("Name" gin_trgm_ops);
            """);
        migrationBuilder.Sql("""
            CREATE INDEX IF NOT EXISTS "IX_Ingredients_Name_Trgm"
            ON drink_catalog."Ingredients"
            USING gin ("Name" gin_trgm_ops);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""DROP INDEX IF EXISTS drink_catalog."IX_Drinks_Name_Trgm";""");
        migrationBuilder.Sql("""DROP INDEX IF EXISTS drink_catalog."IX_Ingredients_Name_Trgm";""");
        migrationBuilder.Sql("DROP EXTENSION IF EXISTS pg_trgm;");
    }
}
