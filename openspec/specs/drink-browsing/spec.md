# Spec: Drink Browsing & Search

## Feature: Browse Drinks

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

### Requirement: View Drink Details

The system SHALL return full details for a single drink including recipe and ingredient information.

**Scenario: View existing drink**

- Given a drink exists with tags, recipe entries, and ingredients (with notable brands)
- When a consumer requests the drink by ID
- Then the system SHALL return the drink with all tags, recipe entries (quantity, recommended brand), and each ingredient's details (name, category, notable brands)

**Scenario: View non-existent drink**

- Given no drink exists with the requested ID
- When a consumer requests the drink by ID
- Then the system SHALL return a not-found response

**Scenario: View soft-deleted drink**

- Given a drink has been soft-deleted
- When a consumer requests it by ID
- Then the system SHALL return a not-found response (query filter excludes it)

## Feature: Search Drinks

### Requirement: Full-Text Search

The system SHALL support searching drinks by name, description, tag name, or ingredient name using case-insensitive partial matching.

**Scenario: Search by drink name**

- Given a drink named "Mojito" exists
- When a consumer searches for "moj"
- Then the system SHALL return "Mojito" in the results

**Scenario: Search by ingredient name**

- Given a drink has an ingredient named "Lime Juice"
- When a consumer searches for "lime"
- Then the system SHALL return that drink in the results

**Scenario: Search by tag name**

- Given a drink is tagged "Refreshing"
- When a consumer searches for "refresh"
- Then the system SHALL return that drink in the results

**Scenario: Search with no results**

- Given no drinks match the search term
- When a consumer searches for "zzzznonexistent"
- Then the system SHALL return an empty paginated result

**Scenario: Search excludes soft-deleted drinks**

- Given a soft-deleted drink matches the search term
- When a consumer searches
- Then the soft-deleted drink SHALL NOT appear in results

**Scenario: Search results are paginated**

- Given many drinks match the search term
- When a consumer searches with pagination parameters
- Then the system SHALL return the correct page with total count
