# Proposal: Drink Catalog Module

## What

Build the **DrinkCatalog** module — the first bounded context in AlCopilot. This module owns the drink database: drinks, ingredients, ingredient categories, tags, and recipes. It provides browse, search, and CRUD capabilities via a REST API under `/api/drink-catalog`.

## Why

The drink catalog is foundational — every other module (Recommendation, Social, Inventory) depends on having a queryable catalog of drinks with their recipes and ingredients. Building it first establishes the DDD infrastructure (`AlCopilot.Shared` base types), the modular monolith patterns (module registration, contracts, Aspire integration), and the testing patterns (TestContainers, architecture tests) that all subsequent modules will reuse.

## Scope

### In Scope

- **Domain model**: Drink aggregate (with RecipeEntry children), Tag aggregate, Ingredient aggregate, IngredientCategory aggregate — all following DDD conventions with value objects, repositories, and domain events
- **DDD infrastructure**: `AlCopilot.Shared` project with `AggregateRoot<TId>`, `Entity<TId>`, `ValueObject<T>`, `IRepository<TRoot, TId>`, `IUnitOfWork`, `IDomainEvent`, `IDomainEventHandler<T>`, `DomainEventInterceptor`, `DomainEventRecord`
- **Persistence**: `DrinkCatalogDbContext` with `drink_catalog` Postgres schema, EF Core migrations, value object conversions
- **Contracts**: `AlCopilot.DrinkCatalog.Contracts` with query/command DTOs and Mediator message types
- **Handlers**: Mediator request handlers for all CRUD and query operations — handlers orchestrate via repositories and UoW, domain logic in aggregates
- **API endpoints**: Minimal API routes under `/api/drink-catalog` mapped in `DrinkCatalogEndpoints.cs`
- **Soft delete**: Drinks support soft delete (`IsDeleted` + `DeletedAtUtc`), filtered by global query filter
- **Aspire integration**: PostgreSQL resource in AppHost, connection string wiring
- **Tests**: Unit tests (handler orchestration, domain logic), integration tests (TestContainers Postgres), architecture tests (module boundaries, sealed classes, DDD compliance)

### Out of Scope

- Authentication/authorization (Identity module, future)
- AI-powered recommendations (Recommendation module, future)
- Ratings and comments (Social module, future)
- Frontend UI (separate change)
- Image upload/storage (future — `ImageUrl` is a simple string for now)

## Affected Modules

| Module                     | Impact                                            |
| -------------------------- | ------------------------------------------------- |
| **DrinkCatalog** (new)     | Primary — new module                              |
| **AlCopilot.Shared** (new) | DDD base types created here                       |
| **AlCopilot.Host**         | Registers module, maps endpoints, runs migrations |
| **AlCopilot.AppHost**      | Adds PostgreSQL resource                          |

## Dependencies

All dependencies are already in `Directory.Packages.props`:

| Package                               | License            | Purpose                    |
| ------------------------------------- | ------------------ | -------------------------- |
| Npgsql.EntityFrameworkCore.PostgreSQL | PostgreSQL License | EF Core Postgres provider  |
| Mediator.Abstractions                 | MIT                | Mediator message types     |
| Mediator.SourceGenerator              | MIT                | Source-generated mediator  |
| Aspire.Hosting.PostgreSQL             | MIT                | Aspire Postgres resource   |
| Microsoft.EntityFrameworkCore.Design  | MIT                | EF Core migrations tooling |

No new dependencies required.

## Risks

- **First module**: Establishes patterns all future modules follow. Extra review on DDD infrastructure is warranted.
- **Migration strategy**: Initial migration only. Rollback via `dotnet ef migrations remove` or manual `DROP SCHEMA drink_catalog CASCADE`.
