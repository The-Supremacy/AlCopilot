# Rebus + Azure Service Bus Setup

## Context

Rebus is the chosen messaging library for cross-module async communication. Azure Service Bus is the transport. This task covers the initial Rebus setup before any consumers exist.

## Prerequisites

- Azure Service Bus namespace provisioned (or local emulator configured in Aspire AppHost)
- Decision on which events to publish (at least one cross-module consumer planned)

## Architecture Decisions

- **Rebus** (not MassTransit, not Wolverine) — simple, transport-agnostic, clean JSON messages
- **Azure Service Bus** as transport — topics + subscriptions for pub/sub
- **Topic per event type** using `[DomainEventName]` logical names (e.g., `drink-catalog.drink-created.v1`)
- Custom `IMessageTypeNameConvention` maps event types to logical names via `DomainEventTypeRegistry`
- **Single Rebus bus instance** on Host — all modules publish through it
- Azure Service Bus emulator for local development (orchestrated by Aspire)

## Tasks

### 1. Add Rebus NuGet packages

In `Directory.Packages.props`:

- `Rebus`
- `Rebus.ServiceProvider`
- `Rebus.AzureServiceBus`

### 2. Configure Rebus in Host

In `AlCopilot.Host` startup:

```csharp
services.AddRebus(configure => configure
    .Transport(t => t.UseAzureServiceBus(connectionString, "alcopilot-host"))
    .Serialization(s => s.UseSystemTextJson())
    .Options(o => o.SetMessageTypeNameConvention(...)));
```

The `IMessageTypeNameConvention` should use `DomainEventTypeRegistry` for bidirectional type ↔ name resolution.

### 3. Configure local emulator in AppHost

Add Azure Service Bus emulator resource to Aspire AppHost for local development. Pass connection string to Host.

### 4. Verify message type naming

- Ensure Rebus publishes with topic name `drink-catalog.drink-created.v1` (not CLR type name)
- Verify messages are clean JSON (no Rebus envelope wrapping the payload)
- Metadata (`rbs2-*`) should travel in transport headers only

### 5. Tests

**Unit tests:**

- `DomainEventTypeRegistry` resolves names correctly for Rebus `IMessageTypeNameConvention`
- Serialization round-trip: event → JSON → deserialize with correct type

**Integration tests (WebApplicationFactory + TestContainers):**

- Use Azure Service Bus Emulator (`mcr.microsoft.com/azure-messaging/servicebus-emulator`) + MSSQL companion container via TestContainers `GenericContainer`
- Publish an event via Rebus, verify it arrives at a test subscriber with correct topic name and payload
- Verify message headers contain `rbs2-msg-type` with logical name (e.g., `drink-catalog.drink-created.v1`)
- Verify message body is clean JSON (no Rebus envelope wrapping)
- `WebApplicationFactory<Program>` overrides ASB connection string to point at emulator — no transport swapping, no conditional logic in prod code

**Testing transport decision:**

- Rebus's `AddRebus` registers a hosted service plus numerous internal types. There is no clean way to replace just the transport in `WebApplicationFactory` without fragile `RemoveAll` hacks. Using the same ASB transport (via emulator) eliminates the problem entirely.
- CI and local dev both use ASB Emulator containers
- Fast unit tests: mock `IBus` or use `Rebus.Transport.InMem` for handler registration verification only

## Out of Scope

- Actual event consumers (separate task per consuming module)
- Outbox worker (see `outbox-worker.md`)
- Rebus retry policies and dead-letter configuration (add with first real consumer)
