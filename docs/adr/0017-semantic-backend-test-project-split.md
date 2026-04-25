# ADR 0017: Semantic Backend Test Project Split

## Status

Accepted

## Date

2026-04-25

## Context

The backend test suite previously used mixed `AlCopilot.{Module}.Tests` projects for unit, application, and normal integration tests.
That kept the initial structure simple, but it made CI policy depend on broad project names and made it harder to discuss integration tests that are still appropriate for the required PR lane.

The project also has live LLM-backed `EvalTests`, which are already separate and should remain opt-in.
Future broad assembled-system tests need a name that describes what they prove instead of a generic runtime label such as `HeavyTests`.

## Decision

Split backend module tests by semantic test type.

Use:

- `AlCopilot.{Module}.UnitTests` for deterministic unit and application tests without external infrastructure.
- `AlCopilot.{Module}.IntegrationTests` for deterministic module tests that use real infrastructure or a real host boundary.
- `AlCopilot.Host.IntegrationTests` for host-owned auth, middleware, serialization, composition, and cross-module orchestration risks that remain suitable for the normal integration lane.
- `AlCopilot.{Area}.EvalTests` for live LLM or model-backed behavior.
- Future `AlCopilot.Host.SystemTests` for broad assembled-system workflows, eventual consistency, long-running orchestration, or environment-sensitive tests that should not be required on every PR.

Do not use `HeavyTests` as a project suffix.
Heavy is an execution characteristic, not a test type.
Use workflow lanes to decide when to pay for an expensive suite.

## Reason

This status is accepted because the split is being implemented now.
It keeps project names tied to the confidence each suite provides while leaving CI free to choose which semantic suites are required, scheduled, or manually triggered.

## Consequences

- CI can include `*.UnitTests` and normal `*.IntegrationTests` explicitly without including live `*.EvalTests`.
- Integration tests remain first-class PR checks when they are deterministic and targeted.
- Broad future system suites have a clear semantic home without overloading the integration-test label.
- More test projects means more solution entries and project references to maintain.
- Internal module visibility must list both unit and integration test assemblies where tests cover internal implementation details.

## Alternatives Considered

### Keep Mixed Module Test Projects

Rejected because the suite has grown enough that unit and integration ownership should be visible in project names and CI discovery.

### Use HeavyTests For Expensive Suites

Rejected because heavy describes runtime cost rather than what behavior the suite verifies.
The preferred name for broad assembled-system behavior is `SystemTests`.

### Treat All Integration Tests As Heavy

Rejected because targeted real-infrastructure tests catch important persistence, mapping, transaction, and host-boundary regressions and can still be appropriate for the required PR lane.

## Supersedes

[ADR 0005: Testing Strategy And Shared Integration Harness](0005-testing-strategy-and-shared-integration-harness.md)

## Superseded by

None.
