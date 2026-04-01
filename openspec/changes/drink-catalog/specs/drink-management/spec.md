## ADDED Requirements

### Requirement: Create a drink

The system SHALL allow administrators to create new drinks with their ingredients.

#### Scenario: Create drink with valid data

- **WHEN** an administrator submits a new drink with name, description, category, and at least one ingredient
- **THEN** the system creates the drink and returns its identifier
- **AND** the drink is immediately available for browsing and search

#### Scenario: Create drink with duplicate name

- **WHEN** an administrator submits a new drink with a name that already exists
- **THEN** the system returns a 409 Conflict response

#### Scenario: Create drink with missing required fields

- **WHEN** an administrator submits a drink without a name or without any ingredients
- **THEN** the system returns a 400 Bad Request response with validation errors

### Requirement: Update a drink

The system SHALL allow administrators to update existing drink details and ingredients.

#### Scenario: Update drink details

- **WHEN** an administrator updates a drink's name, description, or category
- **THEN** the system persists the changes and returns the updated drink

#### Scenario: Update drink ingredients

- **WHEN** an administrator replaces a drink's ingredient list
- **THEN** the system replaces all existing ingredients with the new list

#### Scenario: Update non-existent drink

- **WHEN** an administrator attempts to update a drink that does not exist
- **THEN** the system returns a 404 Not Found response

### Requirement: Delete a drink

The system SHALL allow administrators to soft-delete drinks.

#### Scenario: Soft-delete a drink

- **WHEN** an administrator deletes a drink
- **THEN** the drink is no longer returned in browse or search results
- **AND** the drink data is retained in the database for audit purposes

#### Scenario: Delete non-existent drink

- **WHEN** an administrator attempts to delete a drink that does not exist
- **THEN** the system returns a 404 Not Found response

### Requirement: Manage categories

The system SHALL allow administrators to create and list categories.

#### Scenario: Create a category

- **WHEN** an administrator creates a category with a unique name
- **THEN** the system creates the category and returns its identifier

#### Scenario: Create category with duplicate name

- **WHEN** an administrator creates a category with a name that already exists
- **THEN** the system returns a 409 Conflict response

### Requirement: Manage ingredients

The system SHALL allow administrators to create and list ingredients for reuse across drinks.

#### Scenario: Create an ingredient

- **WHEN** an administrator creates an ingredient with a unique name
- **THEN** the system creates the ingredient and returns its identifier

#### Scenario: List all ingredients

- **WHEN** an administrator requests the list of ingredients
- **THEN** the system returns all ingredients ordered by name
