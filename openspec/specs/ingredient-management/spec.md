# Spec: Ingredient & Category Management

## Feature: Ingredient Category Management

### Requirement: Create Ingredient Category

**Scenario: Create category**

- Given no category named "Spirits" exists
- When a consumer creates a category named "Spirits"
- Then the system SHALL create it and return its ID

**Scenario: Duplicate category name**

- Given a category named "Spirits" already exists
- When a consumer creates another with the same name
- Then the system SHALL reject with a conflict error

### Requirement: List Ingredient Categories

**Scenario: List categories ordered by name**

- Given multiple ingredient categories exist
- When a consumer requests the list
- Then the system SHALL return all categories ordered alphabetically by name

## Feature: Ingredient Management

### Requirement: Create Ingredient

**Scenario: Create ingredient with notable brands**

- Given a category "Spirits" exists
- When a consumer creates an ingredient "Vodka" in category "Spirits" with notable brands ["Absolut", "Grey Goose"]
- Then the system SHALL create the ingredient with the brand list stored as JSON

**Scenario: Create ingredient without notable brands**

- Given a category exists
- When a consumer creates an ingredient with no notable brands
- Then the system SHALL create it with an empty brands list

**Scenario: Duplicate ingredient name**

- Given an ingredient named "Vodka" already exists
- When a consumer creates another "Vodka"
- Then the system SHALL reject with a conflict error

**Scenario: Create ingredient with non-existent category**

- Given no category exists with the provided ID
- When a consumer creates an ingredient referencing that category
- Then the system SHALL reject with a validation error

### Requirement: Update Ingredient Notable Brands

**Scenario: Update notable brands**

- Given an ingredient "Vodka" exists with brands ["Absolut"]
- When a consumer updates brands to ["Absolut", "Grey Goose", "Belvedere"]
- Then the system SHALL replace the entire brands list

**Scenario: Update non-existent ingredient**

- Given no ingredient exists with the requested ID
- When a consumer attempts to update it
- Then the system SHALL return a not-found response

### Requirement: List Ingredients

**Scenario: List all ingredients**

- Given ingredients exist across multiple categories
- When a consumer requests ingredients without a category filter
- Then the system SHALL return all ingredients with their category and notable brands

**Scenario: Filter ingredients by category**

- Given ingredients exist in categories "Spirits" and "Mixers"
- When a consumer requests ingredients filtered to "Spirits"
- Then the system SHALL return only ingredients in that category
