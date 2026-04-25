using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AlCopilot.Recommendation.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class RecommendationSessionRepositoryIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private RecommendationDbContext _db = null!;

    public Task InitializeAsync()
    {
        _db = fixture.CreateDbContext();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync(
            "DELETE FROM recommendation.\"ChatTurns\"; DELETE FROM recommendation.\"DomainEventRecords\"; DELETE FROM recommendation.\"ChatSessions\";");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task SaveAndReload_PersistsTurnsInOrder()
    {
        var repository = new ChatSessionRepository(_db);
        var session = ChatSession.Create("customer-1", "Something refreshing");
        session.AppendUserTurn("Something refreshing");
        session.AppendAssistantTurn(
            "Try a Gimlet.",
            [new RecommendationGroupDto("make-now", "Available Now", [new RecommendationItemDto(Guid.NewGuid(), "Gimlet", null, [], [], 100)])],
            [],
            [
                new RecommendationExecutionTraceStep(
                    "agent.run",
                    "completed",
                    "Generated recommendation response.",
                    DateTimeOffset.UtcNow,
                    new Dictionary<string, string?> { ["finishReason"] = "stop" },
                    [],
                    "Compared the top available citrus drinks before answering.")
            ]);

        repository.Add(session);
        await _db.SaveChangesAsync();

        var loaded = await repository.GetByCustomerSessionIdAsync("customer-1", session.Id);
        loaded.ShouldNotBeNull();
        loaded!.Turns.Count.ShouldBe(2);
        loaded.Turns.Last().Role.ShouldBe("assistant");
        loaded.Turns.Last().GetExecutionTraceSteps().Single().StepName.ShouldBe("agent.run");
        loaded.Turns.Last().GetExecutionTraceSteps().Single().Reasoning.ShouldBe(
            "Compared the top available citrus drinks before answering.");
        var domainEvents = await _db.DomainEventRecords
            .OrderBy(record => record.Id)
            .ToListAsync();
        domainEvents.Count.ShouldBe(3);
        domainEvents.Select(record => record.EventType)
            .ShouldBe(
            [
                "recommendation.session-started.v1",
                "recommendation.customer-message-recorded.v1",
                "recommendation.assistant-message-recorded.v1",
            ]);
    }

    [Fact]
    public async Task SaveExistingSession_PersistsSerializedAgentState_AndAdditionalTurns()
    {
        var repository = new ChatSessionRepository(_db);
        var session = ChatSession.Create("customer-1", "Something refreshing");
        session.AppendUserTurn("Something refreshing");
        session.AppendAssistantTurn("Try a Gimlet.", [], []);

        repository.Add(session);
        await _db.SaveChangesAsync();

        var reloaded = await repository.GetByCustomerSessionIdAsync("customer-1", session.Id);
        reloaded.ShouldNotBeNull();

        reloaded!.UpdateAgentSessionState("""{"stateBag":{"session":"restored"}}""");
        reloaded.AppendUserTurn("Something bitter");
        reloaded.AppendAssistantTurn("Try a Negroni.", [], []);
        await _db.SaveChangesAsync();

        var persisted = await repository.GetByCustomerSessionIdAsync("customer-1", session.Id);
        persisted.ShouldNotBeNull();
        persisted!.AgentSessionStateJson.ShouldBe("""{"stateBag":{"session":"restored"}}""");
        persisted.Turns.Count.ShouldBe(4);
        persisted.Turns.Select(turn => turn.Role)
            .ShouldBe(["user", "assistant", "user", "assistant"]);
    }

    [Fact]
    public async Task RecordTurnFeedback_PersistsFeedbackOnAssistantTurn()
    {
        var repository = new ChatSessionRepository(_db);
        var session = ChatSession.Create("customer-1", "Something refreshing");
        session.AppendUserTurn("Something refreshing");
        session.AppendAssistantTurn("Try a Gimlet.", [], []);
        var assistantTurnId = session.Turns.Last().Id;

        repository.Add(session);
        await _db.SaveChangesAsync();

        var reloaded = await repository.GetByCustomerSessionIdAsync("customer-1", session.Id);
        reloaded.ShouldNotBeNull();

        reloaded!.RecordTurnFeedback(assistantTurnId, "negative", "Missed the prosecco request.");
        await _db.SaveChangesAsync();

        var persisted = await repository.GetByCustomerSessionIdAsync("customer-1", session.Id);
        var feedback = persisted!.Turns.Single(turn => turn.Id == assistantTurnId).GetFeedback();
        feedback.ShouldNotBeNull();
        feedback!.Rating.ShouldBe("negative");
        feedback.Comment.ShouldBe("Missed the prosecco request.");
    }
}
