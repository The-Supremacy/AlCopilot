## MODIFIED Requirements

### Requirement: Cover Import Review And Apply Gating With Frontend Tests

The system SHALL keep frontend tests for the management portal import workspace so batch review visibility and apply gating remain stable without row-level decision state.

#### Scenario: Review page proves stale refresh and inspection-first rendering

- **GIVEN** an in-progress import batch with stale or current review data
- **WHEN** frontend verification runs
- **THEN** the portal test suite SHALL prove stale review refresh behavior and review-row rendering on the review page
- **AND** the portal test suite SHALL prove the review page does not depend on row-level approve or reject controls

#### Scenario: Imports workspace proves apply gating without stored decisions

- **GIVEN** an in-progress import batch with update rows or validation diagnostics
- **WHEN** frontend verification runs
- **THEN** the portal test suite SHALL prove apply stays blocked for validation errors
- **AND** the portal test suite SHALL prove the workspace does not require stored row-level decisions before apply
