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

The system SHALL keep frontend tests for the management portal import workspace so explicit review decisions remain the gate for apply from the current-import workspace.

**Scenario: Review page proves stale refresh and decision editing**

- Given an in-progress import batch with stale or current review data
- When frontend verification runs
- Then the portal test suite SHALL prove stale review refresh behavior and conflict-decision editing on the review page

**Scenario: Imports workspace proves explicit decision gating**

- Given an in-progress import batch with conflicts or validation diagnostics
- When frontend verification runs
- Then the portal test suite SHALL prove apply stays blocked for validation errors or unresolved conflicts
- And the portal test suite SHALL prove apply uses stored review decisions and clears them after successful apply or cancel
