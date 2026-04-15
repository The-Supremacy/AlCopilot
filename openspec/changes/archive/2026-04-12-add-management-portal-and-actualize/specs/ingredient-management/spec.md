## ADDED Requirements

### Requirement: Delete Ingredient

The system SHALL allow deleting an ingredient when no active drink recipe references it.

#### Scenario: Delete unreferenced ingredient

- **GIVEN** an ingredient exists and no active drink recipe uses it
- **WHEN** a manager deletes the ingredient
- **THEN** the system SHALL remove the ingredient

#### Scenario: Delete ingredient in use by active drink

- **GIVEN** an ingredient is used by an active drink recipe
- **WHEN** a manager attempts to delete the ingredient
- **THEN** the system SHALL reject the request with a conflict error indicating the ingredient is still in use
