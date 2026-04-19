using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.Recommendation.Data.Migrations
{
    /// <inheritdoc />
    public partial class PersistAgentFrameworkSessionState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgentSessionStateJson",
                schema: "recommendation",
                table: "ChatSessions",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentSessionStateJson",
                schema: "recommendation",
                table: "ChatSessions");
        }
    }
}
