using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.Recommendation.Data.Migrations
{
    /// <inheritdoc />
    public partial class PersistChatTurnExecutionTrace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExecutionTraceJson",
                schema: "recommendation",
                table: "ChatTurns",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutionTraceJson",
                schema: "recommendation",
                table: "ChatTurns");
        }
    }
}
