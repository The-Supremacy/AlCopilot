# Server Tests Conventions

## Reference Order

Read [docs/constitution.md](../../docs/constitution.md) for project-wide workflow rules.
Read [docs/constitution/server.md](../../docs/constitution/server.md) for backend workflow and quality expectations.
Read [docs/architecture.md](../../docs/architecture.md) for architecture intent.
Read [docs/architecture/server.md](../../docs/architecture/server.md) for backend architecture details.
Read [docs/testing.md](../../docs/testing.md) for the project-wide testing index.
Read [docs/testing/server.md](../../docs/testing/server.md) for backend test taxonomy and ownership rules.
Read [server/AGENTS.md](../AGENTS.md) for backend stack and DDD conventions.

## Project Roles

- `AlCopilot.Architecture.Tests` owns architecture and dependency-rule enforcement.
- `AlCopilot.{Module}.UnitTests` owns deterministic module unit and application tests without external infrastructure.
- `AlCopilot.{Module}.IntegrationTests` owns deterministic module tests that use real infrastructure or a real host boundary.
- `AlCopilot.Recommendation.EvalTests` owns live LLM-backed recommendation agent evaluation and eval corpus validation.
- `AlCopilot.Host.IntegrationTests` owns host-level, cross-module, orchestration, and future eventual-consistency tests that remain in the normal integration lane.
- Future `AlCopilot.Host.SystemTests` projects own broad assembled-system workflows, eventual consistency, long-running orchestration, or other host-level suites that are too expensive or environment-sensitive for the required PR lane.
- `AlCopilot.Testing.Shared` owns shared backend integration harness infrastructure.

Do not place architecture tests in `AlCopilot.Host.IntegrationTests`.
Do not add shallow smoke tests to `AlCopilot.Host.IntegrationTests` when lower-level or fuller host-flow tests already prove the same thing.
Do not use `HeavyTests` as a project suffix; name projects by what they prove and assign expensive suites to a separate workflow lane.

## Layout

- Prefer one file per production call or behavior entry point under test.
- Handler tests should usually be one file per handler.
- Test-class-specific helpers may live in the same file as the test class.
- Reusable fixtures, builders, factories, and infrastructure helpers belong in separate shared files and folders.
- Shared host bootstrapping, PostgreSQL containers, and polling helpers should live in `AlCopilot.Testing.Shared` rather than being reimplemented per project.
- Keep folders named by scope and feature so the ownership of a test is obvious.

## Traits And Tools

- Mark real-infrastructure tests with `[Trait("Category", "Integration")]`.
- Mark live LLM-backed agent evaluation tests with `[Trait("Category", "Eval")]`, and keep fast eval fixture validation under `[Trait("Category", "EvalCorpus")]`.
- Use traits for local filtering inside a project, not as the only boundary between required PR checks and opt-in suites.
- Use xUnit, Shouldly, NSubstitute, Testcontainers, and NetArchTest according to the project role.
- Do not use EF Core in-memory provider or SQLite as a stand-in for Postgres behavior.
- Mock other modules by default in module-owned tests.

## Review Focus

When adding or reviewing tests, verify:

- The test lives in the correct project for its scope.
- The test proves a meaningful behavior rather than duplicating another layer.
- File naming matches the handler or behavior under test.
- Shared helpers are shared only when reuse is real, not speculative.
