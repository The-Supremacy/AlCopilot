## Why

AlCopilot needs a foundational drinks database before any recommendation, search, or social features can work. The Catalog module is the first building block — every other module depends on having drinks, ingredients, and categories to reference. Without it, the platform has nothing to suggest.

This is the first module implementation, so it also establishes the patterns that all subsequent modules will follow: module registration, dedicated DbContext with schema isolation, Contracts project for cross-module communication, and API endpoint conventions.

## What Changes

- Create the **Catalog module** (`AlCopilot.Catalog`) with its own DbContext and `catalog` Postgres schema
- Create the **Catalog Contracts** project (`AlCopilot.Catalog.Contracts`) for cross-module DTOs and queries
- Define domain entities: Drink, Ingredient, Category, and their relationships
- Expose REST API endpoints for browsing, searching, and managing drinks
- Register the module in the Host via `AddCatalogModule()` extension method
- Add the Catalog database to the Aspire AppHost orchestration
- Create the **Catalog Tests** project with unit and integration tests (TestContainers + real Postgres)
- Add architecture tests to enforce module boundary rules

## Capabilities

### New Capabilities

- `drink-browsing`: Browse drinks by category with pagination and view drink details including ingredients
- `drink-search`: Full-text search across drink names, descriptions, and ingredients
- `drink-management`: Admin CRUD operations for drinks, ingredients, and categories

### Modified Capabilities

_None — this is the first module; no existing specs to modify._

## Impact

- **New projects**: `AlCopilot.Catalog`, `AlCopilot.Catalog.Contracts`, `AlCopilot.Catalog.Tests`
- **Modified projects**: `AlCopilot.Host` (module registration, endpoint mapping), `AlCopilot.AppHost` (Postgres resource)
- **Database**: New `catalog` schema in Postgres with EF Core migrations
- **Dependencies**: EF Core + Npgsql (already in stack — no new dependencies)
- **API surface**: New `/api/catalog/*` endpoints (drinks, categories, ingredients)
- **Architecture tests**: New module boundary rules in `AlCopilot.Architecture.Tests`
