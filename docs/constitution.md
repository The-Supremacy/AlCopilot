# AlCopilot Constitution

## Purpose

This document is the project-wide governance source of truth for AlCopilot.
Use it together with [architecture.md](architecture.md), not as a replacement for it.
When guidance is duplicated elsewhere, prefer this document and remove the duplicate when practical.

## Project Identity

AlCopilot is an AI-powered drinks suggestion platform built as a modular monolith.
The product should stay approachable, practical, and grounded in clear user value rather than novelty for its own sake.

## Delivery Workflow

Work is plan-first by default.
Non-trivial work starts with alignment before implementation.
Break work into small, reviewable steps.
Get explicit approval before proceeding with non-trivial implementation.
Use OpenSpec as the default workflow for changes that affect behavior, architecture, or delivery expectations.

## Decision Records

Use ADRs under `docs/adr/` for architectural decisions, deferred technical direction, and major workflow choices that do not describe supported product behavior on their own.
Prefer creating a new ADR that supersedes an older one over rewriting decision history in place.
Update architecture or workflow documents only when an ADR changes current guidance, not merely because an ADR exists.
Follow the ADR structure and status rules in `docs/adr/README.md`.

## OpenSpec Artifact Boundaries

Proposal artifacts capture intent.
Every proposal should explain the problem being solved, the desired outcome, and why the change matters now.
Specification artifacts capture observable behavior and acceptance criteria.
Design artifacts capture technical intent, structure, trade-offs, and implementation approach.
Task artifacts capture the concrete implementation sequence.
If a change is purely architectural or deferred and does not define supported behavior, prefer an ADR over an OpenSpec capability change.
Do not compensate for a weak proposal by stuffing motivation into specs.
Do not compensate for a weak spec by inventing requirements during implementation.

## Architecture Principles

Honor the design decisions in [architecture.md](architecture.md).
Preserve modular-monolith boundaries unless a change explicitly revisits them.
Each module owns its own data model, `DbContext`, and database schema.
Cross-module communication should follow the documented contracts, mediator, and integration-event patterns rather than ad hoc coupling.
The Host remains the BFF and the external integration boundary for the web application.

## Dependency and Tooling Principles

Prefer the approved stack already documented for the project.
Do not introduce dependencies that contradict architectural decisions without an explicit documented decision.
Favor permissive licensing and verify new dependencies before adoption.
Prefer simple, inspectable project guidance over opaque or high-risk automation.

## Quality Principles

Every change should preserve clear module boundaries, testability, and operational simplicity.
Specification scenarios should be implemented with matching verification, not treated as aspirational prose.
Changes that affect architecture boundaries should update architecture tests when applicable.
Testing guidance remains area-specific, but the baseline expectation is that important behavior is covered before archive.

## Documentation Principles

Avoid duplicating architecture and governance text across multiple files.
Reference the source-of-truth document instead of restating it unless local context genuinely needs a short summary.
Keep decisions honest, concrete, and easy to audit later.
When documenting brownfield changes, prefer coherent end-state descriptions over fragmented historical notes.

## Decision Precedence

Use this order when instructions overlap:

1. This constitution.
2. [architecture.md](architecture.md).
3. Root [AGENTS.md](../AGENTS.md).
4. Area-specific `AGENTS.md` files.
5. OpenSpec artifact-specific rules in [openspec/config.yaml](../openspec/config.yaml).
