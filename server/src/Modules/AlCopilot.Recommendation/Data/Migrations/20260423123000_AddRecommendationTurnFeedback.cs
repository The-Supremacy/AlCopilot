using AlCopilot.Recommendation.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.Recommendation.Data.Migrations;

[DbContext(typeof(RecommendationDbContext))]
[Migration("20260423123000_AddRecommendationTurnFeedback")]
public partial class AddRecommendationTurnFeedback : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FeedbackRating",
            schema: "recommendation",
            table: "ChatTurns",
            type: "character varying(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "FeedbackComment",
            schema: "recommendation",
            table: "ChatTurns",
            type: "character varying(1000)",
            maxLength: 1000,
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "FeedbackCreatedAtUtc",
            schema: "recommendation",
            table: "ChatTurns",
            type: "timestamp with time zone",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FeedbackRating",
            schema: "recommendation",
            table: "ChatTurns");

        migrationBuilder.DropColumn(
            name: "FeedbackComment",
            schema: "recommendation",
            table: "ChatTurns");

        migrationBuilder.DropColumn(
            name: "FeedbackCreatedAtUtc",
            schema: "recommendation",
            table: "ChatTurns");
    }
}
