---
name: design-change
description: Update an existing portal DESIGN.md by filtering proposed ideas into durable UI invariants, while routing behavior changes to OpenSpec and architecture decisions to ADRs.
license: MIT
---

Refine portal design guidance without introducing drift.

## Purpose

This skill is a design-focused exploration-and-update workflow.
It helps convert raw change ideas into durable `DESIGN.md` updates and keeps non-design concerns in the correct artifact system.

## Use This Skill When

- a developer proposes UI changes and wants to know what belongs in `DESIGN.md`
- the team wants to evolve portal-level UI invariants safely
- design docs need updates before `/opsx:apply` for UI-affecting work

## Classification Rule (Critical)

For each proposed change, classify it before editing docs:

- **DESIGN.md**: stable UI invariant (layout zones, IA boundaries, global placement, visual principle)
- **OpenSpec**: feature behavior, flow logic, acceptance scenarios, API-driven outcomes
- **ADR**: architecture/workflow/decision-record level direction
- **Backlog only**: future ideas not yet stable enough for invariants

Only apply `DESIGN.md`-class items to the design document.

## Steps

1. Identify the target portal design guide (`web/apps/<portal>/DESIGN.md`).
2. Read the current design guide and related governance:
   - `docs/architecture/web.md`
   - `docs/constitution/web.md`
   - `openspec/config.yaml`
3. Read relevant active OpenSpec change artifacts if UI-affecting work is in flight.
4. Classify each requested change using the rule above.
5. Update `DESIGN.md` with invariant-only changes.
6. Keep drift controls intact:
   - do not add feature workflow steps
   - do not add API behavior rules
   - keep anti-drift guardrail statements present
7. Provide a handoff summary:
   - what was applied to `DESIGN.md`
   - what should be captured in OpenSpec
   - what should be captured in ADR
   - what was deferred to backlog

## Guardrails

- Do not transform `DESIGN.md` into a feature spec.
- Do not remove stable invariants without explicit replacement rationale.
- If a requested change is not clearly durable, prefer deferring it rather than encoding it as an invariant.
- If no valid invariant changes are identified, explicitly report that no design-doc mutation should occur.
