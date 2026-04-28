## Context

The customer portal already receives recommendation-chat turns as conversational prose plus machine-readable recommendation groups.
The existing UI renders those groups immediately under the prose, which keeps the data visible but makes assistant replies feel crowded.
The customer portal design guide now states that recommendation results should keep narration primary and place structured detail behind progressive disclosure:

- `web/apps/web-portal/DESIGN.md`

This is a frontend presentation change using the accepted web stack described by `docs/architecture/web.md` and `docs/constitution/web.md`.

## Goals / Non-Goals

**Goals:**

- Make recommendation replies easier to scan by collapsing structured recommendation detail beneath assistant narration.
- Rename restock-oriented recommendation grouping to `Buy next` in the customer-facing UI.
- Keep availability/restock grouping visible and accessible without relying on color alone.
- Preserve access to recipe entries, matched signals, and missing ingredient details through explicit expansion.
- Cover the changed interaction with focused component tests.

**Non-Goals:**

- Changing recommendation candidate building, scoring, grouping keys, persistence, or API contracts.
- Asking the recommendation backend or model narrator to generate new short-form summaries.
- Introducing a new frontend state library or portal-wide layout pattern.
- Changing management portal behavior.

## Decisions

### Decision 1: Use progressive disclosure inside assistant turns

Assistant turn prose remains inline and visually primary.
Structured recommendation groups render below as a compact titled result block, with group-level disclosure controls for the available-now and buy-next groupings.
Groups are collapsed by default so a user can read the assistant answer first and inspect details only when needed.

### Decision 2: Keep backend group keys stable and change only customer-facing labels

The existing recommendation group keys, such as `make-now` and `buy-next`, remain the behavior contract.
The portal maps those keys to customer-facing labels, including `Available now` and `Buy next`.
This avoids a backend migration for a presentation wording change.

### Decision 3: Preserve item details behind per-drink expansion

Each drink row should show enough collapsed information to identify the drink and understand why it appears.
Expanded drink detail should include recipe entries, matched signals when relevant, and missing ingredients for buy-next items.
Recommendation scores remain hidden, consistent with the customer portal design guide.

## Risks / Trade-offs

- Collapsed-by-default groups can hide useful detail from users who expect everything to be visible. The compact result block and visible group counts mitigate this by making expandable content obvious.
- Native disclosure controls are simple and accessible, but may need careful styling to match the portal. A shadcn/Radix accordion primitive is an option if native controls do not provide enough control.
- Existing tests that assert always-visible detail will need to shift toward interaction-driven assertions.

## Migration Plan

1. Confirm `web/apps/web-portal/DESIGN.md` is updated before implementation.
2. Update `RecommendationTurnList` rendering to introduce the result block, group disclosures, and per-drink disclosures.
3. Update component tests to assert default collapsed state, expansion behavior, conditional group rendering, and hidden scores.
4. Run web portal typecheck/build or the focused Vitest suite before apply closeout.

Rollback strategy:

- Revert the frontend component and test changes to return to the fully expanded structured group presentation.
- No data migration or backend rollback is required.

## Open Questions

- Whether the outer result block title should be `What to make` or `Recommendations` can be decided during implementation. `Recommendations` is safer for mixed response types, while `What to make` is warmer for ordinary drink-pick replies.
