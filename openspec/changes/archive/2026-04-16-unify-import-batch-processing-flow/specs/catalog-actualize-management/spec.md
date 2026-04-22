## ADDED Requirements

### Requirement: Import Processing Produces One Coherent Snapshot Result

The system SHALL compute import diagnostics and review snapshot data through one atomic processing path for import initialization, review refresh, and apply fallback rebuilds.

#### Scenario: Start import processes normalized content atomically

- **WHEN** the system initializes a new import batch
- **THEN** it SHALL compute diagnostics, review summary, and review rows through one processing operation
- **AND** it SHALL persist the prepared snapshot from that coherent processing result

#### Scenario: Review refresh processes batch content atomically

- **WHEN** a manager refreshes review data for an in-progress batch
- **THEN** the system SHALL compute diagnostics, review summary, and review rows through one processing operation
- **AND** it SHALL persist the reviewed snapshot from that coherent processing result

### Requirement: Snapshot Recording Uses One Coherent Processing Result

The system SHALL record prepared and reviewed snapshot state on the import batch using one coherent processing result rather than loosely related public inputs.

#### Scenario: Prepared snapshot records diagnostics and review data together

- **WHEN** the system records a prepared snapshot on an import batch
- **THEN** diagnostics, review summary, and review rows SHALL be recorded from the same processing result
- **AND** the batch SHALL NOT require separate public workflow calls to assemble those fields

#### Scenario: Reviewed snapshot records diagnostics and review data together

- **WHEN** the system records a reviewed snapshot on an import batch
- **THEN** diagnostics, review summary, and review rows SHALL be recorded from the same processing result
- **AND** the batch SHALL set reviewed timestamp metadata in that same aggregate transition

### Requirement: Apply Fallback Reuses The Atomic Processing Path

The system SHALL rebuild missing prepared snapshot data during apply by using the same atomic processing path used by import initialization and review refresh.

#### Scenario: Apply rebuilds missing snapshot before readiness evaluation

- **GIVEN** an in-progress batch does not have a prepared review snapshot
- **WHEN** a manager attempts to apply the batch
- **THEN** the system SHALL rebuild diagnostics, review summary, and review rows through the same processing path used by other import workflows
- **AND** it SHALL evaluate batch apply readiness after recording that rebuilt prepared snapshot

#### Scenario: Apply returns non-ready result after fallback rebuild

- **GIVEN** apply rebuilds a missing snapshot and the resulting batch state is not ready
- **WHEN** the system evaluates batch apply readiness
- **THEN** it SHALL return the current batch with readiness metadata
- **AND** it SHALL indicate that the batch was not applied
