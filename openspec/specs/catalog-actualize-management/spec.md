# Spec: Catalog Actualize Management

### Requirement: Start Import

The system SHALL allow a manager to start an import for a configured import sync strategy.

**Scenario: Start snapshot-based import**

- Given a manager selects the `iba-cocktails-snapshot` strategy
- When the manager starts the import
- Then the system SHALL create an import batch with source provenance metadata
- And the system SHALL validate the normalized payload immediately
- And the system SHALL compute diagnostics, review summary, and review rows through one processing operation
- And the system SHALL persist the current prepared review snapshot as review rows describing planned create, update, and skip outcomes
- And the system SHALL NOT persist source-fingerprint metadata or duplicate-import override workflow state
- And the preserved no-payload preset source SHALL come from the AlCopilot-owned extended snapshot rather than the raw upstream-derived snapshot

### Requirement: Validate Import Changes

The system SHALL validate and normalize imports before any catalog mutation is applied.
Normalization SHALL preserve drink preparation method and garnish fields when present in the seed payload.

**Scenario: Start import validates successfully**

- Given an import contains parseable source records
- When a manager starts the import
- Then the system SHALL keep the batch in `InProgress` status
- And the system SHALL return any diagnostics collected during validation
- And the system SHALL persist review rows for the current import snapshot

**Scenario: Start import records actionable validation diagnostics**

- Given an import contains invalid records
- When a manager starts the import
- Then the system SHALL keep the batch in `InProgress` status
- And the system SHALL return diagnostics with actionable reasons that describe validation or safety findings

**Scenario: Extended snapshot import preserves curated descriptions**

- Given a manager starts the `iba-cocktails-snapshot` preset without providing a custom payload
- When the system normalizes the preserved extended snapshot
- Then the normalized drinks SHALL preserve curated `description` values alongside name, category, method, garnish, and recipe entries
- And the import provenance SHALL remain explicit that the preserved snapshot is an AlCopilot-owned derivative of the upstream seed dataset

### Requirement: Review Import Changes

The system SHALL allow managers to review row-level create, update, and skip plans after validation without changing batch status.

**Scenario: Review is generated after validation**

- Given an in-progress batch exists
- When a manager runs review
- Then the system SHALL compute diagnostics, review summary, and review rows through one processing operation
- And the system SHALL return review rows grouped by create, update, and skip actions
- And the system SHALL preserve update rows as the operator-facing signal that existing catalog records would change

### Requirement: Import Completion Lifecycle

The system SHALL keep import lifecycle status focused on workflow completion rather than intermediate review steps.

**Scenario: Import remains in progress until apply or cancel**

- Given a manager has started an import
- When validation or review data is generated
- Then the batch SHALL remain in `InProgress` status until it is either applied or cancelled

**Scenario: Apply completes a clean create-only batch**

- Given an in-progress batch has no validation errors and no review rows with `update` actions
- When a manager applies the batch
- Then the system SHALL mark the batch as `Completed`

**Scenario: Apply completes a reviewed update batch**

- Given an in-progress batch has no validation errors and includes review rows with `update` actions
- When a manager reviews the batch and applies it
- Then the system SHALL mark the batch as `Completed`

**Scenario: Manager cancels the batch**

- Given an in-progress batch exists
- When a manager cancels it
- Then the system SHALL mark the batch as `Cancelled`

### Requirement: Batch Review Gates Existing-Record Updates

The system SHALL require human batch review before applying an import that would update existing drinks or ingredients.

**Scenario: Update-containing batch remains human-gated**

- Given an in-progress batch contains review rows with `update` actions for existing catalog records
- When the manager opens the review workspace
- Then the system SHALL present those updates for batch-level review before apply

**Scenario: Apply remains blocked by validation errors**

- Given a batch contains validation errors
- When a manager attempts to apply it
- Then the system SHALL return the current batch with apply-readiness metadata describing that the batch is blocked by validation errors
- And the result SHALL indicate that the batch was not applied

**Scenario: Clean create-only batch stays compatible with future auto-apply**

- Given an in-progress batch has no validation errors and no review rows with `update` actions
- When future automation policy evaluates whether the batch may auto-apply
- Then the batch SHALL satisfy the workflow precondition for auto-apply eligibility

### Requirement: Apply Readiness Is Explicit

The system SHALL expose explicit apply-readiness metadata for import batches and apply attempts.

**Scenario: Update batch reports review requirement without error semantics**

- Given an in-progress batch contains update rows and has not yet been explicitly reviewed
- When the system evaluates apply readiness
- Then it SHALL report that the batch requires review before apply

**Scenario: Apply attempt returns valid non-ready outcome**

- Given an in-progress batch is in a valid but non-ready state
- When a manager attempts to apply it
- Then the system SHALL return a result that includes the current batch state and apply-readiness metadata
- And the result SHALL indicate that the batch was not applied

**Scenario: Clean create-only batch applies without explicit review step**

- Given an in-progress batch has no validation errors and no review rows with `update` actions
- When a manager applies the batch without opening the review workspace first
- Then the system SHALL apply the batch successfully
- And the result SHALL indicate that the batch was applied

### Requirement: Import Processing Produces One Coherent Snapshot Result

The system SHALL compute import diagnostics and review snapshot data through one atomic processing path for import initialization, review refresh, and apply fallback rebuilds.

**Scenario: Start import processes normalized content atomically**

- When the system initializes a new import batch
- Then it SHALL compute diagnostics, review summary, and review rows through one processing operation
- And it SHALL persist the prepared snapshot from that coherent processing result

**Scenario: Review refresh processes batch content atomically**

- When a manager refreshes review data for an in-progress batch
- Then the system SHALL compute diagnostics, review summary, and review rows through one processing operation
- And it SHALL persist the reviewed snapshot from that coherent processing result

### Requirement: Snapshot Recording Uses One Coherent Processing Result

The system SHALL record prepared and reviewed snapshot state on the import batch using one coherent processing result rather than loosely related public inputs.

**Scenario: Prepared snapshot records diagnostics and review data together**

- When the system records a prepared snapshot on an import batch
- Then diagnostics, review summary, and review rows SHALL be recorded from the same processing result
- And the batch SHALL NOT require separate public workflow calls to assemble those fields

**Scenario: Reviewed snapshot records diagnostics and review data together**

- When the system records a reviewed snapshot on an import batch
- Then diagnostics, review summary, and review rows SHALL be recorded from the same processing result
- And the batch SHALL set reviewed timestamp metadata in that same aggregate transition

### Requirement: Apply Fallback Reuses The Atomic Processing Path

The system SHALL rebuild missing prepared snapshot data during apply by using the same atomic processing path used by import initialization and review refresh.

**Scenario: Apply rebuilds missing snapshot before readiness evaluation**

- Given an in-progress batch does not have a prepared review snapshot
- When a manager attempts to apply the batch
- Then the system SHALL rebuild diagnostics, review summary, and review rows through the same processing path used by other import workflows
- And it SHALL evaluate batch apply readiness after recording that rebuilt prepared snapshot

**Scenario: Apply returns non-ready result after fallback rebuild**

- Given apply rebuilds a missing snapshot and the resulting batch state is not ready
- When the system evaluates batch apply readiness
- Then it SHALL return the current batch with readiness metadata
- And it SHALL indicate that the batch was not applied
