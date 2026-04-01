# Spec: Drink Browsing & Search (Delta)

## MODIFIED Requirements

### Requirement: List Drinks with Pagination

The system SHALL return a paginated list of active drinks ordered by name.

**Scenario: Browse all drinks**

- Given drinks exist in the catalog
- When a consumer requests drinks without filters
- Then the system SHALL return a paginated list of drinks with id, name, description, image URL, and tags
- And results SHALL be ordered alphabetically by name

**Scenario: Browse drinks filtered by tag**

- Given drinks exist with various tags
- When a consumer requests drinks filtered by one or more tag IDs
- Then the system SHALL return drinks associated with ANY of the specified tags (OR logic)

**Scenario: Browse drinks filtered by multiple tags returns union**

- Given Drink A has Tag1 but not Tag2, and Drink B has Tag2 but not Tag1
- When a consumer requests drinks filtered by both Tag1 and Tag2
- Then the system SHALL return both Drink A and Drink B

**Scenario: Browse with pagination**

- Given more drinks exist than the requested page size
- When a consumer requests page 2 with page size 10
- Then the system SHALL return the correct slice of results
- And the response SHALL include total count, current page, and page size

**Scenario: Empty catalog**

- Given no active drinks exist
- When a consumer requests the drink list
- Then the system SHALL return an empty list with total count 0

**Scenario: Soft-deleted drinks excluded**

- Given a drink has been soft-deleted
- When a consumer browses drinks
- Then the soft-deleted drink SHALL NOT appear in results
