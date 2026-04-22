## 1. Backend import workflow

- [x] 1.1 Remove conflict-list and per-row decision concepts from the Drink Catalog import DTOs, commands, and handlers
- [x] 1.2 Refactor `ImportBatch` workflow state so diagnostics and review rows remain, while separate conflict-summary workflow state is removed or folded into review semantics
- [x] 1.3 Update import apply behavior so validation errors block apply and `Apply` no longer accepts row-level decisions
- [x] 1.4 Add or adjust database migration work needed for persisted import-batch shape changes and verify rollback path
- [x] 1.5 Remove source-fingerprint persistence and duplicate-override behavior from import strategy results, contracts, persistence, and tests
- [x] 1.6 Extract interface-first orchestration boundaries for import processing and audit writing, and switch handlers to depend on interfaces
- [x] 1.7 Replace exception-driven non-ready apply flow with explicit apply-readiness result metadata
- [x] 1.8 Rename import-processing internals to distinguish batch initialization and automated processing from human review

## 2. Frontend import workspace

- [x] 2.1 Remove row-level decision state and controls from the management portal import review workspace
- [x] 2.2 Update imports workspace apply gating and messaging to reflect validation blocking and batch-level review semantics
- [x] 2.3 Confirm `web/apps/management-portal/DESIGN.md` remains intentionally unchanged because this change affects workflow behavior rather than durable UI invariants
- [x] 2.4 Verify the updated import workspace continues using the accepted frontend stack, including Tailwind CSS and existing app-owned UI primitives

## 3. Verification

- [x] 3.1 Update backend unit tests for import start, review, and apply handlers to cover diagnostics, review rows, and apply behavior without row-level decisions
- [x] 3.2 Update backend integration tests for create-only batches, validation-error batches, and update-containing reviewed batches
- [x] 3.3 Update management portal frontend tests to cover inspection-first review rendering, stale review refresh, and apply gating without stored decision state

## 4. Documentation sync

- [x] 4.1 Sync `docs/modules/drinks-catalog.md` so import workflow language no longer describes conflict summaries or row-level decisions
- [x] 4.2 Sync `docs/architecture/server.md` so the JSONB import-workflow example matches the simplified batch review model
- [x] 4.3 Review related implementation notes and API client contracts for stale conflict-resolution terminology and update them where needed
- [x] 4.4 Update `server/AGENTS.md` with the interface-first orchestration boundary rule for handler collaborators
