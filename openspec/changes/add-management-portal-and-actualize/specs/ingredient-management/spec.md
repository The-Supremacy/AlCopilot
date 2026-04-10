## ADDED Requirements

### Requirement: Update Ingredient Category

The system SHALL allow a manager to rename an ingredient category.

#### Scenario: Rename existing category

- **GIVEN** a category named "Mixers" exists
- **WHEN** a manager renames it to "Mixers & Sodas"
- **THEN** the system SHALL persist the new category name

#### Scenario: Rename category to duplicate name

- **GIVEN** categories named "Spirits" and "Mixers" exist
- **WHEN** a manager renames "Mixers" to "Spirits"
- **THEN** the system SHALL reject the request with a conflict error

### Requirement: Delete Ingredient Category

The system SHALL allow deleting an ingredient category when no ingredients reference it.

#### Scenario: Delete unreferenced category

- **GIVEN** a category exists with no ingredient references
- **WHEN** a manager deletes the category
- **THEN** the system SHALL remove the category

#### Scenario: Delete category in use

- **GIVEN** a category is referenced by one or more ingredients
- **WHEN** a manager attempts to delete the category
- **THEN** the system SHALL reject the request with a conflict error indicating the category is in use

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
