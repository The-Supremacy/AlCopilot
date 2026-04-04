using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Contracts.Events;

[DomainEventName("drink-catalog.drink-created")]
public sealed record DrinkCreatedEvent(Guid DrinkId) : IDomainEvent
{
    public Guid AggregateId => DrinkId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}

[DomainEventName("drink-catalog.drink-deleted")]
public sealed record DrinkDeletedEvent(Guid DrinkId, DateTimeOffset DeletedAtUtc) : IDomainEvent
{
    public Guid AggregateId => DrinkId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
