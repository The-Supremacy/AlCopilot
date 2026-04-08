---
description: Sync an ADR into current architecture or workflow guidance
---

Sync an ADR into the repo's current guidance documents.

**Input**: Optionally specify an ADR path or number after `/adr:sync`. If omitted, inspect the latest ADRs and choose the most obviously unsynced candidate when possible; otherwise ask the user to choose.

**Steps**

1. Determine the sync target.
   - If the user specified an ADR, use it.
   - Otherwise inspect the latest 3-5 ADR files in `docs/adr/`, excluding `README.md` and `template.md`.
   - Compare them against current guidance and pick the most obviously unsynced or partially synced candidate.
   - If no single candidate is clearly best, ask the user to choose.
2. Read the ADR and identify its status.
3. Decide what, if anything, should be synced into:
   - `docs/architecture.md`
   - `docs/constitution.md`
   - `openspec/config.yaml`
   - area-specific `AGENTS.md` files when truly local
4. If the ADR is `Deferred`:
   - do not present it as implemented behavior
   - it may be listed in a dedicated deferred/future section of `docs/architecture.md`
5. If the ADR is `Rejected` or `Superseded`, usually do not sync it into active guidance unless a short note is materially helpful.
6. Update only the guidance that is materially affected by the ADR.

**Output**

After syncing, summarize:

- ADR synced
- Files updated
- Whether the ADR is reflected as active guidance or deferred guidance only
- If no changes were needed, say that the ADR already appears sufficiently synced

**Guardrails**

- Do not copy the ADR verbatim into current docs.
- Do not treat `Deferred` ADRs as active system behavior.
- Keep current guidance concise and leave detailed reasoning in the ADR itself.
