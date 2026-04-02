using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.DrinkCatalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class CleanupDomainEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_domain_events_IsPublished",
                schema: "drink_catalog",
                table: "domain_events");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                schema: "drink_catalog",
                table: "domain_events");

            migrationBuilder.DropColumn(
                name: "Sequence",
                schema: "drink_catalog",
                table: "domain_events");

            migrationBuilder.CreateIndex(
                name: "IX_domain_events_AggregateId_Id",
                schema: "drink_catalog",
                table: "domain_events",
                columns: new[] { "AggregateId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_domain_events_OccurredAtUtc",
                schema: "drink_catalog",
                table: "domain_events",
                column: "OccurredAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_domain_events_AggregateId_Id",
                schema: "drink_catalog",
                table: "domain_events");

            migrationBuilder.DropIndex(
                name: "IX_domain_events_OccurredAtUtc",
                schema: "drink_catalog",
                table: "domain_events");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                schema: "drink_catalog",
                table: "domain_events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "Sequence",
                schema: "drink_catalog",
                table: "domain_events",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_domain_events_IsPublished",
                schema: "drink_catalog",
                table: "domain_events",
                column: "IsPublished",
                filter: "\"IsPublished\" = false");
        }
    }
}
