# Proposal: Durable Intermodule Communication

## What

Add durable asynchronous intermodule communication to the modular monolith using the existing domain event persistence as an outbox, a single Host-level `OutboxWorker`, and Rebus over Azure Service Bus topics.

This change also aligns the shared domain event persistence model with the agreed outbox design by adding dispatch tracking and module-level outbox source registration.

## Why

The current codebase persists `DomainEventRecord` rows atomically with aggregate state, but those records stop at the database boundary.
There is no durable publisher, no transport wiring, and no way for another module to subscribe asynchronously.

The architecture already chose Mediator for synchronous in-process orchestration and Rebus plus Azure Service Bus for durable cross-module choreography.
Implementing that path now closes the gap between the documented architecture and the current runtime behavior, while preserving module boundaries and at-least-once delivery semantics.

This change also captures the domain-event persistence adjustments that naturally belong to the outbox design.
Those adjustments are part of the same architectural capability, not a separate feature.

## Scope

### In Scope

- **Rebus setup**: add Rebus packages and configure a single Host-level bus instance
- **Azure Service Bus wiring**: configure Aspire AppHost support for the local Azure Service Bus emulator and pass connection details to the Host
- **Outbox alignment**: extend `DomainEventRecord` with `DispatchedAtUtc` and add the undispatched-record index shape needed by the worker
- **Outbox registration**: add shared `OutboxSourceDescriptor` infrastructure and module registration from `AddXxxModule()`
- **Outbox worker**: implement a single `BackgroundService` on `AlCopilot.Host` that polls registered sources, deserializes events through `DomainEventTypeRegistry`, publishes them, and marks them dispatched
- **Message naming**: ensure event type names use logical `[DomainEventName]` values and remain stable across transport boundaries
- **Tests**: add unit and integration coverage for Rebus naming, outbox publishing, dispatch marking, and retry behavior

### Out of Scope

- A broad refactor of repository interfaces or module folder structure
- Per-consumer delivery tracking beyond Azure Service Bus subscriptions
- Saga or process-manager patterns
- Horizontal scaling of the worker beyond a single initial Host instance
- Full poison-message workflows beyond logging and safe retry behavior
- Adding speculative consumers where no real cross-module use case exists yet

## Affected Modules

| Module                | Impact                                                                            |
| --------------------- | --------------------------------------------------------------------------------- |
| **AlCopilot.Shared**  | Primary: outbox source abstractions and domain event record alignment             |
| **AlCopilot.Host**    | Primary: Rebus configuration and `OutboxWorker`                                   |
| **AlCopilot.AppHost** | Primary: local Azure Service Bus emulator orchestration                           |
| **DrinkCatalog**      | Primary initial publisher: register its outbox source and migrate `domain_events` |

## Dependencies

New runtime dependencies are expected in `server/Directory.Packages.props`:

| Package                 | Purpose                     |
| ----------------------- | --------------------------- |
| `Rebus`                 | Core messaging library      |
| `Rebus.ServiceProvider` | DI integration              |
| `Rebus.AzureServiceBus` | Azure Service Bus transport |

The change also depends on:

- Existing `DomainEventTypeRegistry` and `[DomainEventName]` infrastructure
- Existing per-module `DomainEventRecord` persistence
- Azure Service Bus emulator support in local development and test environments

## Risks

- **Cross-cutting infrastructure change**: this touches shared abstractions, Host runtime wiring, AppHost orchestration, and module persistence together
- **Delivery semantics**: at-least-once delivery means consumers must remain idempotent as new subscriptions are introduced
- **Migration churn**: `domain_events` is already mid-transition in local changes, so the migration path needs careful review
- **Test complexity**: integration coverage depends on the Azure Service Bus emulator and companion infrastructure being reliable in CI and local runs

## Notes

- The current repository refactor appears to be an internal design evolution and does not require a separate change at this time.
- The current domain-event persistence changes do belong in this change because they are part of the durable outbox contract.
