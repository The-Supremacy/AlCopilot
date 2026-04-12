# Spec: Drink Management

## Feature: Drink CRUD

### Requirement: Create Drink

The system SHALL allow creating a new drink with a name, optional description, optional image URL, tags, and recipe entries.

**Scenario: Create drink with full details**

- Given valid tags and ingredients exist
- When a consumer creates a drink with name, description, image URL, tags, and recipe entries (each with ingredient, quantity, and optional recommended brand)
- Then the system SHALL create the drink and return its ID
- And the drink SHALL be associated with the specified tags
- And the drink SHALL contain the specified recipe entries

**Scenario: Create drink with minimal details**

- Given valid data
- When a consumer creates a drink with only a name (no description, no image, no tags, no recipe)
- Then the system SHALL create the drink successfully

**Scenario: Duplicate drink name**

- Given a drink named "Mojito" already exists (including soft-deleted)
- When a consumer creates another drink named "Mojito"
- Then the system SHALL reject the request with a conflict error
- And the error message SHALL indicate the name is already taken

**Scenario: Domain event raised on creation**

- Given valid drink data
- When a drink is created
- Then the Drink aggregate SHALL raise a `DrinkCreatedEvent` containing the drink ID

### Requirement: Update Drink

The system SHALL allow updating a drink's details, tags, and recipe entries.

**Scenario: Update drink details**

- Given a drink exists
- When a consumer updates its name, description, and image URL
- Then the system SHALL persist all changes

**Scenario: Replace drink tags**

- Given a drink has tags ["Refreshing", "Summer"]
- When a consumer updates with tags ["Winter", "Classic"]
- Then the drink SHALL have only the new tags — previous associations are removed

**Scenario: Replace recipe entries**

- Given a drink has recipe entries
- When a consumer updates with different recipe entries
- Then the drink SHALL have only the new recipe entries — previous entries are removed

**Scenario: Update non-existent drink**

- Given no drink exists with the requested ID
- When a consumer attempts to update it
- Then the system SHALL return a not-found response

**Scenario: Update to duplicate name**

- Given drinks "Mojito" and "Daiquiri" exist
- When a consumer updates "Daiquiri" to have the name "Mojito"
- Then the system SHALL reject with a conflict error

### Requirement: Soft Delete Drink

The system SHALL support soft-deleting drinks. Soft-deleted drinks are excluded from all queries but remain in the database.

**Scenario: Soft delete existing drink**

- Given a drink exists and is active
- When a consumer deletes it
- Then the drink SHALL be marked as deleted with a timestamp
- And the drink SHALL no longer appear in browse or search results
- And the Drink aggregate SHALL raise a `DrinkDeletedEvent`

**Scenario: Delete non-existent drink**

- Given no drink exists with the requested ID
- When a consumer attempts to delete it
- Then the system SHALL return a not-found response

## Feature: Tag Management

### Requirement: Create Tag

**Scenario: Create tag**

- Given no tag with the name "Tropical" exists
- When a consumer creates a tag named "Tropical"
- Then the system SHALL create the tag and return its ID

**Scenario: Duplicate tag name**

- Given a tag named "Tropical" already exists
- When a consumer creates another tag named "Tropical"
- Then the system SHALL reject with a conflict error

### Requirement: Delete Tag

**Scenario: Delete unreferenced tag**

- Given a tag exists that is not associated with any active drinks
- When a consumer deletes the tag
- Then the system SHALL remove it

**Scenario: Delete tag referenced by drinks**

- Given a tag is associated with one or more active drinks
- When a consumer attempts to delete it
- Then the system SHALL reject with a conflict error indicating the tag is still in use

### Requirement: List Tags

**Scenario: List all tags**

- Given tags exist
- When a consumer requests the tag list
- Then the system SHALL return all tags with their names

### Requirement: Update Tag

The system SHALL allow a manager to rename an existing tag while preserving tag identity.

**Scenario: Rename existing tag**

- Given a tag exists with name "Refreshing"
- When a manager updates the tag name to "Crisp"
- Then the system SHALL persist the new name for the same tag ID

**Scenario: Rename tag to duplicate name**

- Given tags named "Refreshing" and "Classic" exist
- When a manager renames "Classic" to "Refreshing"
- Then the system SHALL reject the request with a conflict error

### Requirement: Management Portal Drink Curation

The system SHALL expose manager-oriented drink curation behavior through the management portal workflow.

**Scenario: Manager creates and immediately curates drink composition**

- Given valid ingredient and tag references exist
- When a manager creates a drink and includes recipe entries and tags
- Then the system SHALL persist the drink as an active catalog entry
- And the drink SHALL be available for subsequent manager edits in the management workflow

**Scenario: Manager deletes drink from curation workflow**

- Given an active drink exists
- When a manager deletes the drink from management workflow
- Then the system SHALL apply soft delete behavior consistent with drink management requirements
