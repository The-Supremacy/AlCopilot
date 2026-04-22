using AlCopilot.Shared.Domain;

namespace AlCopilot.Recommendation.Contracts.Events;

[DomainEventName("recommendation.session-started")]
public sealed record RecommendationSessionStartedEvent(Guid SessionId, string CustomerId) : IDomainEvent
{
    public Guid AggregateId => SessionId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}

[DomainEventName("recommendation.customer-message-recorded")]
public sealed record RecommendationCustomerMessageRecordedEvent(Guid SessionId, Guid TurnId) : IDomainEvent
{
    public Guid AggregateId => SessionId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}

[DomainEventName("recommendation.assistant-message-recorded")]
public sealed record RecommendationAssistantMessageRecordedEvent(Guid SessionId, Guid TurnId) : IDomainEvent
{
    public Guid AggregateId => SessionId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
