# Integration Event Consumers

## Context

When a module needs to react to another module's domain events asynchronously, it subscribes to Rebus topics. This task covers the pattern for implementing integration event consumers.

## Prerequisites

- Rebus configured (see `outbox-rebus-setup.md`)
- Outbox worker running (see `outbox-worker.md`)

## Architecture Decisions

- Consumers subscribe to **Rebus topics** (Azure Service Bus subscriptions)
- Each subscription has independent retry and dead-lettering — handled by the transport
- Consumer handlers must be **idempotent** (at-least-once delivery)
- Consumers use their own module's `DbContext` — separate transaction from the publisher
- Integration event types are defined in the **publishing module's Contracts** project
- Consuming module references the Contracts project to get the event type

## Pattern

### Event in publisher's Contracts

```csharp
// In AlCopilot.DrinkCatalog.Contracts
[DomainEventName("drink-catalog.drink-created")]
public sealed record DrinkCreatedEvent(Guid DrinkId) : IDomainEvent { ... }
```

### Consumer in subscribing module

```csharp
// In AlCopilot.Recommendation (references DrinkCatalog.Contracts)
public sealed class OnDrinkCreated : IHandleMessages<DrinkCreatedEvent>
{
    public async Task Handle(DrinkCreatedEvent message) { ... }
}
```

### Module registration

```csharp
// In AddRecommendationModule()
services.AutoRegisterHandlersFromAssembly(typeof(RecommendationModule).Assembly);
```

Rebus auto-discovers `IHandleMessages<T>` implementations and creates Azure Service Bus subscriptions.

## Idempotency

Consumers must handle being called more than once for the same event. Strategies:

- **Natural idempotency**: `INSERT ... ON CONFLICT DO NOTHING` or upsert logic
- **Idempotency key**: Store processed event IDs in a `processed_events` table, skip duplicates
- **Last-write-wins**: If the operation is inherently idempotent (updating a cached value), no special handling needed

Choose per consumer based on the operation's nature.

## Tests

**Unit tests:**

- Consumer handler receives event and performs expected operation
- Consumer handles duplicate events idempotently (second call is a no-op)

**Integration tests (WebApplicationFactory + TestContainers):**

- Azure Service Bus Emulator + MSSQL companion container via TestContainers `GenericContainer`
- End-to-end: publisher saves aggregate → outbox worker publishes to ASB Emulator → consumer receives and persists result in its own DbContext
- Verify consumer's data is correct after processing
- Verify duplicate delivery doesn't create duplicate data
- Verify failed consumer leaves message for retry (not acknowledged)
- `WebApplicationFactory<Program>` overrides ASB connection string to emulator — same transport as prod, zero conditional logic

**Testing transport:** Azure Service Bus Emulator everywhere (CI and local dev). No transport swapping in prod code.

## Dead-Letter Handling

Azure Service Bus moves messages to DLQ after max delivery attempts (configurable). Monitor DLQ via:

- Azure Portal alerts on DLQ depth
- Future: a DLQ processor that logs / alerts / retries with manual intervention

## Trigger Point

Implement this when the first real cross-module consumer is needed. Do not add Rebus infrastructure speculatively.

## Out of Scope

- Saga / process manager patterns (add if a multi-step cross-module workflow appears)
- Competing consumers (single instance per subscription is sufficient initially)
