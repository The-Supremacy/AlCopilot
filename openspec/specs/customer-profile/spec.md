# Spec: Customer Profile

### Requirement: Manage Customer Ingredient Preferences

The system SHALL allow an authenticated customer to manage favorite, disliked, and prohibited ingredients as separate normalized preference sets that remain scoped to that customer profile and are suitable for recommendation workflows.

**Scenario: Customer saves preference sets**

- When an authenticated customer updates their favorite, disliked, and prohibited ingredient selections
- Then the system SHALL persist each preference set for that customer profile

**Scenario: Saved preference sets are normalized**

- When an authenticated customer saves preference sets containing duplicates or empty ingredient identifiers
- Then the system SHALL persist only distinct non-empty ingredient identifiers in each resulting preference set

**Scenario: Customer profile loads existing preference sets**

- When an authenticated customer opens the preferences experience after previously saving ingredient preferences
- Then the system SHALL return the saved favorite, disliked, and prohibited ingredient selections for that customer profile

### Requirement: Manage Home Bar Ingredient Inventory

The system SHALL allow an authenticated customer to maintain a normalized set of ingredients they currently have available.

**Scenario: Customer saves owned ingredients**

- When an authenticated customer updates the ingredients in their home bar
- Then the system SHALL persist the resulting owned-ingredient set for that customer profile

**Scenario: Saved home bar is normalized**

- When an authenticated customer saves owned ingredients containing duplicates or empty ingredient identifiers
- Then the system SHALL persist only distinct non-empty ingredient identifiers in the resulting owned-ingredient set

**Scenario: Home bar state stays scoped to the authenticated customer**

- When the system loads a customer profile for recommendation or editing flows
- Then the system SHALL return only the preference and owned-ingredient state for the authenticated customer identity

### Requirement: Provide Stable Customer Profile Snapshots

The system SHALL provide a stable customer-owned profile snapshot for recommendation workflows even when the customer has not previously saved any profile state.

**Scenario: Empty profile snapshot is returned before first save**

- When an authenticated customer or recommendation workflow loads a profile for a customer who has not previously saved one
- Then the system SHALL return an empty but valid profile snapshot for that authenticated customer identity
