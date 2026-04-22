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

[DomainEventName("drink-catalog.tag-created")]
public sealed record TagCreatedEvent(Guid TagId) : IDomainEvent
{
    public Guid AggregateId => TagId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}

[DomainEventName("drink-catalog.tag-renamed")]
public sealed record TagRenamedEvent(Guid TagId) : IDomainEvent
{
    public Guid AggregateId => TagId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}

[DomainEventName("drink-catalog.ingredient-created")]
public sealed record IngredientCreatedEvent(Guid IngredientId) : IDomainEvent
{
    public Guid AggregateId => IngredientId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}

[DomainEventName("drink-catalog.ingredient-updated")]
public sealed record IngredientUpdatedEvent(Guid IngredientId) : IDomainEvent
{
    public Guid AggregateId => IngredientId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}

[DomainEventName("drink-catalog.import-batch-initialized")]
public sealed record ImportBatchInitializedEvent(Guid BatchId) : IDomainEvent
{
    public Guid AggregateId => BatchId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}

[DomainEventName("drink-catalog.import-batch-prepared")]
public sealed record ImportBatchPreparedEvent(Guid BatchId) : IDomainEvent
{
    public Guid AggregateId => BatchId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}

[DomainEventName("drink-catalog.import-batch-reviewed")]
public sealed record ImportBatchReviewedEvent(Guid BatchId) : IDomainEvent
{
    public Guid AggregateId => BatchId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}

[DomainEventName("drink-catalog.import-batch-completed")]
public sealed record ImportBatchCompletedEvent(Guid BatchId) : IDomainEvent
{
    public Guid AggregateId => BatchId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}

[DomainEventName("drink-catalog.import-batch-cancelled")]
public sealed record ImportBatchCancelledEvent(Guid BatchId) : IDomainEvent
{
    public Guid AggregateId => BatchId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
