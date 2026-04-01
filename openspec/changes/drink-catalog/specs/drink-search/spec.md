## ADDED Requirements

### Requirement: Search drinks by text

The system SHALL allow users to search drinks by matching against name, description, and ingredient names.

#### Scenario: Search by drink name

- **WHEN** a user searches with a query matching a drink's name
- **THEN** the system returns drinks whose names contain the query (case-insensitive)

#### Scenario: Search by ingredient

- **WHEN** a user searches with a query matching an ingredient name
- **THEN** the system returns drinks that contain that ingredient

#### Scenario: Search returns paginated results

- **WHEN** a user performs a search
- **THEN** the results are paginated with the same pagination rules as browsing

#### Scenario: No results

- **WHEN** a user searches with a query that matches no drinks
- **THEN** the system returns an empty list with zero total count

#### Scenario: Empty or whitespace-only query

- **WHEN** a user submits a search with an empty or whitespace-only query
- **THEN** the system returns a 400 Bad Request response
