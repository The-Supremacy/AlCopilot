using AlCopilot.Shared.Domain;

namespace AlCopilot.CustomerProfile.Contracts.Events;

[DomainEventName("customer-profile.profile-created")]
public sealed record CustomerProfileCreatedEvent(Guid ProfileId, string CustomerId) : IDomainEvent
{
    public Guid AggregateId => ProfileId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}

[DomainEventName("customer-profile.profile-updated")]
public sealed record CustomerProfileUpdatedEvent(Guid ProfileId, string CustomerId) : IDomainEvent
{
    public Guid AggregateId => ProfileId;
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
}
