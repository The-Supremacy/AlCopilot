## MODIFIED Requirements

### Requirement: Manage Customer Ingredient Preferences

The system SHALL allow an authenticated customer to manage favorite, disliked, and prohibited ingredients as separate normalized preference sets that remain scoped to that customer profile and are suitable for recommendation workflows.

#### Scenario: Customer saves preference sets

- **WHEN** an authenticated customer updates their favorite, disliked, and prohibited ingredient selections
- **THEN** the system SHALL persist each preference set for that customer profile

#### Scenario: Saved preference sets are normalized

- **WHEN** an authenticated customer saves preference sets containing duplicates or empty ingredient identifiers
- **THEN** the system SHALL persist only distinct non-empty ingredient identifiers in each resulting preference set

#### Scenario: Customer profile loads existing preference sets

- **WHEN** an authenticated customer opens the preferences experience after previously saving ingredient preferences
- **THEN** the system SHALL return the saved favorite, disliked, and prohibited ingredient selections for that customer profile

### Requirement: Manage Home Bar Ingredient Inventory

The system SHALL allow an authenticated customer to maintain a normalized set of ingredients they currently have available.

#### Scenario: Customer saves owned ingredients

- **WHEN** an authenticated customer updates the ingredients in their home bar
- **THEN** the system SHALL persist the resulting owned-ingredient set for that customer profile

#### Scenario: Saved home bar is normalized

- **WHEN** an authenticated customer saves owned ingredients containing duplicates or empty ingredient identifiers
- **THEN** the system SHALL persist only distinct non-empty ingredient identifiers in the resulting owned-ingredient set

#### Scenario: Home bar state stays scoped to the authenticated customer

- **WHEN** the system loads a customer profile for recommendation or editing flows
- **THEN** the system SHALL return only the preference and owned-ingredient state for the authenticated customer identity

## ADDED Requirements

### Requirement: Provide Stable Customer Profile Snapshots

The system SHALL provide a stable customer-owned profile snapshot for recommendation workflows even when the customer has not previously saved any profile state.

#### Scenario: Empty profile snapshot is returned before first save

- **WHEN** an authenticated customer or recommendation workflow loads a profile for a customer who has not previously saved one
- **THEN** the system SHALL return an empty but valid profile snapshot for that authenticated customer identity
