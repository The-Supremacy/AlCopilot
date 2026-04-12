# Spec: Ingredient Management

### Requirement: Create Ingredient

**Scenario: Create ingredient with notable brands**

- Given no ingredient named "Vodka" exists
- When a consumer creates an ingredient "Vodka" with notable brands ["Absolut", "Grey Goose"]
- Then the system SHALL create the ingredient with the brand list stored as JSON

**Scenario: Create ingredient without notable brands**

- Given no ingredient named "Tonic Water" exists
- When a consumer creates an ingredient with no notable brands
- Then the system SHALL create it with an empty brands list

**Scenario: Duplicate ingredient name**

- Given an ingredient named "Vodka" already exists
- When a consumer creates another "Vodka"
- Then the system SHALL reject with a conflict error

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

- Given ingredients exist
- When a consumer requests ingredients
- Then the system SHALL return all ingredients with their notable brands

### Requirement: Delete Ingredient

The system SHALL allow deleting an ingredient when no active drink recipe references it.

**Scenario: Delete unreferenced ingredient**

- Given an ingredient exists and no active drink recipe uses it
- When a manager deletes the ingredient
- Then the system SHALL remove the ingredient

**Scenario: Delete ingredient in use by active drink**

- Given an ingredient is used by an active drink recipe
- When a manager attempts to delete the ingredient
- Then the system SHALL reject the request with a conflict error indicating the ingredient is still in use
