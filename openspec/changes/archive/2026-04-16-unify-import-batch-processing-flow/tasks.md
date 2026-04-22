## 1. Backend workflow refactor

- [x] 1.1 Replace the split public import-processing API with one public combined processing method and private helper steps inside the processing service
- [x] 1.2 Add a combined processing result type that carries diagnostics, review summary, and review rows together
- [x] 1.3 Refactor `ImportBatch` prepared and reviewed snapshot-recording methods to consume the combined processing result as one unit
- [x] 1.4 Update initialize, review, and apply handlers to use the atomic processing path, including apply fallback rebuild behavior
- [x] 1.5 Rename ambiguous readiness variables and helper names to batch-focused wording

## 2. Contracts and frontend alignment

- [x] 2.1 Update shared import DTOs or apply-result contracts only where the refactor causes outward naming or result-shape ripple effects
- [x] 2.2 Update management API client and management portal import code for any contract or readiness-naming changes
- [x] 2.3 Verify the refactor does not require management portal `DESIGN.md` changes because behavior-level UX expectations remain unchanged
- [x] 2.4 Verify any affected frontend code continues using the accepted stack and existing app-owned UI primitives

## 3. Verification

- [x] 3.1 Update backend unit tests for import initialization, review refresh, and apply fallback so each workflow path proves one combined processing call and coherent snapshot recording
- [x] 3.2 Update backend integration tests for create-only apply, reviewed update apply, and fallback rebuild returning non-ready results
- [x] 3.3 Update management portal frontend tests for any contract ripple while preserving review refresh and apply-gating coverage

## 4. Documentation sync

- [x] 4.1 Sync Drinks Catalog and runtime workflow notes where they currently describe split processing internals
- [x] 4.2 Sync any implementation-facing documentation that references outdated import-processing naming
