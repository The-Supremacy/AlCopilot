## ADDED Requirements

### Requirement: Browse drinks by category

The system SHALL allow users to browse drinks filtered by category with paginated results.

#### Scenario: List all categories

- **WHEN** a user requests the list of categories
- **THEN** the system returns all active categories with their names and drink counts

#### Scenario: Browse drinks in a category

- **WHEN** a user selects a category
- **THEN** the system returns a paginated list of drinks in that category
- **AND** each drink includes its name, description, image URL, and category

#### Scenario: Browse all drinks without category filter

- **WHEN** a user requests drinks without specifying a category
- **THEN** the system returns a paginated list of all drinks ordered by name

#### Scenario: Empty category

- **WHEN** a user selects a category that contains no drinks
- **THEN** the system returns an empty list with zero total count

### Requirement: View drink details with ingredients

The system SHALL display complete drink details including its ingredients and preparation information.

#### Scenario: View drink by ID

- **WHEN** a user requests a specific drink by its identifier
- **THEN** the system returns the drink's name, description, image URL, category, and full ingredient list
- **AND** each ingredient includes its name and quantity with unit

#### Scenario: Drink not found

- **WHEN** a user requests a drink with a non-existent identifier
- **THEN** the system returns a 404 Not Found response

### Requirement: Paginated results

The system SHALL support cursor-based or offset pagination for all list endpoints.

#### Scenario: Default page size

- **WHEN** a user requests a list without specifying page size
- **THEN** the system returns at most 20 items

#### Scenario: Custom page size with upper bound

- **WHEN** a user requests a list with a page size greater than 100
- **THEN** the system caps the page size at 100

#### Scenario: Pagination metadata

- **WHEN** a user requests a paginated list
- **THEN** the response includes total count, current page, and page size
