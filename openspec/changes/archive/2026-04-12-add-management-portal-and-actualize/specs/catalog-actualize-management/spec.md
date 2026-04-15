## ADDED Requirements

### Requirement: Start Import

The system SHALL allow a manager to start an import for a configured import sync strategy.

#### Scenario: Start snapshot-based import

- **GIVEN** a manager selects the `iba-cocktails-snapshot` strategy
- **WHEN** the manager starts the import
- **THEN** the system SHALL create an import batch with source provenance metadata
- **AND** the system SHALL validate the normalized payload immediately
- **AND** the system SHALL detect conflicts and persist the current review snapshot immediately
- **AND** the system SHALL record the preserved snapshot fingerprint as the source fingerprint

### Requirement: Validate Import Changes

The system SHALL validate and normalize imports before any catalog mutation is applied.
Normalization SHALL preserve drink preparation method and garnish fields when present in the seed payload.

#### Scenario: Start import validates successfully

- **GIVEN** an import contains parseable source records
- **WHEN** a manager starts the import
- **THEN** the system SHALL keep the batch in `InProgress` status
- **AND** the system SHALL return any diagnostics collected during validation
- **AND** the system SHALL persist review rows and conflict markers for the current import snapshot

#### Scenario: Start import records actionable validation diagnostics

- **GIVEN** an import contains invalid records
- **WHEN** a manager starts the import
- **THEN** the system SHALL keep the batch in `InProgress` status
- **AND** the system SHALL return row-level diagnostics with reasons

### Requirement: Review Import Changes

The system SHALL allow managers to review row-level create, update, and skip plans after validation without changing batch status.

#### Scenario: Review is generated after validation

- **GIVEN** an in-progress batch exists
- **WHEN** a manager runs review
- **THEN** the system SHALL return review rows grouped by create, update, and skip actions
- **AND** the system SHALL persist conflict markers needed for explicit review decisions

### Requirement: Explicit Conflict Resolution Before Apply

The system SHALL require explicit manager decisions for conflicts prior to apply.

#### Scenario: Manager applies in-progress batch with explicit decisions

- **GIVEN** an in-progress batch has conflicting candidate records
- **WHEN** the manager submits explicit apply decisions per conflict
- **THEN** the system SHALL apply only the approved create or update actions
- **AND** the system SHALL persist rejected or skipped decisions in the batch audit trail

#### Scenario: Apply is rejected without explicit decisions

- **GIVEN** an in-progress batch contains unresolved conflicts
- **WHEN** a manager attempts to apply without decisions
- **THEN** the system SHALL reject the apply request with a validation error

#### Scenario: Apply is rejected when validation errors remain

- **GIVEN** a batch contains validation errors
- **WHEN** a manager attempts to apply it
- **THEN** the system SHALL reject the apply request

### Requirement: Import Audit And Idempotency

The system SHALL persist audit and provenance information for every import run and prevent accidental duplicate apply.

#### Scenario: Source fingerprint prevents duplicate apply

- **GIVEN** a completed batch exists for the same source strategy and source fingerprint
- **WHEN** a manager attempts to apply the same payload again
- **THEN** the system SHALL prevent duplicate apply unless an explicit re-run override is requested

#### Scenario: Import history is queryable

- **GIVEN** prior import batches exist
- **WHEN** a manager requests import history
- **THEN** the system SHALL return batch status, provenance metadata, timestamps, and apply summary counts

### Requirement: Import Completion Lifecycle

The system SHALL keep import lifecycle status focused on workflow completion rather than intermediate review steps.

#### Scenario: Import remains in progress until apply or cancel

- **GIVEN** a manager has started an import
- **WHEN** validation or review data is generated
- **THEN** the batch SHALL remain in `InProgress` status until it is either applied or cancelled

#### Scenario: Apply completes the batch

- **GIVEN** an in-progress batch has no validation errors and no unresolved conflicts
- **WHEN** a manager applies the batch
- **THEN** the system SHALL mark the batch as `Completed`

#### Scenario: Manager cancels the batch

- **GIVEN** an in-progress batch exists
- **WHEN** a manager cancels it
- **THEN** the system SHALL mark the batch as `Cancelled`
