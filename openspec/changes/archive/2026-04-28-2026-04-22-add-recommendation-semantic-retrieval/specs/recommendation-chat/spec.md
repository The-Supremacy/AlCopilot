## MODIFIED Requirements

### Requirement: Deterministic Candidate Building Before Model Ranking

The system SHALL apply deterministic recommendation preparation before model narration or ranking.

#### Scenario: Semantic retrieval enriches deterministic preparation without bypassing exclusions

- **WHEN** the customer asks for a descriptive recommendation such as flavor, texture, or mood language
- **THEN** the system SHALL allow semantic retrieval to enrich recommendation intent and candidate ranking
- **AND** it SHALL keep prohibited-ingredient exclusion and availability grouping in deterministic module code

### Requirement: Structured Recommendation Responses

The system SHALL return recommendation-chat assistant responses as structured recommendation groups plus conversational prose.

#### Scenario: Recommendation reply can reflect semantic drink matches

- **WHEN** the system identifies strong semantic matches from drink descriptions
- **THEN** the generated response SHALL be allowed to use those semantic matches when explaining the recommendation outcome
- **AND** the response SHALL remain grounded in the deterministic recommendation groups returned by the module

## ADDED Requirements

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
