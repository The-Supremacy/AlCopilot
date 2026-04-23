## MODIFIED Requirements

### Requirement: Start Import

The system SHALL allow a manager to start an import for a configured import sync strategy.

#### Scenario: Start snapshot-based import

- **GIVEN** a manager selects the `iba-cocktails-snapshot` strategy
- **WHEN** the manager starts the import
- **THEN** the system SHALL create an import batch with source provenance metadata
- **AND** the system SHALL validate the normalized payload immediately
- **AND** the system SHALL compute diagnostics, review summary, and review rows through one processing operation
- **AND** the system SHALL persist the current prepared review snapshot as review rows describing planned create, update, and skip outcomes
- **AND** the preserved no-payload preset source SHALL come from the AlCopilot-owned extended snapshot rather than the raw upstream-derived snapshot

### Requirement: Validate Import Changes

The system SHALL validate and normalize imports before any catalog mutation is applied.
Normalization SHALL preserve drink preparation method and garnish fields when present in the seed payload.

#### Scenario: Extended snapshot import preserves curated descriptions

- **GIVEN** a manager starts the `iba-cocktails-snapshot` preset without providing a custom payload
- **WHEN** the system normalizes the preserved extended snapshot
- **THEN** the normalized drinks SHALL preserve curated `description` values alongside name, category, method, garnish, and recipe entries
- **AND** the import provenance SHALL remain explicit that the preserved snapshot is an AlCopilot-owned derivative of the upstream seed dataset
