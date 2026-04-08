---
description: Create a new architecture decision record
---

Create a new ADR using the repository ADR conventions.

**Input**: The argument after `/adr:new` is the decision title, OR a short description of the decision to record.

**Steps**

1. Read `docs/adr/README.md` and `docs/adr/template.md`.
2. Determine whether an ADR is the right artifact.
   - If the request defines supported product behavior, suggest OpenSpec instead.
   - If the request is architectural, deferred, rejected, or workflow-oriented, continue with ADR creation.
3. If no clear title or decision is provided, ask the user what decision they want to record.
4. Create the next numbered ADR in `docs/adr/` with a stable slug.
5. Fill in the required sections:
   - Status
   - Date
   - Context
   - Decision
   - Reason
   - Consequences
   - Alternatives Considered
6. If the ADR is `Deferred`, include:
   - why it is deferred
   - what should trigger reconsideration
7. After creating the ADR, tell the user whether they should also run `/adr:sync`.

**Output**

After creating the ADR, summarize:

- ADR number and path
- Status
- Short decision summary
- Whether sync is recommended

**Guardrails**

- Do not encode status in the filename.
- Do not use an ADR when OpenSpec is the better fit.
- Do not rewrite history in older ADRs when a new ADR should supersede them.
