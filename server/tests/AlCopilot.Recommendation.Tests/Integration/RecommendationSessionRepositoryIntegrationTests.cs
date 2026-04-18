using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation;
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
            [new RecommendationGroupDto("make-now", "Make Now", [new RecommendationItemDto(Guid.NewGuid(), "Gimlet", null, [], [], 100)])],
            []);

        repository.Add(session);
        await _db.SaveChangesAsync();

        var loaded = await repository.GetByCustomerSessionIdAsync("customer-1", session.Id);
        loaded.ShouldNotBeNull();
        loaded!.Turns.Count.ShouldBe(2);
        loaded.Turns.Last().Role.ShouldBe("assistant");
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
}
