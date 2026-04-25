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

| Type                       | Purpose                                                                                                           | I/O                                          | Typical placement                     |
| -------------------------- | ----------------------------------------------------------------------------------------------------------------- | -------------------------------------------- | ------------------------------------- |
| Architecture               | Enforce modular-monolith boundaries and conventions                                                               | None                                         | `AlCopilot.Architecture.Tests`        |
| Unit                       | Verify isolated domain or application logic                                                                       | None                                         | `AlCopilot.{Module}.UnitTests`        |
| Application                | Verify collaboration across real application and domain classes with external boundaries substituted              | None                                         | `AlCopilot.{Module}.UnitTests`        |
| Infrastructure Integration | Verify repositories, EF mappings, handlers, transactions, and interceptors against real infrastructure below HTTP | Real PostgreSQL and module infrastructure    | `AlCopilot.{Module}.IntegrationTests` |
| Host Integration           | Verify auth, middleware, serialization, composition, orchestration, and other host-owned risks                    | Real HTTP path plus realistic infrastructure | `AlCopilot.Host.IntegrationTests`     |
| Agent Eval                 | Verify real agent prompts, tools, session state, and LLM behavior for high-value scenarios                        | Real configured LLM provider                 | `AlCopilot.{Area}.EvalTests`          |

---

## Project Naming And Execution Lanes

Use project names to describe the kind of confidence a suite provides.
Use CI workflow lanes to decide when to pay for that confidence.

The current backend test project names are:

- `AlCopilot.{Module}.UnitTests` for deterministic module tests without external infrastructure.
- `AlCopilot.{Module}.IntegrationTests` for deterministic module tests that use real infrastructure or a real host boundary.
- `AlCopilot.Host.IntegrationTests` for host-owned auth, middleware, serialization, composition, and orchestration risks that should remain in the normal integration lane.
- `AlCopilot.Architecture.Tests` for architecture rules.
- `AlCopilot.Recommendation.EvalTests` for live recommendation agent evaluation.

If a normal integration project becomes too broad, split by semantic test type rather than by runtime weight:

- `AlCopilot.Host.SystemTests` for broad assembled-system workflows, eventual consistency, long-running orchestration, or other suites that are too expensive or too environment-sensitive for the normal PR path.
- `AlCopilot.{Area}.EvalTests` for live LLM or model-backed behavior.

Do not use `HeavyTests` as a project suffix.
Heavy is an execution characteristic, not a test type.
If a suite is expensive, keep the project name focused on what it proves and move it to the appropriate workflow lane.

---

## Placement Rules

### Module-Owned Behavior

If the behavior is owned by a single module, its tests belong in that module’s test project.
That includes module-owned HTTP integration tests when the endpoint belongs to the module and the real HTTP boundary is valuable to cover.

### Cross-Module Orchestration

If the test proves real cross-module orchestration, put it in `AlCopilot.Host.IntegrationTests`.
Module tests should not become mini-system tests by default.

### Choreography And Eventual Consistency

Choreography and eventual-consistency verification belongs in host-level or heavier integration testing.
These tests may require polling or wait helpers and should not become the default shape of module tests.

---

## Module Boundaries Inside Tests

Module tests should mock or substitute other modules by default.
Use the other module’s contracts-facing boundary rather than its internal implementation types.
If the real interaction across modules is the subject under test, move the test to `AlCopilot.Host.IntegrationTests`.

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

Normal unit, application, architecture, and targeted integration coverage should stay in the main PR path unless the suite becomes materially too slow or unreliable.
`*.UnitTests` and normal `*.IntegrationTests` projects are eligible for the PR lane by default.
Broad `*.SystemTests` projects and live `*.EvalTests` projects are not part of the required PR lane by default.

Move a suite out of the required PR path only when it has a clear reason, such as:

- live LLM calls or other paid/rate-limited external services
- broad cross-module orchestration that takes materially longer than targeted integration tests
- eventual-consistency polling that is useful but not suitable for every PR
- environment-sensitive behavior that would create noisy required checks

Use traits for local filtering inside a project.
Do not rely on traits as the only boundary between required PR checks and opt-in suites.

---

## Agent Eval

Use Agent Framework evaluation when the behavior under test depends on the real agent runtime rather than only deterministic module code.

Recommendation-style prompts that need the real `ChatClientAgent`, provider pipeline, tool surface, and real LLM behavior are good candidates for this layer.

Keep Agent Eval in a dedicated test project so focused unit and integration project runs remain intentional.

Treat it as:

- slow
- explicit
- seeded with stable dedicated test data
- complementary to lower-level regression tests

Keep eval projects in the solution for restore and build coverage.
Run integration and LLM eval projects when the affected logic needs that coverage or before the actual commit.

Use the dedicated guide for structure and rollout expectations:

- [agent-eval.md](agent-eval.md)
