---
name: adr-sync
description: Sync accepted ADR decisions into current guidance documents when they materially change architecture or workflow. Use after an ADR is created or updated and the user wants the active guidance docs aligned.
license: MIT
compatibility: Local repo workflow
---

Sync an ADR into the repo's current guidance documents.

## Purpose

ADRs are decision records.
They should only be reflected into active guidance when they change how the system or team should operate today.

## Steps

1. Determine the sync target.
   - If the user specified an ADR, use it.
   - If not, inspect the latest 3-5 ADR files in `docs/adr/`, excluding `README.md` and `template.md`.
   - Compare those ADRs against current guidance and find the most obviously unsynced or partially synced candidate.
   - If exactly one candidate is clearly the best target, use it automatically.
   - If multiple candidates are similarly plausible, ask the user to choose.
2. Read the ADR and identify its status.
3. If the ADR is `Deferred`, `Rejected`, or `Superseded`, do not present it as implemented behavior.
4. Decide which current guidance documents need updates:
   - `docs/architecture.md` for current architecture
   - `docs/constitution.md` for workflow/governance
   - `openspec/config.yaml` for artifact-writing rules
   - area-specific `AGENTS.md` files only when the change is truly area-local
5. Update only the guidance that is materially changed by the ADR.
6. Keep the ADR as the decision record and avoid duplicating its full content elsewhere.

## Guardrails

- Do not copy ADRs wholesale into current guidance.
- Do not treat `Deferred` ADRs as active runtime behavior.
- `Deferred` ADRs may be listed in a dedicated deferred/future section of `docs/architecture.md` for discoverability.
- Do not create a deferred catalog in `docs/constitution.md` unless the deferred ADR changes active team workflow.
- Treat an ADR as already synced if its intended guidance is already reflected in the relevant current docs.
- Prefer short references to the ADR over repeated prose.
