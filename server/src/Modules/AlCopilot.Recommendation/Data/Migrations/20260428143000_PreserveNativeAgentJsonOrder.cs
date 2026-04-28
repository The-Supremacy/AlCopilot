using AlCopilot.Recommendation.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.Recommendation.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(RecommendationDbContext))]
    [Migration("20260428143000_PreserveNativeAgentJsonOrder")]
    public partial class PreserveNativeAgentJsonOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE recommendation."AgentMessages"
                    ALTER COLUMN "RawMessageJson" TYPE text
                    USING "RawMessageJson"::text;

                ALTER TABLE recommendation."ChatSessions"
                    ALTER COLUMN "AgentSessionStateJson" TYPE text
                    USING "AgentSessionStateJson"::text;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE recommendation."ChatSessions"
                    ALTER COLUMN "AgentSessionStateJson" TYPE jsonb
                    USING "AgentSessionStateJson"::jsonb;

                ALTER TABLE recommendation."AgentMessages"
                    ALTER COLUMN "RawMessageJson" TYPE jsonb
                    USING "RawMessageJson"::jsonb;
                """);
        }
    }
}
