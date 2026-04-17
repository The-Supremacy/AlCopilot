## MODIFIED Requirements

### Requirement: Start Import

The system SHALL allow a manager to start an import for a configured import sync strategy.

#### Scenario: Start snapshot-based import

- **GIVEN** a manager selects the `iba-cocktails-snapshot` strategy
- **WHEN** the manager starts the import
- **THEN** the system SHALL create an import batch with source provenance metadata
- **AND** the system SHALL validate the normalized payload immediately
- **AND** the system SHALL persist the current review snapshot as review rows describing planned create, update, and skip outcomes
- **AND** the system SHALL NOT persist source-fingerprint metadata or duplicate-import override workflow state

### Requirement: Validate Import Changes

The system SHALL validate and normalize imports before any catalog mutation is applied.
Normalization SHALL preserve drink preparation method and garnish fields when present in the seed payload.

#### Scenario: Start import validates successfully

- **GIVEN** an import contains parseable source records
- **WHEN** a manager starts the import
- **THEN** the system SHALL keep the batch in `InProgress` status
- **AND** the system SHALL return any diagnostics collected during validation
- **AND** the system SHALL persist review rows for the current import snapshot

#### Scenario: Start import records actionable validation diagnostics

- **GIVEN** an import contains invalid records
- **WHEN** a manager starts the import
- **THEN** the system SHALL keep the batch in `InProgress` status
- **AND** the system SHALL return diagnostics with actionable reasons that describe validation or safety findings

### Requirement: Review Import Changes

The system SHALL allow managers to review row-level create, update, and skip plans after validation without changing batch status.

#### Scenario: Review is generated after validation

- **GIVEN** an in-progress batch exists
- **WHEN** a manager runs review
- **THEN** the system SHALL return review rows grouped by create, update, and skip actions
- **AND** the system SHALL preserve update rows as the operator-facing signal that existing catalog records would change

### Requirement: Import Completion Lifecycle

The system SHALL keep import lifecycle status focused on workflow completion rather than intermediate review steps.

#### Scenario: Import remains in progress until apply or cancel

- **GIVEN** a manager has started an import
- **WHEN** validation or review data is generated
- **THEN** the batch SHALL remain in `InProgress` status until it is either applied or cancelled

#### Scenario: Apply completes a clean create-only batch

- **GIVEN** an in-progress batch has no validation errors and no review rows with `update` actions
- **WHEN** a manager applies the batch
- **THEN** the system SHALL mark the batch as `Completed`

#### Scenario: Apply completes a reviewed update batch

- **GIVEN** an in-progress batch has no validation errors and includes review rows with `update` actions
- **WHEN** a manager reviews the batch and applies it
- **THEN** the system SHALL mark the batch as `Completed`

#### Scenario: Manager cancels the batch

- **GIVEN** an in-progress batch exists
- **WHEN** a manager cancels it
- **THEN** the system SHALL mark the batch as `Cancelled`

## ADDED Requirements

### Requirement: Batch Review Gates Existing-Record Updates

The system SHALL require human batch review before applying an import that would update existing drinks or ingredients.

#### Scenario: Update-containing batch remains human-gated

- **GIVEN** an in-progress batch contains review rows with `update` actions for existing catalog records
- **WHEN** the manager opens the review workspace
- **THEN** the system SHALL present those updates for batch-level review before apply

#### Scenario: Apply remains blocked by validation errors

- **GIVEN** a batch contains validation errors
- **WHEN** a manager attempts to apply it
- **THEN** the system SHALL return the current batch with apply-readiness metadata describing that the batch is blocked by validation errors
- **AND** the result SHALL indicate that the batch was not applied

#### Scenario: Clean create-only batch stays compatible with future auto-apply

- **GIVEN** an in-progress batch has no validation errors and no review rows with `update` actions
- **WHEN** future automation policy evaluates whether the batch may auto-apply
- **THEN** the batch SHALL satisfy the workflow precondition for auto-apply eligibility

## ADDED Requirements

### Requirement: Apply Readiness Is Explicit

The system SHALL expose explicit apply-readiness metadata for import batches and apply attempts.

#### Scenario: Update batch reports review requirement without error semantics

- **GIVEN** an in-progress batch contains update rows and has not yet been explicitly reviewed
- **WHEN** the system evaluates apply readiness
- **THEN** it SHALL report that the batch requires review before apply

#### Scenario: Apply attempt returns valid non-ready outcome

- **GIVEN** an in-progress batch is in a valid but non-ready state
- **WHEN** a manager attempts to apply it
- **THEN** the system SHALL return a result that includes the current batch state and apply-readiness metadata
- **AND** the result SHALL indicate that the batch was not applied

#### Scenario: Clean create-only batch applies without explicit review step

- **GIVEN** an in-progress batch has no validation errors and no review rows with `update` actions
- **WHEN** a manager applies the batch without opening the review workspace first
- **THEN** the system SHALL apply the batch successfully
- **AND** the result SHALL indicate that the batch was applied

## REMOVED Requirements

### Requirement: Import Audit And Idempotency

**Reason**: Duplicate-import fingerprint persistence and override workflow are being deferred until there is a stable operational need for them.

**Migration**: Keep import provenance and history, but remove source-fingerprint persistence and duplicate-override behavior from the active import contract.

### Requirement: Explicit Conflict Resolution Before Apply

**Reason**: The import workflow no longer models row-level conflicts or per-row operator decisions as a first-class contract.

**Migration**: Replace conflict markers and explicit decision submission with review rows for planned updates, validation diagnostics for blocking issues, and `Apply` as the single batch-level approval action.
