using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlCopilot.Recommendation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNativeAgentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "recommendation");

            migrationBuilder.Sql(
                """
                TRUNCATE TABLE
                    recommendation."ChatTurns",
                    recommendation."ChatSessions",
                    recommendation."DomainEventRecords"
                CASCADE;
                """);

            migrationBuilder.DropTable(
                name: "ChatTurns",
                schema: "recommendation");

            migrationBuilder.CreateTable(
                name: "AgentRuns",
                schema: "recommendation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FinishReason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InputTokenCount = table.Column<long>(type: "bigint", nullable: true),
                    OutputTokenCount = table.Column<long>(type: "bigint", nullable: true),
                    ReasoningTokenCount = table.Column<long>(type: "bigint", nullable: true),
                    ErrorSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentRuns_ChatSessions_ChatSessionId",
                        column: x => x.ChatSessionId,
                        principalSchema: "recommendation",
                        principalTable: "ChatSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgentMessages",
                schema: "recommendation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    NativeMessageId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Kind = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TextContent = table.Column<string>(type: "text", nullable: true),
                    RawMessageJson = table.Column<string>(type: "jsonb", nullable: false),
                    FeedbackRating = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FeedbackComment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FeedbackCreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentMessages_AgentRuns_AgentRunId",
                        column: x => x.AgentRunId,
                        principalSchema: "recommendation",
                        principalTable: "AgentRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AgentMessages_ChatSessions_ChatSessionId",
                        column: x => x.ChatSessionId,
                        principalSchema: "recommendation",
                        principalTable: "ChatSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationTurnGroups",
                schema: "recommendation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationTurnGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendationTurnGroups_AgentRuns_AgentRunId",
                        column: x => x.AgentRunId,
                        principalSchema: "recommendation",
                        principalTable: "AgentRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgentMessageDiagnostics",
                schema: "recommendation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    Kind = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    RawPayloadJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentMessageDiagnostics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentMessageDiagnostics_AgentMessages_AgentMessageId",
                        column: x => x.AgentMessageId,
                        principalSchema: "recommendation",
                        principalTable: "AgentMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AgentMessageDiagnostics_AgentRuns_AgentRunId",
                        column: x => x.AgentRunId,
                        principalSchema: "recommendation",
                        principalTable: "AgentRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgentMessageDiagnostics_ChatSessions_ChatSessionId",
                        column: x => x.ChatSessionId,
                        principalSchema: "recommendation",
                        principalTable: "ChatSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationTurnItems",
                schema: "recommendation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecommendationTurnGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    DrinkName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationTurnItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendationTurnItems_RecommendationTurnGroups_Recommenda~",
                        column: x => x.RecommendationTurnGroupId,
                        principalSchema: "recommendation",
                        principalTable: "RecommendationTurnGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationTurnItemMatchedSignals",
                schema: "recommendation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecommendationTurnItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Signal = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationTurnItemMatchedSignals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendationTurnItemMatchedSignals_RecommendationTurnItem~",
                        column: x => x.RecommendationTurnItemId,
                        principalSchema: "recommendation",
                        principalTable: "RecommendationTurnItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationTurnItemMissingIngredients",
                schema: "recommendation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecommendationTurnItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    IngredientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationTurnItemMissingIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendationTurnItemMissingIngredients_RecommendationTurn~",
                        column: x => x.RecommendationTurnItemId,
                        principalSchema: "recommendation",
                        principalTable: "RecommendationTurnItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationTurnItemRecipeEntries",
                schema: "recommendation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecommendationTurnItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    IngredientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsOwned = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationTurnItemRecipeEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendationTurnItemRecipeEntries_RecommendationTurnItems~",
                        column: x => x.RecommendationTurnItemId,
                        principalSchema: "recommendation",
                        principalTable: "RecommendationTurnItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentMessageDiagnostics_AgentMessageId",
                schema: "recommendation",
                table: "AgentMessageDiagnostics",
                column: "AgentMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentMessageDiagnostics_AgentRunId",
                schema: "recommendation",
                table: "AgentMessageDiagnostics",
                column: "AgentRunId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentMessageDiagnostics_ChatSessionId",
                schema: "recommendation",
                table: "AgentMessageDiagnostics",
                column: "ChatSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentMessages_AgentRunId",
                schema: "recommendation",
                table: "AgentMessages",
                column: "AgentRunId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentMessages_ChatSessionId_NativeMessageId",
                schema: "recommendation",
                table: "AgentMessages",
                columns: new[] { "ChatSessionId", "NativeMessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentMessages_ChatSessionId_Sequence",
                schema: "recommendation",
                table: "AgentMessages",
                columns: new[] { "ChatSessionId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentRuns_ChatSessionId",
                schema: "recommendation",
                table: "AgentRuns",
                column: "ChatSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationTurnGroups_AgentRunId_Sequence",
                schema: "recommendation",
                table: "RecommendationTurnGroups",
                columns: new[] { "AgentRunId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationTurnItemMatchedSignals_RecommendationTurnItem~",
                schema: "recommendation",
                table: "RecommendationTurnItemMatchedSignals",
                columns: new[] { "RecommendationTurnItemId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationTurnItemMissingIngredients_RecommendationTurn~",
                schema: "recommendation",
                table: "RecommendationTurnItemMissingIngredients",
                columns: new[] { "RecommendationTurnItemId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationTurnItemRecipeEntries_RecommendationTurnItemI~",
                schema: "recommendation",
                table: "RecommendationTurnItemRecipeEntries",
                columns: new[] { "RecommendationTurnItemId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationTurnItems_RecommendationTurnGroupId_Sequence",
                schema: "recommendation",
                table: "RecommendationTurnItems",
                columns: new[] { "RecommendationTurnGroupId", "Sequence" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentMessageDiagnostics",
                schema: "recommendation");

            migrationBuilder.DropTable(
                name: "RecommendationTurnItemMatchedSignals",
                schema: "recommendation");

            migrationBuilder.DropTable(
                name: "RecommendationTurnItemMissingIngredients",
                schema: "recommendation");

            migrationBuilder.DropTable(
                name: "RecommendationTurnItemRecipeEntries",
                schema: "recommendation");

            migrationBuilder.DropTable(
                name: "AgentMessages",
                schema: "recommendation");

            migrationBuilder.DropTable(
                name: "RecommendationTurnItems",
                schema: "recommendation");

            migrationBuilder.DropTable(
                name: "RecommendationTurnGroups",
                schema: "recommendation");

            migrationBuilder.DropTable(
                name: "AgentRuns",
                schema: "recommendation");

            migrationBuilder.CreateTable(
                name: "ChatTurns",
                schema: "recommendation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    RecommendationGroupsJson = table.Column<string>(type: "jsonb", nullable: false),
                    ToolInvocationsJson = table.Column<string>(type: "jsonb", nullable: false),
                    ExecutionTraceJson = table.Column<string>(type: "jsonb", nullable: true),
                    FeedbackRating = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FeedbackComment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FeedbackCreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatTurns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatTurns_ChatSessions_ChatSessionId",
                        column: x => x.ChatSessionId,
                        principalSchema: "recommendation",
                        principalTable: "ChatSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatTurns_ChatSessionId",
                schema: "recommendation",
                table: "ChatTurns",
                column: "ChatSessionId");
        }
    }
}
