---
name: adr-create
description: Create a new architecture decision record using the repo ADR conventions. Use when the user wants to record a technical or workflow decision that is not best represented as an OpenSpec capability.
license: MIT
compatibility: Local repo workflow
---

Create a new ADR under `docs/adr/`.

## Use This Skill When

- the user wants to record an architectural decision
- the user wants to capture a deferred technical direction
- the user wants to record a rejected alternative
- the change affects team workflow or governance more than product behavior

Do not use this skill for changes that should become OpenSpec capabilities.

## Steps

1. Read `docs/adr/README.md` and `docs/adr/template.md`.
2. Determine whether the ADR status should be `Proposed`, `Accepted`, `Deferred`, `Rejected`, or `Superseded`.
3. Create the next numbered ADR file in `docs/adr/` using a stable slug based on the decision title.
4. Fill in the required sections:
   - Status
   - Date
   - Context
   - Decision
   - Reason
   - Consequences
   - Alternatives Considered
5. If the decision changes an older ADR, add `Supersedes` or `Superseded by`.
6. If the ADR is `Deferred`, explicitly state:
   - why it is deferred
   - what should trigger reconsideration
7. After drafting the ADR, tell the user whether related docs should also be synced:
   - `docs/architecture.md`
   - `docs/constitution.md`
   - `openspec/config.yaml`

## Guardrails

- Do not encode status in the filename.
- Do not rewrite older ADRs just to make the timeline cleaner.
- Do not turn a purely architectural decision into an OpenSpec capability.
- Keep the ADR concrete and auditable.
