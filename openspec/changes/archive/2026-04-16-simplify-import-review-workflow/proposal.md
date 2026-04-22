## Why

The current Drink Catalog import workflow mixes validation diagnostics, review rows, conflict markers, and per-row decision submission into one operator experience.
That creates unnecessary backend and frontend complexity for a workflow where most imports will create new catalog items and only occasional imports will modify existing records.

The system needs a simpler workflow now so import review remains human-centered for meaningful catalog changes without forcing row-level conflict-resolution mechanics into the API, the management portal, and the persisted batch model.

## What Changes

- Simplify import workflow semantics so diagnostics describe validity and safety, while review rows describe planned create, update, and skip outcomes.
- Remove separate conflict-marker and per-row decision concepts from the import workflow contract.
- Treat `Apply` as the single explicit operator approval action after batch review.
- Keep validation errors as the blocking condition for apply.
- Require human review for batches that would update existing drinks or ingredients, without introducing row-level approve or reject inputs.
- Add explicit apply-readiness metadata so the system can describe valid non-ready paths without using exceptions as the primary control-flow mechanism.
- Preserve a future-friendly path where fully clean create-only batches may auto-apply, while batches that would modify existing catalog data remain human-gated.
- Remove the current source-fingerprint persistence and duplicate-import override workflow until there is a concrete operational need for it.
- Rename import-processing internals to better distinguish system processing from human review.
- Move orchestration-style services in this workflow to interface-first DI boundaries so handlers can be tested against behavior contracts instead of concrete workflow implementations.
- Update implementation-facing documentation after the behavior change so Drinks Catalog and server architecture guidance no longer describe conflict-summary workflow state.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `catalog-actualize-management`: import lifecycle requirements change from explicit conflict resolution to diagnostics plus batch-level review and apply semantics
- `management-portal-frontend-testing`: import workspace expectations change from row-level decision editing and stored decision gating to review visibility and apply gating without per-row decision state

## Impact

- Drink Catalog import commands, DTOs, persisted batch workflow state, and import initialization/review/apply handlers
- Management portal import workspace, review page behavior, and frontend tests
- Import API contracts shared through the management API client
- Documentation sync for Drinks Catalog module guidance and server architecture guidance after implementation
