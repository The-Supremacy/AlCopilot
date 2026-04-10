## ADDED Requirements

### Requirement: Create Import Draft

The system SHALL allow a manager to create an import draft for a configured actualization strategy.

#### Scenario: Create file-based import draft

- **GIVEN** a manager provides a valid file payload reference for `file-csv-json`
- **WHEN** the manager creates a draft import
- **THEN** the system SHALL create an import batch in `Draft` status with source provenance metadata

#### Scenario: Create IBA-based import draft

- **GIVEN** a manager selects the `iba-source` strategy
- **WHEN** the manager creates a draft import
- **THEN** the system SHALL create an import batch in `Draft` status with strategy metadata and source fingerprint placeholders

### Requirement: Validate And Preview Import Changes

The system SHALL validate and normalize draft imports before any catalog mutation is applied.

#### Scenario: Validation succeeds and preview is generated

- **GIVEN** a draft import contains parseable source records
- **WHEN** a manager runs validation
- **THEN** the system SHALL move the batch to `Validated` status
- **AND** the system SHALL produce a preview diff grouped by create, update, and skip actions

#### Scenario: Validation fails with actionable diagnostics

- **GIVEN** a draft import contains invalid records
- **WHEN** a manager runs validation
- **THEN** the system SHALL mark the batch as failed validation
- **AND** the system SHALL return row-level diagnostics with reasons

### Requirement: Explicit Conflict Resolution Before Apply

The system SHALL require explicit manager decisions for conflicts prior to apply.

#### Scenario: Manager applies previewed batch with explicit decisions

- **GIVEN** a batch is `Previewed` with conflicting candidate records
- **WHEN** the manager submits explicit apply decisions per conflict
- **THEN** the system SHALL apply only the approved create or update actions
- **AND** the system SHALL persist rejected or skipped decisions in the batch audit trail

#### Scenario: Apply is rejected without explicit decisions

- **GIVEN** a previewed batch contains unresolved conflicts
- **WHEN** a manager attempts to apply without decisions
- **THEN** the system SHALL reject the apply request with a validation error

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
