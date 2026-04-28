# Spec: Recommendation Chat

## Purpose

Define supported customer recommendation chat behavior, including persisted sessions, deterministic recommendation preparation, structured assistant responses, bounded model assistance, semantic retrieval, and runtime observability.

## Requirements

### Requirement: Start And Continue Recommendation Chat Sessions

The system SHALL allow an authenticated customer to create and continue persisted recommendation chat sessions.

#### Scenario: First customer message creates a chat session

- **WHEN** an authenticated customer sends a first recommendation message without an existing session
- **THEN** the system SHALL create a persisted chat session and store the resulting conversation turns

#### Scenario: Existing session can be revisited

- **WHEN** an authenticated customer opens a previously saved recommendation session
- **THEN** the system SHALL return the stored conversation history for that session in order

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

#### Scenario: Semantic retrieval enriches deterministic preparation without bypassing exclusions

- **WHEN** the customer asks for a descriptive recommendation such as flavor, texture, or mood language
- **THEN** the system SHALL allow semantic retrieval to enrich recommendation intent and candidate ranking
- **AND** it SHALL keep prohibited-ingredient exclusion and availability grouping in deterministic module code

**Scenario: Semantic retrieval enriches deterministic preparation without bypassing exclusions**

- When the customer asks for a descriptive recommendation such as flavor, texture, or mood language
- Then the system SHALL allow semantic retrieval to enrich recommendation intent and candidate ranking
- And it SHALL keep prohibited-ingredient exclusion and availability grouping in deterministic module code

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

### Requirement: Recommendation Chat Supports Description Semantic Retrieval

The system SHALL support semantic retrieval over recommendation-owned drink description projection text so that descriptive natural-language drink requests do not depend only on exact substring matching.
Drink-name and ingredient-name typo tolerance SHALL use catalog-backed fuzzy lookup rather than semantic retrieval.

#### Scenario: Descriptive request matches drinks through descriptions

- **WHEN** an authenticated customer asks for a descriptive request such as "I want a sparkly sweet drink"
- **THEN** the recommendation flow SHALL be able to match drinks through semantic similarity against curated drink descriptions

#### Scenario: Slight drink-name typo still resolves the intended drink

- **WHEN** an authenticated customer asks for a known drink with a slight typo
- **THEN** the recommendation flow SHALL be able to use catalog-backed fuzzy drink-name lookup to resolve the intended drink candidate
- **AND** semantic retrieval SHALL NOT be required for drink-name typo tolerance

#### Scenario: Slight ingredient-name typo still contributes to ingredient-constrained recommendation

- **WHEN** an authenticated customer asks for drinks with an ingredient using a slight typo or close wording
- **THEN** the recommendation flow SHALL be able to use catalog-backed fuzzy ingredient-name lookup to contribute to ingredient-constrained recommendation
- **AND** semantic retrieval SHALL NOT be required for ingredient-name typo tolerance

**Scenario: Recommendation reply can reflect semantic drink matches**

- When the system identifies strong semantic matches from drink descriptions
- Then the generated response SHALL be allowed to use those semantic matches when explaining the recommendation outcome
- And the response SHALL remain grounded in the deterministic recommendation groups returned by the module

### Requirement: Recommendation Chat Supports Description Semantic Retrieval

The system SHALL support semantic retrieval over recommendation-owned drink description projection text so that descriptive natural-language drink requests do not depend only on exact substring matching.
Drink-name and ingredient-name typo tolerance SHALL use catalog-backed fuzzy lookup rather than semantic retrieval.

**Scenario: Descriptive request matches drinks through descriptions**

- When an authenticated customer asks for a descriptive request such as "I want a sparkly sweet drink"
- Then the recommendation flow SHALL be able to match drinks through semantic similarity against curated drink descriptions

**Scenario: Slight drink-name typo still resolves the intended drink**

- When an authenticated customer asks for a known drink with a slight typo
- Then the recommendation flow SHALL be able to use catalog-backed fuzzy drink-name lookup to resolve the intended drink candidate
- And semantic retrieval SHALL NOT be required for drink-name typo tolerance

**Scenario: Slight ingredient-name typo still contributes to ingredient-constrained recommendation**

- When an authenticated customer asks for drinks with an ingredient using a slight typo or close wording
- Then the recommendation flow SHALL be able to use catalog-backed fuzzy ingredient-name lookup to contribute to ingredient-constrained recommendation
- And semantic retrieval SHALL NOT be required for ingredient-name typo tolerance

### Requirement: Limited Read-Only Model Tool Calling

The system SHALL keep recommendation execution bounded so that model-owned execution remains read-only and persistence stays outside model-controlled actions.

#### Scenario: Read-only model helper can contribute to a recommendation response

- **WHEN** the recommendation flow invokes an allowed read-only model helper during recommendation generation
- **THEN** the system SHALL allow the helper result to contribute to the generated recommendation response

#### Scenario: Recommendation flow does not allow model-owned writes

- **WHEN** the recommendation flow runs with model-assisted recommendation generation enabled
- **THEN** the system SHALL keep persistence and profile mutation outside model-owned execution

#### Scenario: Tool usage is recorded on assistant turns

- **WHEN** the recommendation agent calls an allowed read-only model tool during recommendation generation
- **THEN** the persisted assistant turn SHALL record the tool invocation metadata alongside the generated response

### Requirement: Recommendation Runtime Is Observable

The system SHALL emit recommendation-runtime traces that help developers understand recommendation execution without exposing private model reasoning.

#### Scenario: Recommendation invocation emits agent and model traces

- **WHEN** a recommendation message is processed
- **THEN** the runtime SHALL emit spans for recommendation agent invocation and model/chat execution through the existing OpenTelemetry pipeline

#### Scenario: Tool execution emits trace data

- **WHEN** the recommendation agent invokes an allowed read-only model tool
- **THEN** the runtime SHALL emit trace data for that tool execution through the existing OpenTelemetry pipeline

#### Scenario: Deterministic run-context assembly is traceable

- **WHEN** the recommendation module assembles the deterministic run context before narration
- **THEN** the runtime SHALL emit a trace span for that run-context assembly step

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
