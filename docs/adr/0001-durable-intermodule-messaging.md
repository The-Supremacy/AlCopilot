# ADR 0001: Durable Intermodule Messaging

## Status

Deferred

## Date

2026-04-08

## Context

AlCopilot is currently a modular monolith with no concrete cross-module subscriber workflow that requires durable asynchronous delivery.
The repository briefly implemented a Host-level outbox worker, transport wiring, and supporting specs before there was a real business or operational need for them.
That created speculative code and future-facing specs that the project does not want to maintain.

We still want to record the preferred direction for future durable intermodule messaging so the decision and rationale are not lost.

## Decision

Durable intermodule messaging is not implemented today.
If a concrete async use case is approved later, the preferred direction is:

- `DomainEventRecord` rows remain the persisted event source.
- A dedicated outbox or publishing flow may be added on top of those records.
- Rebus is the preferred transport abstraction.
- Azure Service Bus is the preferred production transport.

Until such a use case exists:

- OpenSpec capability specs SHALL NOT describe durable intermodule messaging as a supported behavior.
- The runtime SHALL NOT include speculative outbox workers, transport wiring, broker emulator setup, or transport-specific tests.
- Cross-module communication SHALL use synchronous in-process contracts through Mediator unless and until a concrete async use case is approved.

## Reason

This ADR is `Deferred` because the architectural direction is worth keeping, but there is no current business or operational need that justifies implementation.
Reconsider this ADR when a real publisher and at least one meaningful consumer require eventual consistency across module boundaries or when a module extraction makes durable out-of-process communication necessary.

## Consequences

- The codebase stays free of dead or speculative messaging infrastructure.
- The architecture keeps a documented future direction without pretending the capability exists today.
- When async messaging is introduced later, the team will revisit the design with current requirements instead of inheriting frozen assumptions.

## Alternatives Considered

### Keep the speculative implementation

Rejected.
There is no current consumer, no current business intent, and no appetite for maintaining dead code.

### Keep OpenSpec capability specs but remove the code

Rejected.
The specs would still imply supported or intended behavior without a present use case.
This is architecture direction, not a current capability.

### Forget the decision entirely

Rejected.
The preferred direction is still useful and should remain discoverable as an architectural decision.
