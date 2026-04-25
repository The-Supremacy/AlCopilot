# ADR 0005: Testing Strategy And Shared Integration Harness

## Status

Superseded

## Date

2026-04-09

## Context

The project needed a clearer testing taxonomy, cleaner ownership rules for module versus host integration tests, and a consistent way to bootstrap backend integration tests.
The earlier guidance did not cleanly distinguish module-owned HTTP coverage from host-owned integration concerns, and backend test infrastructure risked drifting into one-off fixture patterns per project.

## Decision

Adopt the backend taxonomy `Architecture`, `Unit`, `Application`, `Infrastructure Integration`, and `Host Integration`.
Keep `AlCopilot.{Module}.Tests` as the default home for unit, application, infrastructure integration, and module-owned HTTP integration tests.
Reserve `AlCopilot.Host.Tests` for host-owned, cross-module, orchestration, and future choreography or eventual-consistency scenarios.
Mock or substitute other modules by default in module-owned tests.
Create a shared backend integration harness project under `server/tests/` to standardize host bootstrapping, PostgreSQL Testcontainers lifecycle, configuration overrides, auth setup, client creation, and future polling helpers.
Adopt Testcontainers with real PostgreSQL as the default for backend persistence and infrastructure integration tests.

## Reason

This status is accepted because the testing strategy and reusable harness direction are being implemented now.
The chosen model keeps test ownership aligned with modules, avoids turning `AlCopilot.Host.Tests` into a catch-all suite, and provides a single reusable backbone for HTTP-backed backend integration tests.

## Consequences

- Module test projects remain the default mixed backend test projects until runtime proves a split is necessary.
- Host tests stay smaller and more focused on host and cross-module risk.
- Backend integration coverage avoids EF in-memory and SQLite-as-PostgreSQL substitutes.
- Shared backend harness code becomes a maintained testing surface that multiple test projects rely on.
- Choreography-heavy and eventual-consistency testing remain future-focused and may require additional polling helpers and execution lanes later.

## Alternatives Considered

### Put All HTTP Integration Tests In Host Tests

Rejected because it makes host tests a catch-all suite and weakens module ownership of module-owned API behavior.

### Split Fast And Infrastructure Backend Tests Into Separate Projects Immediately

Rejected for now because the project can keep a simpler default structure until real runtime pressure justifies a split.

### Use In-Memory Or SQLite Persistence Substitutes For Integration Coverage

Rejected because those options do not give trustworthy PostgreSQL behavior for persistence and infrastructure testing.

## Supersedes

[ADR 0017: Semantic Backend Test Project Split](0017-semantic-backend-test-project-split.md)

## Superseded by

None.
