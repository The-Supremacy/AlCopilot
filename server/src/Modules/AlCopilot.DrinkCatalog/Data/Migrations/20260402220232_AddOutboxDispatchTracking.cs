using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.DrinkCatalog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxDispatchTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DispatchedAtUtc",
                schema: "drink_catalog",
                table: "domain_events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_domain_events_DispatchedAtUtc_Id",
                schema: "drink_catalog",
                table: "domain_events",
                columns: new[] { "DispatchedAtUtc", "Id" },
                filter: "\"DispatchedAtUtc\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_domain_events_DispatchedAtUtc_Id",
                schema: "drink_catalog",
                table: "domain_events");

            migrationBuilder.DropColumn(
                name: "DispatchedAtUtc",
                schema: "drink_catalog",
                table: "domain_events");
        }
    }
}
