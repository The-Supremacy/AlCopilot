# Spec: Management Portal Frontend Testing

### Requirement: Cover Manager Catalog Edit Pages With Frontend Tests

The system SHALL keep page-level frontend tests for the management portal catalog edit workflows so refactors continue proving manager mutation payloads, navigation, and empty-state handling.

**Scenario: Ingredient pages remain covered**

- Given the management portal ingredient create and edit pages are implemented
- When frontend verification runs
- Then the portal test suite SHALL prove parsed notable-brand submission, mutation error handling, delete confirmation, and not-found handling

**Scenario: Drink pages remain covered**

- Given the management portal drink create and edit pages are implemented
- When frontend verification runs
- Then the portal test suite SHALL prove normalized payload submission, filtered recipe-entry submission, loading behavior, delete confirmation, and not-found handling

### Requirement: Cover Import Review And Apply Gating With Frontend Tests

The system SHALL keep frontend tests for the management portal import workspace so batch review visibility and apply gating remain stable without row-level decision state, including after import-processing flow refactors.

**Scenario: Review page proves stale refresh and inspection-first rendering**

- Given an in-progress import batch with stale or current review data
- When frontend verification runs
- Then the portal test suite SHALL prove stale review refresh behavior and review-row rendering on the review page
- And the portal test suite SHALL prove the review page does not depend on row-level approve or reject controls
- And the portal test suite SHALL remain valid if backend processing internals change while review-page behavior stays the same

**Scenario: Imports workspace proves apply gating without stored decisions**

- Given an in-progress import batch with update rows or validation diagnostics
- When frontend verification runs
- Then the portal test suite SHALL prove apply stays blocked for validation errors or review-required update batches
- And the portal test suite SHALL prove the workspace does not require stored row-level decisions before apply
- And the portal test suite SHALL cover any contract ripple caused by refined batch or apply-result readiness naming
