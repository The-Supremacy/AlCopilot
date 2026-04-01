## Context

DrinkCatalog is the first module in the AlCopilot modular monolith. It owns the drink database — drinks, ingredients, categories, tags, and recipes — and exposes browse, search, and CRUD operations via REST endpoints.

Because this is the first module, it also establishes the cross-cutting DDD infrastructure in `AlCopilot.Shared` (aggregate base types, value objects, repositories, unit of work, domain event interceptor) that all subsequent modules will reuse.

Current state: the project has `AlCopilot.Host` (BFF), `AlCopilot.AppHost` (Aspire orchestrator), and `AlCopilot.ServiceDefaults`. No modules exist yet. Existing code on the branch from a prior implementation attempt will be adapted to follow proper DDD patterns.

## Goals / Non-Goals

**Goals:**

- Establish DDD infrastructure in `AlCopilot.Shared` (base types, interceptor, repositories)
- Implement DrinkCatalog module with proper aggregate roots, value objects, and repositories
- Provide REST API for drink browse, search, and CRUD under `/api/drink-catalog`
- Demonstrate the full module lifecycle: domain → persistence → contracts → handlers → endpoints → tests
- Set up testing infrastructure: TestContainers fixture, architecture test project

**Non-Goals:**

- Authentication/authorization (Identity module, future)
- Frontend UI (separate change)
- Cross-module eventing via Rebus (no other modules exist yet to consume events)
- Image upload/storage (ImageUrl is a string URL)
- Full outbox background worker (events are persisted for future use, but no Rebus publisher yet)

## Decisions

### 1. Aggregate Boundaries

| Aggregate Root         | Children                     | Value Objects                                          | Rationale                                                                                                               |
| ---------------------- | ---------------------------- | ------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------- |
| **Drink**              | `RecipeEntry` (child entity) | `DrinkName` (max 200), `ImageUrl` (max 1000, optional) | Drink is the central concept. Recipe entries are meaningless outside a drink — they form a single consistency boundary. |
| **Tag**                | —                            | `TagName` (max 100)                                    | Tags are independent, referenced by ID from Drink. Simple lookup aggregate.                                             |
| **Ingredient**         | —                            | `IngredientName` (max 200)                             | Ingredients exist independently of drinks. Recipes reference them by ID.                                                |
| **IngredientCategory** | —                            | `CategoryName` (max 100)                               | Categories group ingredients. Referenced by ID from Ingredient.                                                         |

**Alternative considered**: Making Tag/Ingredient/IngredientCategory simple EF entities without aggregate root base. Rejected because it would create two patterns in one module — some entities with DDD, some without. Consistency matters more than saving a few lines on simple aggregates.

**RecipeEntry** is a child entity (`Entity<(Guid DrinkId, Guid IngredientId)>`) within the Drink aggregate. It holds `Quantity` (value object, max 100 chars) and optional `RecommendedBrand` (plain string, max 200). The composite key (DrinkId + IngredientId) enforces one entry per ingredient per drink.

### 2. Value Objects

| Value Object     | Wraps     | Validation                                 | Used By                   |
| ---------------- | --------- | ------------------------------------------ | ------------------------- |
| `DrinkName`      | `string`  | Non-empty, max 200 chars, trimmed          | `Drink.Name`              |
| `TagName`        | `string`  | Non-empty, max 100 chars, trimmed          | `Tag.Name`                |
| `IngredientName` | `string`  | Non-empty, max 200 chars, trimmed          | `Ingredient.Name`         |
| `CategoryName`   | `string`  | Non-empty, max 100 chars, trimmed          | `IngredientCategory.Name` |
| `Quantity`       | `string`  | Non-empty, max 100 chars                   | `RecipeEntry.Quantity`    |
| `ImageUrl`       | `string?` | Max 1000 chars, nullable (null = no image) | `Drink.ImageUrl`          |

All inherit from `ValueObject<T>` with implicit conversion to `T`. Factory method `Create(raw)` validates and returns the value object. EF Core maps via `HasConversion(v => v.Value, raw => TypeName.Create(raw))`.

**Alternative considered**: Skipping value objects for simple names. Rejected — validation in the type system prevents invalid data from ever existing, and the pattern is established now for all modules.

### 3. Domain Events

| Event               | Raised By                | Payload                   |
| ------------------- | ------------------------ | ------------------------- |
| `DrinkCreatedEvent` | `Drink.Create()` factory | `DrinkId`                 |
| `DrinkDeletedEvent` | `Drink.SoftDelete()`     | `DrinkId`, `DeletedAtUtc` |

No same-module handlers react to these events in DrinkCatalog. They are persisted to `domain_events` for future cross-module consumption (e.g., Recommendation module reacting to new drinks). The interceptor infrastructure is built now; consumers come later.

**Alternative considered**: Adding events for tags/ingredients/categories. Rejected — no foreseeable consumer. Events can be added when a use case arises.

### 4. Repository Design

```
IRepository<TRoot, TId>           (in AlCopilot.Shared)
  ├── IDrinkRepository            (in DrinkCatalog — adds query methods)
  ├── ITagRepository              (in DrinkCatalog)
  ├── IIngredientRepository       (in DrinkCatalog)
  └── IIngredientCategoryRepository (in DrinkCatalog)
```

- `IDrinkRepository` extends generic with: `GetPagedAsync(tagIds, page, pageSize)`, `SearchAsync(query, page, pageSize)`, `ExistsByNameAsync(name)`
- Other repositories use the generic interface only (GetByIdAsync, Add, Remove)
- Implementations are `internal sealed` classes wrapping `DrinkCatalogDbContext`
- `DrinkRepository` loads the complete aggregate: Drink + Tags + RecipeEntries (with Ingredient → IngredientCategory)

**Alternative considered**: Using DbContext directly in handlers (current code). Rejected — violates DDD, makes handlers untestable without a real database, couples domain logic to EF.

### 5. Handler Pattern

Handlers are thin orchestrators:

```
1. Receive Mediator request
2. Load aggregate(s) via IRepository
3. Call domain method on aggregate (validation + business logic happens here)
4. Save via IUnitOfWork (triggers domain event interceptor)
5. Return result
```

Handlers do NOT contain business logic, EF queries, or direct DbContext access.

**Query handlers** are an exception — they use `IReadOnlyRepository` or a read-specific interface for optimized projections (no need to load full aggregates for list/search views). This avoids the N+1 problem of loading aggregates just to project DTOs.

### 6. Persistence

- Schema: `drink_catalog`
- `DrinkCatalogDbContext` implements `IUnitOfWork`
- `DomainEventInterceptor` registered on the DbContext via `AddDbContext` options factory
- `DomainEventRecord` mapped to `drink_catalog.domain_events` table
- Global query filter on `Drink`: `HasQueryFilter(d => !d.IsDeleted)`
- Unique indexes on: `Drink.Name`, `Tag.Name`, `Ingredient.Name`, `IngredientCategory.Name`
- `Ingredient.NotableBrands` (`List<string>`) mapped to `jsonb`
- Many-to-many Drink ↔ Tag via shadow join table `DrinkTag`

### 7. API Endpoints

All under `/api/drink-catalog`:

| Method | Path                       | Handler                                           |
| ------ | -------------------------- | ------------------------------------------------- |
| GET    | `/drinks`                  | `GetDrinksQuery` — paginated, optional tag filter |
| GET    | `/drinks/{id}`             | `GetDrinkByIdQuery` — full detail with recipe     |
| GET    | `/drinks/search?q=`        | `SearchDrinksQuery` — paginated text search       |
| POST   | `/drinks`                  | `CreateDrinkCommand`                              |
| PUT    | `/drinks/{id}`             | `UpdateDrinkCommand`                              |
| DELETE | `/drinks/{id}`             | `DeleteDrinkCommand` (soft delete)                |
| GET    | `/tags`                    | `GetTagsQuery`                                    |
| POST   | `/tags`                    | `CreateTagCommand`                                |
| DELETE | `/tags/{id}`               | `DeleteTagCommand`                                |
| GET    | `/ingredient-categories`   | `GetIngredientCategoriesQuery`                    |
| POST   | `/ingredient-categories`   | `CreateIngredientCategoryCommand`                 |
| GET    | `/ingredients`             | `GetIngredientsQuery` — optional category filter  |
| POST   | `/ingredients`             | `CreateIngredientCommand`                         |
| PUT    | `/ingredients/{id}/brands` | `UpdateIngredientCommand`                         |

### 8. Project Structure

```
server/src/
  AlCopilot.Shared/
    AlCopilot.Shared.csproj
    Domain/
      IAggregateRoot.cs, AggregateRoot.cs, Entity.cs
      IDomainEvent.cs, IDomainEventHandler.cs
      ValueObject.cs
    Data/
      IRepository.cs, IUnitOfWork.cs
      DomainEventInterceptor.cs, DomainEventRecord.cs

  Modules/
    AlCopilot.DrinkCatalog/
      AlCopilot.DrinkCatalog.csproj
      DrinkCatalogModule.cs          → AddDrinkCatalogModule()
      DrinkCatalogEndpoints.cs       → MapDrinkCatalogEndpoints()
      Domain/
        Aggregates/  → Drink.cs, Tag.cs, Ingredient.cs, IngredientCategory.cs
        Entities/    → RecipeEntry.cs
        ValueObjects/ → DrinkName.cs, TagName.cs, IngredientName.cs, CategoryName.cs, Quantity.cs, ImageUrl.cs
        Events/      → DrinkCreatedEvent.cs, DrinkDeletedEvent.cs
      Data/
        DrinkCatalogDbContext.cs (implements IUnitOfWork)
        Repositories/ → DrinkRepository.cs, TagRepository.cs, IngredientRepository.cs, IngredientCategoryRepository.cs
        Migrations/
      Handlers/
        Queries/  → GetDrinksHandler.cs, GetDrinkByIdHandler.cs, SearchDrinksHandler.cs, ...
        Commands/ → CreateDrinkHandler.cs, UpdateDrinkHandler.cs, DeleteDrinkHandler.cs, ...

    AlCopilot.DrinkCatalog.Contracts/
      AlCopilot.DrinkCatalog.Contracts.csproj
      DTOs/     → DrinkDto.cs, DrinkDetailDto.cs, TagDto.cs, ...
      Queries/  → GetDrinksQuery.cs, SearchDrinksQuery.cs, ...
      Commands/ → CreateDrinkCommand.cs, UpdateDrinkCommand.cs, ...

server/tests/
  AlCopilot.DrinkCatalog.Tests/   → unit + integration tests
  AlCopilot.Architecture.Tests/   → module boundary + DDD compliance tests
```

## Risks / Trade-offs

**[Value objects add EF mapping complexity]** → Manageable via `HasConversion`. The pattern is consistent and established once. Worth it for domain validation guarantees.

**[Repository abstraction over EF adds indirection]** → Handlers become testable with NSubstitute. DDD compliance outweighs the extra interface. Query handlers may use read-optimized paths for performance.

**[Domain events persisted but no consumers yet]** → Events are stored atomically. When cross-module consumers arrive, the data is already there. No wasted work.

**[First module sets all patterns]** → Risk of over-engineering. Mitigated by keeping aggregates simple (no deep hierarchies), value objects straightforward (single validated primitives), and the interceptor loop well-tested.

**[Migration rollback]** → `dotnet ef migrations remove` for dev. For production: `DROP SCHEMA drink_catalog CASCADE` since this is the initial schema with no existing data.
