# Outbox Worker Implementation

## Context

`DomainEventRecord` rows are already persisted atomically with aggregate state during `SaveChangesAsync`. These records serve as the outbox — they just need a worker to pick them up and publish to Rebus.

This task implements the `OutboxWorker` and the module registration pattern for outbox sources.

## Prerequisites

- Rebus + Azure Service Bus transport configured (see `outbox-rebus-setup.md`)
- At least one cross-module integration event consumer exists

## Architecture Decisions

- **Single `OutboxWorker`** `BackgroundService` on Host — not per-module workers
- Each module registers its outbox source during `AddXxxModule()`
- Worker resolves all registered sources and processes them round-robin
- `DomainEventTypeRegistry` handles deserialization (already implemented)
- One Rebus bus instance on Host — all modules publish through it
- Per-source isolation: if one module's source is unavailable, worker skips and continues

## Tasks

### 1. Add `DispatchedAtUtc` to `DomainEventRecord`

Note: this replaces the old `IsPublished` boolean. A nullable timestamp is strictly better — same filter (`WHERE "DispatchedAtUtc" IS NULL`) plus observability (when was it published?).

- Add `DateTimeOffset? DispatchedAtUtc` property to `DomainEventRecord`
- Add partial index `WHERE "DispatchedAtUtc" IS NULL` to each module's EF configuration
- Generate EF migration per module
- Interceptor does NOT set `DispatchedAtUtc` — it stays null (worker sets it after publishing)

### 2. Create `OutboxSource` descriptor

In `AlCopilot.Shared`:

```
public sealed class OutboxSourceDescriptor
{
    public required Type DbContextType { get; init; }
    public required string Schema { get; init; }
    public required string TableName { get; init; }
}
```

### 3. Create `AddOutboxSource` extension

Extension on `IServiceCollection` that registers `OutboxSourceDescriptor` instances (similar to the `DomainEventAssemblyMarker` pattern).

### 4. Wire module registration

Each module calls `services.AddOutboxSource(...)` in its `AddXxxModule()` method:

```csharp
services.AddOutboxSource(new OutboxSourceDescriptor
{
    DbContextType = typeof(DrinkCatalogDbContext),
    Schema = "drink_catalog",
    TableName = "domain_events"
});
```

### 5. Implement `OutboxWorker`

`BackgroundService` in `AlCopilot.Host`:

1. Resolve all `OutboxSourceDescriptor` instances
2. For each source, create a scoped `DbContext` via `IServiceProvider`
3. Query: `WHERE "DispatchedAtUtc" IS NULL ORDER BY "Id" LIMIT {batchSize}`
4. Deserialize each record using `DomainEventTypeRegistry.GetType(eventName)`
5. Publish to Rebus topic
6. Set `DispatchedAtUtc = DateTimeOffset.UtcNow` and save
7. Configurable polling interval (default: 1 second) and batch size (default: 50)

### 6. Error handling

- If Rebus publish fails: log error, skip the record, retry on next poll cycle
- If deserialization fails: log error, skip (consider moving to a poison record state)
- Worker should not crash on individual record failures — isolation per record

### 7. Tests

**Unit tests:**

- `OutboxWorker` processes records and sets `DispatchedAtUtc`
- Worker skips already-dispatched records
- Worker handles deserialization failures gracefully (logs, skips)
- Worker continues processing other sources if one source fails

**Integration tests (WebApplicationFactory + TestContainers):**

- Use Azure Service Bus Emulator (`mcr.microsoft.com/azure-messaging/servicebus-emulator`) + MSSQL companion container via TestContainers `GenericContainer`
- Same transport as production — no transport swapping, no conditional logic in prod code
- End-to-end: save aggregate → interceptor persists `DomainEventRecord` → worker picks up → publishes to ASB Emulator → consumer receives message with correct event type and payload
- Verify `DispatchedAtUtc` is set after successful publish
- Verify failed publish leaves `DispatchedAtUtc` null for retry
- `WebApplicationFactory<Program>` overrides connection strings to point at emulator containers — no prod code changes

**Testing transport decision:**

- Swapping Rebus transport in `WebApplicationFactory` is not clean — `AddRebus` registers numerous internal types with no built-in replace mechanism. Calling it twice adds duplicates. Using the real ASB Emulator avoids transport swapping entirely.
- CI and local dev both use ASB Emulator — zero divergence between test and prod transport
- Fast unit tests: mock `IBus` for handler wiring and outbox logic (no container needed)

## Out of Scope

- Per-consumer delivery tracking (handled by Azure Service Bus subscriptions)
- Competing consumers / horizontal scaling (single worker is sufficient initially)
- Poison message handling beyond logging (add DLQ integration later if needed)
