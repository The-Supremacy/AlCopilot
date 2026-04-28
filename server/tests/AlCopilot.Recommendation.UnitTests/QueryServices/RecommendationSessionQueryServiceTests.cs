using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AlCopilot.Recommendation.UnitTests.QueryServices;

public sealed class RecommendationSessionQueryServiceTests
{
    [Fact]
    public async Task GetSessionAsync_ProjectsVisibleTurnsFromPersistedTextAgentMessages()
    {
        await using var dbContext = CreateDbContext();
        var session = ChatSession.Create("customer-1", "Something citrusy");
        var runId = Guid.NewGuid();
        dbContext.ChatSessions.Add(session);
        dbContext.AgentMessages.AddRange(
            CreateAgentMessage(session.Id, runId, 1, "user", "text", "Something citrusy"),
            CreateAgentMessage(session.Id, runId, 2, "assistant", "text", "Try the Gimlet."),
            CreateAgentMessage(session.Id, runId, 3, "assistant", "reasoning", "Private scratchpad-style content."),
            CreateAgentMessage(session.Id, runId, 4, "tool", "tool-result", "{}"));
        dbContext.RecommendationTurnGroups.AddRange(
            RecommendationTurnGroup.CreateMany(
                runId,
                [new RecommendationGroupDto("make-now", "Available Now", [])]));
        await dbContext.SaveChangesAsync();

        var queryService = new RecommendationSessionQueryService(dbContext);

        var result = await queryService.GetSessionAsync("customer-1", session.Id);

        result.ShouldNotBeNull();
        result.Turns.Select(turn => (turn.Sequence, turn.Role, turn.Content)).ShouldBe(
        [
            (1, "user", "Something citrusy"),
            (2, "assistant", "Try the Gimlet."),
        ]);
        result.Turns.Last().RecommendationGroups.Select(group => group.Key).ShouldBe(["make-now"]);
    }

    private static RecommendationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<RecommendationDbContext>()
            .UseInMemoryDatabase($"recommendation-session-query-service-{Guid.NewGuid():N}")
            .Options;

        return new RecommendationDbContext(options);
    }

    private static AgentMessage CreateAgentMessage(
        Guid sessionId,
        Guid agentRunId,
        int sequence,
        string role,
        string kind,
        string? text) =>
        AgentMessage.Create(
            sessionId,
            agentRunId,
            sequence,
            Guid.NewGuid().ToString("N"),
            role,
            kind,
            "maf",
            text,
            "{}");
}
