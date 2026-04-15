# Spec: Catalog Actualize Management

### Requirement: Start Import

The system SHALL allow a manager to start an import for a configured import sync strategy.

**Scenario: Start snapshot-based import**

- Given a manager selects the `iba-cocktails-snapshot` strategy
- When the manager starts the import
- Then the system SHALL create an import batch with source provenance metadata
- And the system SHALL validate the normalized payload immediately
- And the system SHALL detect conflicts and persist the current review snapshot immediately
- And the system SHALL record the preserved snapshot fingerprint as the source fingerprint

### Requirement: Validate Import Changes

The system SHALL validate and normalize imports before any catalog mutation is applied.
Normalization SHALL preserve drink preparation method and garnish fields when present in the seed payload.

**Scenario: Start import validates successfully**

- Given an import contains parseable source records
- When a manager starts the import
- Then the system SHALL keep the batch in `InProgress` status
- And the system SHALL return any diagnostics collected during validation
- And the system SHALL persist review rows and conflict markers for the current import snapshot

**Scenario: Start import records actionable validation diagnostics**

- Given an import contains invalid records
- When a manager starts the import
- Then the system SHALL keep the batch in `InProgress` status
- And the system SHALL return row-level diagnostics with reasons

### Requirement: Review Import Changes

The system SHALL allow managers to review row-level create, update, and skip plans after validation without changing batch status.

**Scenario: Review is generated after validation**

- Given an in-progress batch exists
- When a manager runs review
- Then the system SHALL return review rows grouped by create, update, and skip actions
- And the system SHALL persist conflict markers needed for explicit review decisions

### Requirement: Explicit Conflict Resolution Before Apply

The system SHALL require explicit manager decisions for conflicts prior to apply.

**Scenario: Manager applies in-progress batch with explicit decisions**

- Given an in-progress batch has conflicting candidate records
- When the manager submits explicit apply decisions per conflict
- Then the system SHALL apply only the approved create or update actions
- And the system SHALL persist rejected or skipped decisions in the batch audit trail

**Scenario: Apply is rejected without explicit decisions**

- Given an in-progress batch contains unresolved conflicts
- When a manager attempts to apply without decisions
- Then the system SHALL reject the apply request with a validation error

**Scenario: Apply is rejected when validation errors remain**

- Given a batch contains validation errors
- When a manager attempts to apply it
- Then the system SHALL reject the apply request

### Requirement: Import Audit And Idempotency

The system SHALL persist audit and provenance information for every import run and prevent accidental duplicate apply.

**Scenario: Source fingerprint prevents duplicate apply**

- Given a completed batch exists for the same source strategy and source fingerprint
- When a manager attempts to apply the same payload again
- Then the system SHALL prevent duplicate apply unless an explicit re-run override is requested

**Scenario: Import history is queryable**

- Given prior import batches exist
- When a manager requests import history
- Then the system SHALL return batch status, provenance metadata, timestamps, and apply summary counts

### Requirement: Import Completion Lifecycle

The system SHALL keep import lifecycle status focused on workflow completion rather than intermediate review steps.

**Scenario: Import remains in progress until apply or cancel**

- Given a manager has started an import
- When validation or review data is generated
- Then the batch SHALL remain in `InProgress` status until it is either applied or cancelled

**Scenario: Apply completes the batch**

- Given an in-progress batch has no validation errors and no unresolved conflicts
- When a manager applies the batch
- Then the system SHALL mark the batch as `Completed`

**Scenario: Manager cancels the batch**

- Given an in-progress batch exists
- When a manager cancels it
- Then the system SHALL mark the batch as `Cancelled`
