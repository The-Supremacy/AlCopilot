## Context

AlCopilot currently has a bare-bones Host with no modules. The Catalog module is the first to be implemented, establishing the patterns for all future modules. The Host's `Program.cs` has only service defaults and a hello-world endpoint. The AppHost orchestrates only the Host project with no database resources.

The `Directory.Packages.props` already includes testing packages (xUnit, Shouldly, NSubstitute, TestContainers, NetArchTest) but has no EF Core, Npgsql, or Mediator packages yet.

## Goals / Non-Goals

**Goals:**

- Establish the Catalog module as the reference implementation for all future modules
- Implement browsing, search, and management endpoints for drinks, categories, and ingredients
- Set up EF Core with a dedicated `catalog` Postgres schema and migrations
- Create the Contracts project pattern for cross-module communication
- Wire Postgres into the Aspire AppHost for local development
- Achieve full test coverage with TestContainers against real Postgres

**Non-Goals:**

- Authentication/authorization (Identity module — future work; endpoints are unauthenticated for now)
- Frontend UI (web portal — separate change)
- AI-powered recommendations (Recommendation module — depends on Catalog.Contracts)
- Image upload/storage (drinks reference image URLs, not binary storage)
- Full-text search via Postgres `tsvector` (initial implementation uses `ILIKE` queries; can migrate to `tsvector` when performance requires it)

## Decisions

### Decision: EF Core with schema-per-module

`CatalogDbContext` uses `catalog` as its Postgres schema. All Catalog tables (`drinks`, `categories`, `ingredients`, `drink_ingredients`) live under this schema. Other modules will use their own schemas, keeping data boundaries clean in a shared database.

**Alternative considered**: Separate databases per module. Rejected — unnecessary operational overhead at this stage. Schema isolation provides the same logical separation with simpler infrastructure.

### Decision: Mediator for request dispatch

All API endpoints dispatch through Mediator (source-generated). Queries and commands are defined in `AlCopilot.Catalog.Contracts` so other modules can send queries (e.g., `GetDrinkByIdQuery`) without referencing the Catalog implementation.

**Alternative considered**: Direct service injection. Rejected — Mediator provides the decoupling needed for the Contracts pattern and future module extraction.

### Decision: Minimal API endpoints grouped by resource

Endpoints use .NET Minimal APIs with `MapGroup()` for route organization:

- `GET /api/catalog/drinks` — list/browse with optional category filter
- `GET /api/catalog/drinks/{id}` — drink details
- `GET /api/catalog/drinks/search?q=` — search
- `POST /api/catalog/drinks` — create
- `PUT /api/catalog/drinks/{id}` — update
- `DELETE /api/catalog/drinks/{id}` — soft delete
- `GET /api/catalog/categories` — list categories
- `POST /api/catalog/categories` — create category
- `GET /api/catalog/ingredients` — list ingredients
- `POST /api/catalog/ingredients` — create ingredient

**Alternative considered**: Controllers. Rejected — Minimal APIs are the modern .NET approach, lighter weight, and recommended by the Aspire template.

### Decision: Soft delete via `IsDeleted` flag

Drinks support soft deletion with an `IsDeleted` boolean and `DeletedAtUtc` timestamp. A global query filter on the DbContext excludes deleted drinks from all queries by default. Admin queries can explicitly include deleted records.

**Alternative considered**: Hard delete. Rejected — other modules (Social, Recommendation) may reference drink IDs; hard delete would break referential integrity across modules.

### Decision: ILIKE for initial search

Search uses Postgres `ILIKE` for case-insensitive matching across drink name, description, and ingredient names via a `LEFT JOIN`. This is simple, correct, and sufficient for the initial dataset.

**Alternative considered**: Full-text search with `tsvector/tsquery`. Deferred — adds migration complexity (GIN indexes, trigger-maintained search vectors) that isn't justified until we have performance data showing `ILIKE` is a bottleneck.

### Decision: Offset pagination

API list endpoints use offset pagination (`page` + `pageSize` parameters) returning total count. Default page size is 20, max is 100.

**Alternative considered**: Cursor-based pagination. Deferred — offset pagination is simpler for the browsing use case where users navigate to specific pages. Can be reconsidered if performance degrades on large datasets.

## Risks / Trade-offs

**[ILIKE performance on large datasets]** → Acceptable risk. Monitoring via Aspire dashboard. Migration to `tsvector` is a backward-compatible enhancement that doesn't change the API contract.

**[No auth on management endpoints]** → Temporary. Management endpoints will be secured when the Identity module is implemented. Documented as a known gap.

**[Offset pagination skip cost]** → For deep pages (`page=1000`), Postgres must scan and discard rows. Mitigated by the max page size of 100 and the expectation that the catalog will stay under 10k drinks for the foreseeable future.

## Migration Plan

This is a greenfield module — no existing data to migrate.

1. Add EF Core + Npgsql packages to `Directory.Packages.props`
2. Add Mediator package to `Directory.Packages.props`
3. Create module projects and wire into the solution
4. Add Postgres resource to AppHost
5. Generate initial EF Core migration
6. Module registration in Host runs migrations on startup (development only; production uses explicit migration tooling)

**Rollback**: Remove the module registration from Host and drop the `catalog` schema.

## Open Questions

- Should drink names be globally unique or unique within a category? (Proposal assumes globally unique)
- Should the ingredient quantity be a string (e.g., "2 oz", "a splash") or structured (amount + unit)? (Design assumes string for flexibility)
