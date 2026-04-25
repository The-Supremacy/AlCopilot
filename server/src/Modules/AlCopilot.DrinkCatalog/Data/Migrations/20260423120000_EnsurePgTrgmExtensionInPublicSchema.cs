using AlCopilot.DrinkCatalog.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.DrinkCatalog.Data.Migrations;

[DbContext(typeof(DrinkCatalogDbContext))]
[Migration("20260423120000_EnsurePgTrgmExtensionInPublicSchema")]
public partial class EnsurePgTrgmExtensionInPublicSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE EXTENSION IF NOT EXISTS pg_trgm WITH SCHEMA public;
            ALTER EXTENSION pg_trgm SET SCHEMA public;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
