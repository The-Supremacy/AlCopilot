---
name: backend-add-aggregate-root
description: Add a new aggregate root to an existing backend module. Use when the user wants a new domain aggregate added inside `server/src/Modules/AlCopilot.*`, including the domain type, repository, EF configuration, handlers, contracts, endpoints, registration, migrations, and tests.
license: MIT
---

Add a new aggregate root inside an existing backend module.

## Purpose

This skill provides a repeatable backend delivery workflow for aggregate-root work in the modular monolith.
Use it for tasks similar to adding a new `Drink`, `Ingredient`, `Tag`, or other module-owned aggregate with command-side persistence, read-side queries, endpoints, and verification.

## Use This Skill When

- the user wants a new aggregate root added to an existing backend module
- the change belongs inside `server/src/Modules/AlCopilot.*`
- the work needs a normal backend vertical slice: domain model, repository, EF configuration, handlers, contracts, endpoints, and tests
- the change follows existing DDD and module patterns rather than introducing a new architectural direction

Do not use this skill for:

- cross-module architecture redesign
- new module creation
- pure read-model additions with no new aggregate
- decisions that should first become an ADR or OpenSpec change

## Workflow

1. Read the relevant root and area guidance that already applies through `AGENTS.md`.
2. Inspect the target module for the closest existing aggregate pattern before editing code.
3. Decide whether the task is small enough to stay single-agent.
4. If discovery is needed, keep the main thread clean and delegate exploration:
   - If an `explorer` subagent exists, spawn `explorer` to map the closest aggregate, affected files, registration points, and likely tests.
   - If no such subagent exists, do that exploration in the main thread.
5. Implement the aggregate end to end, preserving module boundaries and command/query separation.
6. Verify the change with the cheapest meaningful tests.
7. If review is useful:
   - If a `reviewer` subagent exists, spawn `reviewer` after implementation to look for regressions, missing tests, and boundary violations.
   - If no such subagent exists, perform that review in the main thread.
8. Return a concise summary of what changed, what was verified, and any residual risks.

## Delegation Rules

Use subagents only when they reduce noise or parallelize independent work.

Spawn `explorer` when:

- the target module has multiple plausible patterns
- the aggregate will touch several files and the dependency path is not obvious
- you want a compressed map of files, handlers, endpoints, and tests before writing code

Spawn `reviewer` when:

- the change is non-trivial
- you want an independent pass on regressions, architecture drift, or testing gaps
- review can happen after implementation without blocking the next coding step

Stay single-agent when:

- the task is a straightforward copy-and-adapt from one existing aggregate pattern
- the work is confined to a small number of files
- the next step depends on immediate local inspection rather than parallel work

Do not assume these subagents exist.
If they are unavailable, follow the same workflow directly in the main thread.

## Implementation Checklist

Use the closest existing aggregate in the target module as the structural template.
For a standard aggregate-root addition, inspect whether the module needs:

- aggregate type under `Features/<Aggregate>/`
- value objects for validated primitives
- repository interface and implementation
- query service interface and implementation for DTO projection paths
- command and query contracts under the module `.Contracts` project
- handlers for create/update/delete/get flows as needed
- endpoint mapping in the module endpoints file
- `DbSet` registration in the module `DbContext`
- EF Core configuration under `Data/Configurations/`
- dependency registration in `*Module.cs`
- migration files if persistence shape changes
- module-owned tests for domain behavior, repositories, handlers, or integration flows
- architecture-test updates only if conventions or coverage lists changed

## Backend Guardrails

- Keep domain logic in aggregates and domain services, not in handlers.
- Repositories stay aggregate-focused and command-side only.
- Query handlers should use query services for DTO projection.
- Keep module boundaries intact and reference other modules through Contracts only.
- Do not introduce speculative abstractions or new architecture patterns just because a new aggregate exists.
- Prefer explicit relational modeling for core aggregates unless existing guidance already allows a narrower exception.
- Add the cheapest tests that prove the new behavior and persistence shape.

## Output Expectations

When using this skill, finish with:

- changed module and aggregate name
- major files or areas added/updated
- tests run or not run
- any follow-up risks, assumptions, or missing decisions
