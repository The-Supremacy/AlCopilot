using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Domain.Events;

public sealed record DrinkCreatedEvent(Guid DrinkId) : IDomainEvent
{
    public Guid AggregateId => DrinkId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}

public sealed record DrinkDeletedEvent(Guid DrinkId, DateTimeOffset DeletedAtUtc) : IDomainEvent
{
    public Guid AggregateId => DrinkId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
