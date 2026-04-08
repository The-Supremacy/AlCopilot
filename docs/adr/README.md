# ADR Conventions

## Purpose

Architecture Decision Records capture significant technical and workflow decisions that matter to the project but do not necessarily map to supported product behavior.
Use ADRs to preserve decision history without turning every architectural idea into an OpenSpec capability.

## When To Use An ADR

Create an ADR when you need to record:

- architectural direction
- dependency or framework selection
- deferred technical direction
- rejected alternatives worth remembering
- workflow or governance decisions that affect how the team operates

Prefer OpenSpec instead when the change defines or modifies supported behavior.

## Statuses

Use one of these statuses:

- `Proposed`
- `Accepted`
- `Deferred`
- `Rejected`
- `Superseded`

`Deferred` means the direction is useful to remember, but the project is intentionally not implementing it yet.
`Rejected` means the alternative was considered and intentionally not chosen.
`Superseded` means a newer ADR replaced the decision.

## Required Structure

Every ADR should include:

1. Title
2. Status
3. Date
4. Context
5. Decision
6. Reason
7. Consequences
8. Alternatives Considered

Use `Supersedes` and `Superseded by` when relevant.

## Mandatory Rules

- All ADRs MUST include an explicit date.
- `Deferred` ADRs MUST explain why the decision is deferred and what should trigger reconsideration.
- `Rejected` ADRs MUST explain why the option was rejected.
- When a decision changes, prefer writing a new ADR and linking the old one rather than rewriting history in place.

## Sync Guidance

ADRs are source records, not OpenSpec-style delta specs.
Do not automatically copy every ADR into `docs/architecture.md` or `docs/constitution.md`.

Instead, sync only when the ADR changes current guidance:

- Update `docs/architecture.md` when the ADR changes the current architectural narrative.
- Update `docs/constitution.md` when the ADR changes team workflow or governance.
- Do not present `Deferred` ADRs as implemented behavior.
