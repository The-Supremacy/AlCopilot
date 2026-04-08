# ADR 0002: Domain-Driven Design Patterns for Modules

## Status

Accepted

## Date

2026-04-08

## Context

AlCopilot is a modular monolith with bounded contexts, module-owned schemas, and domain logic that should remain explicit and testable.
The project needs a stable reference for the DDD patterns that are already in use so they are not diluted by ad hoc handler logic or direct persistence shortcuts.

## Decision

Each backend module follows these DDD patterns:

- Aggregates are the primary consistency boundary.
- Repositories load and persist aggregate roots, not partial graphs.
- Value objects are preferred for validated primitives.
- Handlers orchestrate use cases but do not contain domain rules.
- Module `DbContext` implementations serve as the unit of work.
- Domain events are raised by aggregates and processed in a `SaveChangesInterceptor` dispatch-before-commit loop for same-module reactions.
- `DomainEventRecord` rows are persisted for auditability and replay-oriented diagnostics within the current module design.

Cross-module async messaging is not part of this ADR.
That decision is deferred separately in [ADR 0001](0001-durable-intermodule-messaging.md).

## Reason

This ADR is `Accepted` because these patterns already underpin the current backend design and provide the clearest default for maintaining module boundaries, explicit domain logic, and testable persistence behavior.

## Consequences

- Backend code has a clear default structure for domain logic and persistence.
- Review conversations can anchor on a documented pattern instead of preference drift.
- The current event persistence model remains useful without forcing speculative distributed messaging infrastructure into the runtime.

## Alternatives Considered

### Transaction scripts in handlers

Rejected.
This would blur orchestration and domain logic, making behavior harder to test and evolve.

### Direct `DbContext` usage everywhere without repository boundaries

Rejected.
That would weaken aggregate boundaries and make module rules easier to bypass.

### Event sourcing as the default model

Rejected.
The current project does not need event sourcing complexity for its present scope.
