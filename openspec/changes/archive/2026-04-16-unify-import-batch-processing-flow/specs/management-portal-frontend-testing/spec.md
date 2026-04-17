## MODIFIED Requirements

### Requirement: Cover Import Review And Apply Gating With Frontend Tests

The system SHALL keep frontend tests for the management portal import workspace so batch review visibility and apply gating remain stable after import-processing flow refactors.

#### Scenario: Review page remains covered after processing-model refactor

- **GIVEN** an in-progress import batch with stale or current review data
- **WHEN** frontend verification runs
- **THEN** the portal test suite SHALL prove stale review refresh behavior and review-row rendering on the review page
- **AND** the portal test suite SHALL remain valid if backend processing internals change while review-page behavior stays the same

#### Scenario: Imports workspace remains covered after readiness naming or result-shape cleanup

- **GIVEN** an in-progress import batch with update rows or validation diagnostics
- **WHEN** frontend verification runs
- **THEN** the portal test suite SHALL prove apply stays blocked for validation errors or review-required update batches
- **AND** the portal test suite SHALL cover any contract ripple caused by refined batch or apply-result readiness naming
