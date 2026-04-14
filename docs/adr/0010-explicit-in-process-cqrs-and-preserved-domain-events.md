# ADR 0010: Explicit In-Process CQRS And Preserved Domain Events

## Status

Accepted

## Date

2026-04-14

## Context

The backend now has enough module-owned management behavior that aggregate repositories, query projections, auditability, and same-module domain reactions need clearer boundaries.

The earlier shape mixed aggregate repositories with read-side DTO projection methods.
That made repository intent less clear and encouraged aggregate repositories to compose read models across other aggregates.

At the same time, the project already raises domain events and wants to preserve them for:

- machine-readable domain history
- future aggregate-level audit timelines
- same-module transactional domain reactions
- future optionality for replay and integration-event projection

The project is still an in-process modular monolith.
Cross-module orchestration currently uses contracts and Mediator, while durable out-of-process messaging remains deferred.

The team needs a backend direction that:

- keeps aggregate repositories command-focused
- allows query handlers to serve consumer-specific read models efficiently
- preserves domain events as versioned module history
- supports same-module synchronous reactions without forcing event infrastructure across module boundaries
- avoids premature generic repository or service-layer abstraction

## Decision

Adopt an explicit in-process CQRS split inside backend modules while preserving versioned domain events as module-owned history and reaction hooks.

Specifically:

- Treat aggregate repositories as command-side only.
- Aggregate repositories load and persist aggregates, including the full child graph required for mutation and invariant enforcement.
- Read-side DTO projection moves to explicit query services rather than aggregate repositories.
- Query handlers depend on query services, not aggregate repositories, when serving contract DTOs or paged read models.
- Cross-aggregate validation may live in handlers or dedicated supporting services, but only introduce a named service when the rule is meaningfully cross-aggregate, reused, or clearer as an explicit policy.
- Keep Mediator as the current cross-module command/query boundary through `.Contracts` projects.
- Preserve domain events in module-owned `domain_events` tables with logical versioned names such as `*.v1`.
- Keep same-module domain event dispatch synchronous and transactional inside the save pipeline.
- Do not treat preserved domain events as a replacement for explicit operator-facing audit records when richer workflow-specific audit is required.

## Reason

This ADR is `Accepted` because it reflects the architecture the backend now needs and the codebase is already implementing.

An explicit in-process CQRS split keeps aggregate repositories honest and prevents read-model composition concerns from diluting command-side DDD boundaries.
At the same time, dedicated query services allow list views, detail views, and other read scenarios to optimize shape and loading independently of aggregate mutation needs.

Preserved versioned domain events provide durable business history and leave room for future replay or integration-event projection without requiring durable messaging today.
Keeping same-module reactions transactional through domain event handlers supports natural domain choreography inside a module while avoiding premature distributed patterns.

## Consequences

- Aggregate repositories should no longer return contract DTOs.
- Query services become the default home for consumer-specific DTO projection and paging logic.
- Handlers remain the primary application-service boundary; supporting domain/application services are introduced selectively rather than universally.
- Domain events must remain intentionally named and versioned because the project wants to preserve them as long-lived history.
- Aggregate audit timelines may be built from preserved domain events, but management workflow audit may still need explicit audit records.
- Cross-module integrity over time remains a separate policy concern and is not solved merely by preserved domain events or in-process deployment.

## Alternatives Considered

### Keep Mixed Repositories For Both Aggregates And Read Models

Rejected.
This makes repositories less honest about their role and encourages read-side coupling across aggregates.

### Introduce A Broad Service Layer For Every Command

Rejected.
This adds indirection without enough benefit and risks repeating earlier .NET over-abstraction patterns.

### Use In-Process Deployment To Blur Cross-Module Boundaries

Rejected.
The system may exploit in-process Mediator orchestration, but module boundaries should still behave like module boundaries rather than shared persistence space.

## Supersedes

None.

## Superseded by

None.
