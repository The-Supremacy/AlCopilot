## ADDED Requirements

### Requirement: Manage Customer Ingredient Preferences

The system SHALL allow an authenticated customer to manage favorite, disliked, and prohibited ingredients as separate preference sets.

#### Scenario: Customer saves preference sets

- **WHEN** an authenticated customer updates their favorite, disliked, and prohibited ingredient selections
- **THEN** the system SHALL persist each preference set for that customer profile

#### Scenario: Customer profile loads existing preference sets

- **WHEN** an authenticated customer opens the preferences experience after previously saving ingredient preferences
- **THEN** the system SHALL return the saved favorite, disliked, and prohibited ingredient selections for that customer profile

### Requirement: Manage Home Bar Ingredient Inventory

The system SHALL allow an authenticated customer to maintain a simple set of ingredients they currently have available.

#### Scenario: Customer saves owned ingredients

- **WHEN** an authenticated customer updates the ingredients in their home bar
- **THEN** the system SHALL persist the resulting owned-ingredient set for that customer profile

#### Scenario: Home bar state stays scoped to the authenticated customer

- **WHEN** the system loads a customer profile for recommendation or editing flows
- **THEN** the system SHALL return only the preference and owned-ingredient state for the authenticated customer identity
