## MODIFIED Requirements

### Requirement: Structured Recommendation Responses

The system SHALL return recommendation-chat assistant responses as structured recommendation groups plus conversational prose.

#### Scenario: Recommendation reply includes machine-readable groups

- **WHEN** the assistant returns recommendation results
- **THEN** the system SHALL include machine-readable recommendation group data for stable customer-portal rendering

#### Scenario: Recommendation reply includes conversational explanation

- **WHEN** the assistant returns recommendation results
- **THEN** the system SHALL include prose that explains the recommendation outcome in customer-facing language

#### Scenario: Recommendation reply supports lightweight emphasis and bullets in portal rendering

- **WHEN** the assistant returns prose containing `**highlighted text**` or `* bullet` lines
- **THEN** the customer portal SHALL render those patterns as visual emphasis and bullet lists rather than displaying the raw punctuation literally

#### Scenario: Recommendation reply can reflect semantic drink matches

- **WHEN** the system identifies strong semantic matches from drink descriptions
- **THEN** the generated response SHALL be allowed to use those semantic matches when explaining the recommendation outcome
- **AND** the response SHALL remain grounded in the deterministic recommendation groups returned by the module

#### Scenario: Customer portal renders structured recommendation groups as progressive disclosure

- **WHEN** the customer portal displays assistant prose with structured recommendation groups
- **THEN** the prose SHALL remain visible inline as the primary answer
- **AND** the structured recommendation groups SHALL render beneath the prose as collapsed progressive disclosure by default

#### Scenario: Customer portal labels restock-oriented recommendation groups as buy-next options

- **WHEN** the customer portal displays a structured recommendation group keyed as `buy-next`
- **THEN** the group SHALL be labeled `Buy next`
- **AND** the group SHALL expose missing-ingredient detail only when the relevant group or drink detail is expanded

#### Scenario: Customer portal omits empty recommendation groups

- **WHEN** a structured recommendation group contains no items
- **THEN** the customer portal SHALL omit that group from the visible recommendation summary
