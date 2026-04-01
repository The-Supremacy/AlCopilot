## 1. DDD Infrastructure (AlCopilot.Shared)

- [x] 1.1 Create `AlCopilot.Shared` project, add to solution under `/Shared/` folder, reference from DrinkCatalog
- [x] 1.2 Implement `IAggregateRoot`, `AggregateRoot<TId>` base class with `Id`, `DomainEvents` list, protected `Raise()` method
- [x] 1.3 Implement `Entity<TId>` base class for child entities
- [x] 1.4 Implement `ValueObject<T>` base class with `Value` property, implicit conversion to `T`, equality by value
- [x] 1.5 Implement `IDomainEvent` marker interface and `IDomainEventHandler<T>` handler interface
- [x] 1.6 Implement `IRepository<TRoot, TId>` generic interface (`GetByIdAsync`, `Add`, `Remove`)
- [x] 1.7 Implement `IUnitOfWork` interface (`SaveChangesAsync`)
- [x] 1.8 Implement `DomainEventRecord` entity (Id, AggregateId, AggregateType, EventType, Payload, OccurredAtUtc, Sequence, IsPublished)
- [x] 1.9 Implement `DomainEventInterceptor` (`SaveChangesInterceptor`) with dispatch-before-commit loop (collect events → persist records → dispatch handlers → repeat, max depth 5)

## 2. Project Setup & Aspire Integration

- [x] 2.1 Update `Directory.Packages.props` with any missing package versions
- [x] 2.2 Create `AlCopilot.DrinkCatalog` project (class library + FrameworkReference AspNetCore.App), add to solution under `/Modules/`
- [x] 2.3 Create `AlCopilot.DrinkCatalog.Contracts` project, add to solution under `/Modules/`
- [x] 2.4 Update `AlCopilot.Host.csproj` — add Mediator.SourceGenerator, project reference to DrinkCatalog
- [x] 2.5 Update `AlCopilot.AppHost` — add PostgreSQL resource with `drink-catalog` database

## 3. Domain Model

- [x] 3.1 Create value objects: `DrinkName`, `TagName`, `IngredientName`, `CategoryName`, `Quantity`, `ImageUrl`
- [x] 3.2 Create `Drink` aggregate root with factory method `Create()`, domain methods `Update()`, `SoftDelete()`, `SetTags()`, `SetRecipeEntries()`
- [x] 3.3 Create `RecipeEntry` child entity (`Entity<(Guid, Guid)>`) with Quantity value object and optional RecommendedBrand
- [x] 3.4 Create `Tag` aggregate root with factory method `Create()`
- [x] 3.5 Create `Ingredient` aggregate root with factory method `Create()`, domain method `UpdateBrands()`
- [x] 3.6 Create `IngredientCategory` aggregate root with factory method `Create()`
- [x] 3.7 Create domain events: `DrinkCreatedEvent`, `DrinkDeletedEvent`

## 4. Persistence

- [x] 4.1 Create `DrinkCatalogDbContext` implementing `IUnitOfWork`, register `DomainEventInterceptor` via options factory
- [x] 4.2 Configure Drink entity mapping (value object conversions, unique index on Name, soft delete query filter, DrinkTag join table)
- [x] 4.3 Configure Tag, Ingredient, IngredientCategory entity mappings (value object conversions, unique indexes)
- [x] 4.4 Configure RecipeEntry mapping (composite key, value object conversion for Quantity)
- [x] 4.5 Configure `DomainEventRecord` mapping in `drink_catalog.domain_events` table
- [x] 4.6 Map `Ingredient.NotableBrands` (`List<string>`) to `jsonb` column
- [x] 4.7 Create repository interfaces: `IDrinkRepository` (extends generic with paged queries, search, exists-by-name), `ITagRepository`, `IIngredientRepository`, `IIngredientCategoryRepository`
- [x] 4.8 Implement `DrinkRepository` (internal sealed, loads complete aggregate with Include chains)
- [x] 4.9 Implement `TagRepository`, `IngredientRepository`, `IngredientCategoryRepository`
- [x] 4.10 Create `DrinkCatalogDbContextFactory` for design-time migrations
- [x] 4.11 Generate initial EF Core migration

## 5. Contracts (DTOs, Queries, Commands)

- [x] 5.1 Create DTOs: `DrinkDto`, `DrinkDetailDto`, `TagDto`, `IngredientDto`, `IngredientCategoryDto`, `RecipeEntryDto`
- [x] 5.2 Create pagination types: `PagedRequest`, `PagedResult<T>`
- [x] 5.3 Create queries: `GetDrinksQuery`, `GetDrinkByIdQuery`, `SearchDrinksQuery`, `GetTagsQuery`, `GetIngredientCategoriesQuery`, `GetIngredientsQuery`
- [x] 5.4 Create commands: `CreateDrinkCommand`, `UpdateDrinkCommand`, `DeleteDrinkCommand`, `CreateTagCommand`, `DeleteTagCommand`, `CreateIngredientCategoryCommand`, `CreateIngredientCommand`, `UpdateIngredientCommand`

## 6. Query Handlers

- [x] 6.1 Implement `GetDrinksHandler` (paginated, optional tag filter, ordered by name)
- [x] 6.2 Implement `GetDrinkByIdHandler` (full detail with recipe, ingredients, notable brands)
- [x] 6.3 Implement `SearchDrinksHandler` (case-insensitive partial match on name, description, tag, ingredient)
- [x] 6.4 Implement `GetTagsHandler` (list all tags)
- [x] 6.5 Implement `GetIngredientCategoriesHandler` (list ordered by name)
- [x] 6.6 Implement `GetIngredientsHandler` (list, optional category filter)

## 7. Command Handlers

- [x] 7.1 Implement `CreateDrinkHandler` (use Drink.Create factory, duplicate name check, associate tags, add recipe entries, save via UoW)
- [x] 7.2 Implement `UpdateDrinkHandler` (load via repository, call domain Update/SetTags/SetRecipeEntries, duplicate name check, save via UoW)
- [x] 7.3 Implement `DeleteDrinkHandler` (load via repository, call SoftDelete, save via UoW)
- [x] 7.4 Implement `CreateTagHandler` (use Tag.Create factory, duplicate name check, save via UoW)
- [x] 7.5 Implement `DeleteTagHandler` (load via repository, reject if referenced by active drinks, save via UoW)
- [x] 7.6 Implement `CreateIngredientCategoryHandler` (use factory, duplicate name check, save via UoW)
- [x] 7.7 Implement `CreateIngredientHandler` (use factory, validate category exists, duplicate name check, save via UoW)
- [x] 7.8 Implement `UpdateIngredientHandler` (load via repository, call UpdateBrands, save via UoW)

## 8. Module Registration & Endpoints

- [x] 8.1 Create `DrinkCatalogModule.cs` with `AddDrinkCatalogModule()` — register DbContext, repositories, DomainEventInterceptor
- [x] 8.2 Create `DrinkCatalogEndpoints.cs` — map all routes under `/api/drink-catalog` via `MapGroup()`
- [x] 8.3 Register module in Host `Program.cs` — AddMediator, AddDrinkCatalogModule, MigrateAsync (dev), MapDrinkCatalogEndpoints

## 9. Unit Tests

- [x] 9.1 Create `AlCopilot.DrinkCatalog.Tests` project (xUnit, Shouldly, NSubstitute), add to solution under `/Tests/`
- [x] 9.2 Unit tests for value objects: DrinkName, TagName, IngredientName, CategoryName, Quantity, ImageUrl (valid creation, rejection of invalid input)
- [x] 9.3 Unit tests for Drink aggregate: Create factory raises event, Update changes fields, SoftDelete sets flag + raises event, SetTags replaces tags, SetRecipeEntries replaces entries
- [x] 9.4 Unit tests for Tag, Ingredient, IngredientCategory aggregates (factory methods, domain methods)
- [x] 9.5 Unit tests for `GetDrinksHandler` (tag filtering, pagination, empty results)
- [x] 9.6 Unit tests for `GetDrinkByIdHandler` (found with full recipe, not found returns null)
- [x] 9.7 Unit tests for `SearchDrinksHandler` (matching by name/ingredient/tag, no results)
- [x] 9.8 Unit tests for `GetTagsHandler`, `GetIngredientCategoriesHandler`, `GetIngredientsHandler`
- [x] 9.9 Unit tests for `CreateDrinkHandler` (valid creation, duplicate name throws)
- [x] 9.10 Unit tests for `UpdateDrinkHandler` (update details, replace tags/recipe, not found, duplicate name)
- [x] 9.11 Unit tests for `DeleteDrinkHandler` (soft delete, not found)
- [x] 9.12 Unit tests for `CreateTagHandler` (create, duplicate throws)
- [x] 9.13 Unit tests for `DeleteTagHandler` (unreferenced delete, referenced throws)
- [x] 9.14 Unit tests for `CreateIngredientCategoryHandler`, `CreateIngredientHandler`, `UpdateIngredientHandler`
- [x] 9.15 Unit tests for `DomainEventInterceptor` (events collected, records persisted, handlers dispatched, loop terminates at max depth)

## 10. Integration Tests

- [x] 10.1 Create TestContainers PostgreSQL shared fixture (`IAsyncLifetime`, runs migrations on startup)
- [x] 10.2 Integration tests for browse drinks (all, by tag, multiple tags AND logic, pagination, soft-deleted excluded)
- [x] 10.3 Integration tests for drink details (found with full recipe + notable brands, not found, soft-deleted excluded)
- [x] 10.4 Integration tests for search (by name, by ingredient, by tag, no results, soft-deleted excluded, paginated)
- [x] 10.5 Integration tests for drink CRUD (create with recipe + tags, duplicate name rejected, update details/tags/recipe, soft delete)
- [x] 10.6 Integration tests for tag management (create, duplicate rejected, delete unreferenced, delete-blocked-by-reference)
- [x] 10.7 Integration tests for ingredient category management (create, duplicate rejected, list ordered by name)
- [x] 10.8 Integration tests for ingredient management (create with notable brands, update brands, filter by category, duplicate rejected)
- [x] 10.9 Integration tests for domain events (DrinkCreatedEvent persisted on create, DrinkDeletedEvent persisted on soft delete)

## 11. Architecture Tests

- [x] 11.1 Create `AlCopilot.Architecture.Tests` project (xUnit, NetArchTest.eNhancedEdition), add to solution under `/Tests/`
- [x] 11.2 Test: DrinkCatalog module does not reference other module implementations
- [x] 11.3 Test: Contracts project contains only interfaces, DTOs, records, and Mediator message types
- [x] 11.4 Test: All classes in DrinkCatalog module are sealed unless they are base types in Shared
- [x] 11.5 Test: Handlers do not reference DbContext directly (must use IRepository + IUnitOfWork)
