## Context

The current Drink Catalog import workflow already reflects the simplified product behavior introduced by `simplify-import-review-workflow`, but its internal implementation still exposes validation and review-snapshot generation as separate public operations.
In practice, `InitializeImportBatchHandler`, `ReviewImportBatchHandler`, and the snapshot-rebuild path in `ApplyImportBatchHandler` always need diagnostics, review summary, and review rows together.

That split creates two problems:

- handlers assemble workflow state across multiple public calls and aggregate methods even though those calls are not independently meaningful in current workflows
- the aggregate API allows future edits to persist partially assembled review state more easily than necessary, even though current transaction boundaries avoid that in practice

Affected domain model:

- Aggregate root: `ImportBatch`
- Workflow service boundary: `IImportBatchProcessingService`
- Referenced aggregates during apply: `Drink`, `Ingredient`, `Tag`
- Relevant workflow records/value-like types: `ImportDiagnostic`, `ImportReviewRow`, `ImportReviewSummary`, `ImportApplySummary`, `ImportBatchApplyReadiness`

This change is full-stack scoped because the refactor may produce outward DTO or result-shape cleanup, but the main work remains backend workflow modeling.

## Goals / Non-Goals

**Goals:**

- Make import processing atomic at the public application-service boundary.
- Keep diagnostics and review rows conceptually distinct while computing them together for the workflows that always need both.
- Refactor aggregate snapshot-recording methods so coherent processing data moves together.
- Simplify initialize, review, and apply-fallback handler orchestration.
- Preserve explicit non-ready apply results instead of exception-driven business flow.
- Clarify batch-scoped readiness naming where the current wording is ambiguous.

**Non-Goals:**

- Reintroducing conflict-resolution workflow concepts.
- Adding new lifecycle statuses only to model internal preparation steps.
- Moving repository- or query-dependent comparison logic into the aggregate.
- Changing the accepted product semantics of create-only apply, update review gating, or validation blocking beyond what this internal refactor requires.

## Decisions

### Decision 1: Keep one public processing service boundary

`IImportBatchProcessingService` remains the application-service boundary for import preparation because matching and validation depend on repositories and query services that do not belong in the aggregate.
However, it will expose one public combined processing method instead of separate public validation and review-building methods.

Why:

- The current workflows always need the combined result.
- A single public processing method better expresses the real unit of work.
- Keeping a service boundary still allows isolated handler tests and avoids moving infrastructure-aware logic into `ImportBatch`.

Alternative considered:

- Inline the combined logic directly into handlers with private methods.
  Rejected because it duplicates orchestration and weakens the reusable application-service boundary we already established.

### Decision 2: Introduce a combined processing result

Add a workflow result type such as `ImportBatchProcessingResult` that contains:

- diagnostics
- review summary
- review rows

Aggregate snapshot-recording methods will consume that result as one object.

Why:

- It prevents summary, rows, and diagnostics from drifting apart through public API misuse.
- It removes redundant aggregate transitions where one method records diagnostics and another immediately overwrites them as part of the same workflow.
- It keeps diagnostics and review semantics separate in data while making processing atomic in execution.

Alternative considered:

- Keep separate arguments and only hide the public service split.
  Rejected because the aggregate API would still expose a loosely assembled workflow shape.

### Decision 3: Keep `ImportBatch.Create(...)` separate from processing

`ImportBatch.Create(...)` will continue to establish identity, provenance, raw normalized import content, and initial lifecycle timestamps.
Processing still happens immediately afterward in initialize workflows, but creation will not require repositories, comparison reads, or a precomputed processing result.

Why:

- Creation and processing are different responsibilities even if they happen in one transaction.
- Forcing processing into creation would blur aggregate and application-service boundaries.
- Transaction safety is sufficient for transient in-memory intermediate state here.

Alternative considered:

- Require creation to accept a combined processing result so a newly created batch is always prepared.
  Rejected because it pushes repository-dependent workflow preparation too close to aggregate construction for little gain.

### Decision 4: Apply fallback rebuilds through the same atomic path

If `Apply` finds a batch without a prepared snapshot, it will rebuild snapshot data through the same combined processing method used by initialize and review flows.
If the rebuilt state is not ready, the handler returns the non-applied result with batch readiness metadata.

Why:

- This keeps fallback behavior consistent with the rest of the workflow.
- It avoids a special-case processing path in apply.
- It preserves the accepted rule that valid non-ready states are not exceptions.

Alternative considered:

- Reject apply when prepared snapshot data is missing.
  Rejected because it removes a useful recovery path and is not necessary while the module is still evolving.

### Decision 5: Use batch-focused readiness naming

Local variable and helper naming should make it explicit that readiness belongs to the import batch, not a generic apply action.
Names like `batchApplyReadiness` and `ImportBatchProcessingResult` are preferred.

Why:

- Current local naming is easy to misread.
- Batch-focused terminology better matches the aggregate-owned workflow state.

Alternative considered:

- Minimal rename of only the most ambiguous local variable.
  Rejected because the refactor already touches the workflow boundary and should leave clearer naming behind.

## Risks / Trade-offs

- [The refactor may look like pure implementation detail and drift out of spec scope] -> Capture only the behavior-level guarantees that change: atomic processing path, coherent snapshot recording, and apply fallback semantics.
- [Combined processing could obscure the conceptual distinction between diagnostics and review rows] -> Keep them as distinct fields in the combined result and in persisted batch state.
- [Outward contract cleanup could grow larger than the backend refactor needs] -> Limit frontend/API changes to places where naming or result-shape cleanup materially improves clarity.
- [Apply fallback rebuild could hide stale or missing snapshot issues] -> Keep the behavior explicit in specs and tests, and continue returning non-ready batch results when rebuild does not yield a ready state.

## Migration Plan

1. Add the new OpenSpec deltas for import processing flow and frontend test coverage.
2. Refactor `IImportBatchProcessingService` and its implementation to expose one public combined processing method.
3. Add the combined processing result type and update `ImportBatch` snapshot-recording methods to consume it.
4. Update initialize, review, and apply handlers to use the atomic processing path.
5. Adjust outward contracts and portal/client code only where the refactor causes naming or result-shape ripple effects.
6. Update backend and frontend tests plus affected runtime/module documentation.

Rollback strategy:

- Revert the refactor and restore the split processing API together with any dependent DTO or frontend changes.
- If local development data becomes incompatible during the refactor, reset non-production import batch data rather than preserving transitional state.

## Open Questions

- Whether the combined processing method should be named `ProcessAsync(...)` or `PrepareSnapshotAsync(...)` can be resolved during implementation as long as the boundary remains single-entry and batch-focused.
