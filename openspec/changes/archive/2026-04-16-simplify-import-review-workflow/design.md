## Context

The Drink Catalog import flow currently treats a batch as a mix of validation output, review snapshot, conflict markers, and row-level operator decisions.
That shape spans the `ImportBatch` aggregate, import DTOs, apply command input, management portal local state, and frontend tests.

The current workflow does not match the real operational intent very well.
Most imports are expected to create new drinks, ingredients, and tags, while updates to existing catalog data should remain visible and human-reviewed without forcing per-row approve or reject mechanics.

This change affects both backend and frontend behavior.
On the backend, it changes the import workflow contract owned by the Drink Catalog module.
On the frontend, it changes how the management portal review workspace is rendered and how apply gating is determined using the accepted React, TanStack Query, TanStack Router, and Zustand stack.

Affected domain model:

- Aggregate root: `ImportBatch`
- Referenced aggregates during apply: `Drink`, `Ingredient`, `Tag`
- Relevant workflow records/value-like types: `ImportDiagnostic`, `ImportReviewRow`, `ImportReviewSummary`, `ImportApplySummary`, `ImportBatchApplyReadiness`
- Domain events: existing aggregate domain events remain unchanged and are not the workflow mechanism for import approval

## Goals / Non-Goals

**Goals:**

- Separate validation diagnostics from review semantics cleanly.
- Preserve review rows as the operator-facing list of planned create, update, and skip outcomes.
- Remove row-level conflict markers and row-level apply decisions from the backend contract and management portal workflow.
- Make `Apply` the single explicit operator approval action after review.
- Keep validation errors as the blocking condition for apply.
- Keep batches that would update existing records human-reviewed without introducing a new persisted approval state.
- Expose explicit apply-readiness metadata so valid non-ready paths are represented as workflow state instead of exceptions.
- Remove duplicate-import fingerprint persistence and override workflow for now.
- Clarify naming so system processing and human review are not both called “review.”
- Introduce interface-first DI boundaries for orchestration-style services used directly by handlers.
- Leave room for future automation where clean create-only batches may auto-apply.

**Non-Goals:**

- Adding in-review editing of imported values.
- Designing recurring or scheduled import automation.
- Changing the management portal `DESIGN.md`.
- Renaming `AlCopilot.AppHost` in this change.
- Changing domain-event infrastructure beyond what existing import behavior already uses.

## Decisions

### Decision 1: Replace conflict semantics with review-required update semantics

The workflow will no longer model a separate `conflict` concept for imports.
Instead, review rows remain the canonical representation of planned changes, and rows with `update` actions represent changes to existing catalog data that require operator review.

Why:

- Existing-record updates are the real reason for operator caution.
- The current `conflict` term overstates the situation and forces extra API and UI state for what is effectively “please review this update.”
- A review row already carries the target and planned action, so a separate conflict list is redundant.

Alternative considered:

- Keep separate conflict markers and rename them only.
  Rejected because it preserves duplicate workflow state and most of the current UI/API complexity.

### Decision 2: Keep diagnostics focused on validity and safety only

Diagnostics will remain persisted import findings with codes, messages, severities, and optional row/target context.
They will not encode review workflow state.

Why:

- Diagnostics answer “is this import valid or suspicious?”
- Review rows answer “what would happen if we apply?”
- Keeping those concerns separate makes both API and UI easier to reason about.

Alternative considered:

- Collapse review and diagnostics into one list of messages.
  Rejected because operators still need a structured create/update/skip review workspace.

### Decision 3: `Apply` is the approval action and no separate approved state is introduced

The system will not add a persisted “approved” lifecycle state or a separate approval command.
Operators review the batch on the review page and then apply it.

Why:

- This preserves a simple lifecycle: `InProgress` -> `Completed` or `Cancelled`.
- It avoids introducing extra persisted workflow state that provides little value for the current management use case.
- It aligns with the desired operator experience of “inspect the batch, then approve by applying.”

Alternative considered:

- Add a distinct batch approval step before apply.
  Rejected because it complicates lifecycle state and APIs without solving a current operational problem.

### Decision 4: Explicit apply readiness models valid non-ready paths

The import workflow will expose explicit apply-readiness metadata such as `Ready`, `RequiresReview`, or `BlockedByValidationErrors`.
Applying a batch in a valid but non-ready state must not depend on exceptions as the primary business-flow mechanism.

Why:

- `RequiresReview` and validation blocking are normal workflow outcomes, not system faults.
- The management portal can gate actions and messaging more clearly when readiness is first-class metadata.
- A result-style apply response can return the current batch plus readiness metadata without mutating batch lifecycle state.

Alternative considered:

- Keep using exceptions for review-required and validation-blocked apply attempts.
  Rejected because it treats expected workflow branches as errors and makes API/handler behavior less honest.

### Decision 5: Create-only clean batches remain directly applicable, while update-containing batches are human-gated

Batches with no blocking diagnostics and no planned updates to existing catalog data may be applied directly.
Batches with planned updates remain valid but require human inspection before apply.
The spec will describe this as behavior, while implementation may expose an explicit readiness signal if that materially simplifies the API and UI.

Why:

- This matches the expected dominant import mode.
- It keeps the future auto-apply rule straightforward: only clean create-only batches may auto-apply.
- It avoids encoding future automation policy as a separate manual-decision system today.

Alternative considered:

- Make all imports create-only.
  Rejected because correcting or refreshing existing catalog data through import is still a valid management need.

### Decision 6: Remove fingerprint persistence and duplicate-override workflow for now

The import workflow will stop persisting source fingerprints and will remove the current duplicate-import override path from the active contract.

Why:

- The import payload and import semantics are still changing rapidly.
- Persisted fingerprint values add workflow and schema surface without protecting a stable operational path yet.
- Duplicate detection can be reintroduced later when automation and payload stability make it materially useful.

Alternative considered:

- Keep persisting fingerprints as passive metadata while removing only the override parameter.
  Rejected because it leaves behind workflow-adjacent state that the product is not using yet.

### Decision 7: Remove per-row decision storage from the management portal

The management portal will stop storing row-level import decisions in Zustand and will render the review page as an inspection-first workspace.
Frontend tests will prove review visibility, refresh behavior, and apply gating without local decision state.

Why:

- The existing per-row decision store exists only to support a workflow that the backend no longer needs.
- Removing it reduces divergence risk between UI state and persisted batch state.
- It keeps the portal aligned with the accepted frontend stack while simplifying state boundaries.

Alternative considered:

- Keep local row decisions but submit them only as optional metadata.
  Rejected because it preserves unnecessary state management and weakens the simplified operator model.

### Decision 8: Use interface-first DI for orchestration-style services

Handlers in this workflow will depend on interfaces for orchestration-style collaborators such as import processing and audit writing.
Aggregate repositories and aggregate methods remain the natural exception because they already express domain boundaries directly.

Why:

- These collaborators are stable behavior boundaries that handlers coordinate through.
- Interface-first DI keeps handler tests focused on behavior contracts rather than concrete workflow implementations.
- It matches the intended backend style for application services better than injecting concrete orchestration classes directly.

Alternative considered:

- Keep injecting concrete types and rely on integration tests for most coverage.
  Rejected because it makes handler-level isolation harder than necessary and obscures collaboration boundaries.

### Decision 9: Rename import-processing internals to distinguish system processing from human review

System-side processing that validates and matches imported data to existing catalog state will use processing-oriented names rather than review-oriented names.
Human-triggered review handlers keep review-oriented naming.

Why:

- The previous naming reused “review” for both automated processing and operator inspection.
- Clearer names better reflect the simplified workflow and reduce drift back toward the older conflict-resolution model.

## Risks / Trade-offs

- [Operators may want richer review metadata than create/update/skip] -> Keep review rows extensible with clear summaries and optional review-required hints rather than reintroducing row decisions now.
- [The distinction between “review required” and “blocking error” could become unclear in UI copy] -> Make API and portal messaging explicitly separate validation diagnostics from update review.
- [Introducing readiness/result metadata could create duplicate state if overdone] -> Keep readiness derived from batch state and use result wrappers only where a command needs to report an un-applied but valid outcome.
- [Removing conflict-specific workflow state changes persisted batch shape] -> Migrate additive-first where possible and update import-history rendering and tests together.
- [Interface-first extraction could be applied too broadly] -> Limit it to orchestration-style collaborators and document the rule in `server/AGENTS.md`.

## Migration Plan

1. Update OpenSpec deltas for `catalog-actualize-management` and `management-portal-frontend-testing`.
2. Remove conflict-list, decision-input usage, and source-fingerprint persistence from the Drink Catalog import workflow contract and management portal behavior.
3. Rename import-processing internals and extract interface-first orchestration boundaries for handler collaborators.
4. Migrate persisted batch data shape so stored review state no longer depends on conflict summaries or source fingerprints.
5. Update backend and frontend tests to prove the new readiness/result semantics and simplified review/apply flow.
6. Sync Drinks Catalog and server architecture documentation after implementation, plus update `server/AGENTS.md` with the interface-first orchestration rule.

Rollback strategy:

- Revert the application code and spec deltas together if the simplified workflow proves inadequate during implementation.
- If a database migration changes persisted import-batch shape, use standard EF rollback scripts and preserve import batch history unless rollback requires explicit corrective migration handling.

## Open Questions

- Whether diagnostics should gain explicit target metadata in addition to row number remains open as long as the spec-level behavior stays focused on validity and safety.
