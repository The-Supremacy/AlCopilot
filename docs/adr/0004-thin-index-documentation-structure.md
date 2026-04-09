# ADR 0004: Thin Index Documentation Structure

## Status

Accepted

## Date

2026-04-09

## Context

Root documentation had grown into the main home for architecture and testing detail.
That made the top-level docs longer, more repetitive, and harder to keep aligned with area-specific guidance.
It also pushed too much human-readable detail into files that should primarily orient readers toward the right source.

## Decision

Adopt a thin-index documentation structure.
Keep `docs/architecture.md`, `docs/constitution.md`, and `docs/testing.md` short and navigational.
Move detailed human-readable guidance into area-specific documents under `docs/architecture/`, `docs/constitution/`, and `docs/testing/`.
Treat `AGENTS.md` files as routing and operational guidance rather than the primary long-form source of truth.

## Reason

This status is accepted because the documentation structure is being updated now.
The thin-index model reduces duplication, keeps root docs readable, and gives ADR sync a clearer target when only one area changes.

## Consequences

- Root docs become easier to review and maintain.
- Detailed guidance is written once in area-specific documents.
- ADR sync must target the detailed docs first and update root indexes only when global guidance changes.

## Alternatives Considered

### Keep Detailed Guidance In Root Docs

Rejected because it encourages duplication and makes the root docs harder to keep current.

### Move Most Detail Into AGENTS Files

Rejected because `AGENTS.md` is operational guidance for agents and reviewers, not the preferred long-form human-readable architecture and process reference.

## Supersedes

None.

## Superseded by

None.
