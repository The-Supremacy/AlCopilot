## ADDED Requirements

### Requirement: Update Tag

The system SHALL allow a manager to rename an existing tag while preserving tag identity.

#### Scenario: Rename existing tag

- **GIVEN** a tag exists with name "Refreshing"
- **WHEN** a manager updates the tag name to "Crisp"
- **THEN** the system SHALL persist the new name for the same tag ID

#### Scenario: Rename tag to duplicate name

- **GIVEN** tags named "Refreshing" and "Classic" exist
- **WHEN** a manager renames "Classic" to "Refreshing"
- **THEN** the system SHALL reject the request with a conflict error

### Requirement: Management Portal Drink Curation

The system SHALL expose manager-oriented drink curation behavior through the management portal workflow.

#### Scenario: Manager creates and immediately curates drink composition

- **GIVEN** valid ingredient and tag references exist
- **WHEN** a manager creates a drink and includes recipe entries and tags
- **THEN** the system SHALL persist the drink as an active catalog entry
- **AND** the drink SHALL be available for subsequent manager edits in the management workflow

#### Scenario: Manager deletes drink from curation workflow

- **GIVEN** an active drink exists
- **WHEN** a manager deletes the drink from management workflow
- **THEN** the system SHALL apply soft delete behavior consistent with drink management requirements
