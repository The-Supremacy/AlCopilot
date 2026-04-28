## Why

The customer portal currently renders structured recommendation groups as fully expanded blocks under the assistant narration.
That makes the answer feel visually heavy and repeats availability/restock copy that the prose already explains.
We need the structured results to stay useful and auditable while making the chat response easier to scan.

## What Changes

- Update the customer portal recommendation turn UI so assistant narration remains inline and structured recommendations render as a compact progressive-disclosure block.
- Use customer-facing group language where restock-oriented recommendations are labeled `Buy next`.
- Render `Available now` and `Buy next` groups only when they contain recommendation items.
- Keep groups collapsed by default, with each group exposing compact drink rows that can be expanded for recipe, matched-signal, and missing-ingredient details.
- Preserve drink-details responses as resolved drink details rather than scored recommendation lists.
- Keep the implementation aligned with the accepted frontend stack: React, TypeScript, Tailwind CSS, and shadcn/ui primitives already used by `web/apps/web-portal`.
- Use the customer portal design guide as the UI source of truth for this change:
  - `web/apps/web-portal/DESIGN.md`

## Capabilities

### New Capabilities

- None.

### Modified Capabilities

- `recommendation-chat`: customer portal rendering of structured recommendation response groups.

## Impact

- Affected code is expected to stay in `web/apps/web-portal`, primarily the recommendation turn rendering component and colocated tests.
- Impacted portals for UI-affecting work:
  - `web-portal`
- No backend API, persistence, domain-model, or infrastructure change is expected.
- No new dependency is expected unless the implementation chooses to add a local shadcn/Radix accordion primitive; native disclosure controls are acceptable if they meet accessibility and styling requirements.
