using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Shouldly;
using System.Text.Json;

namespace AlCopilot.Recommendation.IntegrationTests.Integration;

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
            """
            TRUNCATE TABLE
                recommendation."AgentMessageDiagnostics",
                recommendation."RecommendationTurnItemMatchedSignals",
                recommendation."RecommendationTurnItemMissingIngredients",
                recommendation."RecommendationTurnItemRecipeEntries",
                recommendation."RecommendationTurnItems",
                recommendation."RecommendationTurnGroups",
                recommendation."AgentMessages",
                recommendation."AgentRuns",
                recommendation."DomainEventRecords",
                recommendation."ChatSessions"
            CASCADE;
            """);
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task SaveAndReload_PersistsTurnsInOrder()
    {
        var repository = new ChatSessionRepository(_db);
        var session = ChatSession.Create("customer-1", "Something refreshing");
        AppendMessage(session, "user", "Something refreshing");
        var assistantMessage = AppendMessage(
            session,
            "assistant",
            "Try a Gimlet.");

        repository.Add(session);
        var run = AddRun(session);
        _db.RecommendationTurnGroups.AddRange(RecommendationTurnGroup.CreateMany(
            run.Id,
            [new RecommendationGroupDto("make-now", "Available Now", [new RecommendationItemDto(Guid.NewGuid(), "Gimlet", null, [], [], 100)])]));
        _db.AgentMessageDiagnostics.Add(AgentMessageDiagnostic.Create(
            session.Id,
            run.Id,
            assistantMessage.Id,
            "reasoning",
            "provider.reasoning",
            "Compared the top available citrus drinks before answering.",
            null));
        await _db.SaveChangesAsync();

        var loaded = await repository.GetByCustomerSessionIdAsync("customer-1", session.Id);
        loaded.ShouldNotBeNull();
        var messages = await _db.AgentMessages
            .Where(message => message.ChatSessionId == session.Id)
            .OrderBy(message => message.Sequence)
            .ToListAsync();
        messages.Count.ShouldBe(2);
        messages.Last().Role.ShouldBe("assistant");
        var diagnostic = await _db.AgentMessageDiagnostics.SingleAsync();
        diagnostic.Name.ShouldBe("provider.reasoning");
        diagnostic.Text.ShouldBe(
            "Compared the top available citrus drinks before answering.");
        var domainEvents = await _db.DomainEventRecords
            .OrderBy(record => record.Id)
            .ToListAsync();
        domainEvents.Count.ShouldBe(1);
        domainEvents.Select(record => record.EventType)
            .ShouldBe(
            [
                "recommendation.session-started.v1",
            ]);
    }

    [Fact]
    public async Task SaveExistingSession_PersistsSerializedAgentState_AndAdditionalTurns()
    {
        var repository = new ChatSessionRepository(_db);
        var session = ChatSession.Create("customer-1", "Something refreshing");
        AppendMessage(session, "user", "Something refreshing");
        AppendMessage(session, "assistant", "Try a Gimlet.");

        repository.Add(session);
        await _db.SaveChangesAsync();

        var reloaded = await repository.GetByCustomerSessionIdAsync("customer-1", session.Id);
        reloaded.ShouldNotBeNull();

        reloaded!.UpdateAgentSessionState("""{"stateBag":{"session":"restored"}}""");
        AppendMessage(reloaded, "user", "Something bitter");
        AppendMessage(reloaded, "assistant", "Try a Negroni.");
        await _db.SaveChangesAsync();

        var persisted = await repository.GetByCustomerSessionIdAsync("customer-1", session.Id);
        persisted.ShouldNotBeNull();
        persisted!.AgentSessionStateJson.ShouldBe("""{"stateBag":{"session":"restored"}}""");
        var persistedMessages = await _db.AgentMessages
            .Where(message => message.ChatSessionId == session.Id)
            .OrderBy(message => message.Sequence)
            .ToListAsync();
        persistedMessages.Count.ShouldBe(4);
        persistedMessages.Select(message => message.Role)
            .ShouldBe(["user", "assistant", "user", "assistant"]);
    }

    [Fact]
    public async Task RecordTurnFeedback_PersistsFeedbackOnAssistantTurn()
    {
        var repository = new ChatSessionRepository(_db);
        var session = ChatSession.Create("customer-1", "Something refreshing");
        AppendMessage(session, "user", "Something refreshing");
        var assistantMessage = AppendMessage(session, "assistant", "Try a Gimlet.");

        repository.Add(session);
        await _db.SaveChangesAsync();

        var reloadedMessage = await _db.AgentMessages.SingleAsync(message => message.Id == assistantMessage.Id);
        reloadedMessage.RecordFeedback("negative", "Missed the prosecco request.");
        await _db.SaveChangesAsync();

        var persisted = await _db.AgentMessages.SingleAsync(message => message.Id == assistantMessage.Id);
        persisted.FeedbackRating.ShouldBe("negative");
        persisted.FeedbackComment.ShouldBe("Missed the prosecco request.");
    }

    private AgentMessage AppendMessage(
        ChatSession session,
        string role,
        string content)
    {
        var maxPendingSequence = _db.ChangeTracker
            .Entries<AgentMessage>()
            .Where(entry => entry.Entity.ChatSessionId == session.Id)
            .Select(entry => entry.Entity.Sequence)
            .DefaultIfEmpty(0)
            .Max();
        var maxPersistedSequence = _db.AgentMessages
            .Where(message => message.ChatSessionId == session.Id)
            .Select(message => (int?)message.Sequence)
            .Max() ?? 0;
        var sequence = Math.Max(maxPendingSequence, maxPersistedSequence) + 1;
        var chatRole = string.Equals(role, "assistant", StringComparison.Ordinal)
            ? ChatRole.Assistant
            : ChatRole.User;
        var chatMessage = new ChatMessage(chatRole, content)
        {
            MessageId = Guid.NewGuid().ToString("N"),
        };
        var agentMessage = AgentMessage.Create(
            session.Id,
            null,
            sequence,
            chatMessage.MessageId,
            role,
            "text",
            "test",
            content,
            JsonSerializer.Serialize(chatMessage, AIJsonUtilities.DefaultOptions));

        _db.AgentMessages.Add(agentMessage);
        return agentMessage;
    }

    private AgentRun AddRun(ChatSession session)
    {
        var run = AgentRun.Start(session.Id);
        run.Complete(null, null, "stop", null);
        _db.AgentRuns.Add(run);
        return run;
    }
}
