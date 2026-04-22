## MODIFIED Requirements

### Requirement: Deterministic Candidate Building Before Model Ranking

The system SHALL apply deterministic recommendation preparation before model narration or ranking.

#### Scenario: Prohibited ingredients exclude drinks from candidates

- **WHEN** the customer profile contains prohibited ingredients
- **THEN** the system SHALL exclude drinks containing those ingredients from the recommendation candidate set before model narration or ranking

#### Scenario: Disliked ingredients reduce ranking priority

- **WHEN** candidate drinks contain ingredients the customer marked as disliked
- **THEN** the system SHALL treat those drinks as lower-priority candidates rather than automatically excluding them

#### Scenario: Inventory split distinguishes available and near-miss drinks

- **WHEN** the system evaluates candidate drinks against the customer's owned ingredients
- **THEN** the system SHALL separate recommendation results into drinks that are available now and drinks that are better framed as restock candidates

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

### Requirement: Limited Read-Only Model Tool Calling

The system SHALL keep recommendation execution bounded so that model-owned execution remains read-only and persistence stays outside model-controlled actions.

#### Scenario: Read-only model helper can contribute to a recommendation response

- **WHEN** the recommendation flow invokes an allowed read-only model helper during recommendation generation
- **THEN** the system SHALL allow the helper result to contribute to the generated recommendation response

#### Scenario: Recommendation flow does not allow model-owned writes

- **WHEN** the recommendation flow runs with model-assisted recommendation generation enabled
- **THEN** the system SHALL keep persistence and profile mutation outside model-owned execution

## ADDED Requirements

### Requirement: Recommendation Chat Uses A Bounded Workflow

The system SHALL orchestrate recommendation execution through a bounded workflow that coordinates deterministic preparation and model narration without bypassing module boundaries.

#### Scenario: Workflow loads bounded recommendation inputs before narration

- **WHEN** an authenticated customer sends a recommendation message
- **THEN** the system SHALL load the customer profile snapshot and bounded recommendation input set before generating the assistant response

#### Scenario: Workflow persists session state outside model-owned execution

- **WHEN** the recommendation workflow finishes recommendation generation
- **THEN** the system SHALL append and persist the resulting conversation turns outside model-owned execution

#### Scenario: Workflow can persist internal execution diagnostics separate from the customer transcript

- **WHEN** development-time execution diagnostics are enabled for recommendation chat
- **THEN** the system SHALL persist internal step traces, including tool activity and returned reasoning metadata, separately from the customer-facing recommendation turn payload
