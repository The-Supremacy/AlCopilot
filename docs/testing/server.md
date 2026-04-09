# Server Testing Strategy

## Purpose

This document is the detailed backend testing guide for AlCopilot.
It defines the backend test taxonomy, ownership rules, project placement, and shared harness direction.

---

## Principles

- Prefer the lowest backend test level that gives enough confidence for the behavior under change.
- Keep module-owned behavior in module-owned test projects.
- Use host tests only when the host boundary or real cross-module wiring is part of the risk.
- Use real PostgreSQL through Testcontainers when persistence or infrastructure behavior is under test.

---

## Backend Taxonomy

| Type                       | Purpose                                                                                                           | I/O                                          | Typical placement              |
| -------------------------- | ----------------------------------------------------------------------------------------------------------------- | -------------------------------------------- | ------------------------------ |
| Architecture               | Enforce modular-monolith boundaries and conventions                                                               | None                                         | `AlCopilot.Architecture.Tests` |
| Unit                       | Verify isolated domain or application logic                                                                       | None                                         | `AlCopilot.{Module}.Tests`     |
| Application                | Verify collaboration across real application and domain classes with external boundaries substituted              | None                                         | `AlCopilot.{Module}.Tests`     |
| Infrastructure Integration | Verify repositories, EF mappings, handlers, transactions, and interceptors against real infrastructure below HTTP | Real PostgreSQL and module infrastructure    | `AlCopilot.{Module}.Tests`     |
| Host Integration           | Verify auth, middleware, serialization, composition, orchestration, and other host-owned risks                    | Real HTTP path plus realistic infrastructure | `AlCopilot.Host.Tests`         |

---

## Placement Rules

### Module-Owned Behavior

If the behavior is owned by a single module, its tests belong in that module’s test project.
That includes module-owned HTTP integration tests when the endpoint belongs to the module and the real HTTP boundary is valuable to cover.

### Cross-Module Orchestration

If the test proves real cross-module orchestration, put it in `AlCopilot.Host.Tests`.
Module tests should not become mini-system tests by default.

### Choreography And Eventual Consistency

Choreography and eventual-consistency verification belongs in host-level or heavier integration testing.
These tests may require polling or wait helpers and should not become the default shape of module tests.

---

## Module Boundaries Inside Tests

Module tests should mock or substitute other modules by default.
Use the other module’s contracts-facing boundary rather than its internal implementation types.
If the real interaction across modules is the subject under test, move the test to `AlCopilot.Host.Tests`.

---

## Shared Backend Integration Harness

Backend HTTP integration tests should converge on a shared harness rather than ad hoc per-project bootstraps.
The shared harness lives in `server/tests/AlCopilot.Testing.Shared/`.

The harness standardizes:

- host bootstrapping
- shared `WebApplicationFactory` patterns
- PostgreSQL Testcontainers lifecycle
- configuration overrides
- auth and test identity setup
- client creation
- database reset and seed conventions
- future polling helpers for eventual-consistency tests

Module-owned HTTP integration tests and host integration tests should use the same harness family.

---

## Tools And Rules

- xUnit
- Shouldly
- NSubstitute
- Testcontainers with real PostgreSQL
- NetArchTest.eNhanced for architecture tests

Not used for backend persistence or infrastructure integration coverage:

- EF Core in-memory provider
- SQLite as a stand-in for PostgreSQL behavior
- Host-level smoke tests that only duplicate stronger coverage elsewhere

---

## CI Expectations

`AlCopilot.{Module}.Tests` remains the default mixed backend project until runtime proves a split is necessary.
Normal unit, application, and infrastructure integration coverage should stay in the main PR path unless the suite becomes materially too slow.
Heavier suites may move to separate projects or separate execution lanes later if evidence justifies it.
