## 1. Project Setup & Dependencies

- [ ] 1.1 Add EF Core, Npgsql, and Mediator packages to `server/Directory.Packages.props`
- [ ] 1.2 Create `AlCopilot.Catalog` class library project in `server/src/Modules/AlCopilot.Catalog/`
- [ ] 1.3 Create `AlCopilot.Catalog.Contracts` class library project in `server/src/Modules/AlCopilot.Catalog.Contracts/`
- [ ] 1.4 Add both projects to `server/AlCopilot.slnx`
- [ ] 1.5 Add Postgres resource to Aspire AppHost and pass connection to Host

## 2. Domain Entities & DbContext

- [ ] 2.1 Define `Category` entity (Id, Name, CreatedAtUtc)
- [ ] 2.2 Define `Ingredient` entity (Id, Name, CreatedAtUtc)
- [ ] 2.3 Define `Drink` entity (Id, Name, Description, ImageUrl, CategoryId, IsDeleted, DeletedAtUtc, CreatedAtUtc)
- [ ] 2.4 Define `DrinkIngredient` join entity (DrinkId, IngredientId, Quantity)
- [ ] 2.5 Create `CatalogDbContext` with `catalog` schema and entity configurations
- [ ] 2.6 Add global query filter for soft-deleted drinks
- [ ] 2.7 Generate initial EF Core migration

## 3. Contracts

- [ ] 3.1 Define request/response DTOs: `DrinkDto`, `DrinkDetailDto`, `CategoryDto`, `IngredientDto`
- [ ] 3.2 Define pagination DTOs: `PagedRequest`, `PagedResult<T>`
- [ ] 3.3 Define Mediator queries: `GetDrinksQuery`, `GetDrinkByIdQuery`, `SearchDrinksQuery`, `GetCategoriesQuery`, `GetIngredientsQuery`
- [ ] 3.4 Define Mediator commands: `CreateDrinkCommand`, `UpdateDrinkCommand`, `DeleteDrinkCommand`, `CreateCategoryCommand`, `CreateIngredientCommand`

## 4. Query & Command Handlers

- [ ] 4.1 Implement `GetDrinksHandler` (browse with optional category filter + pagination)
- [ ] 4.2 Implement `GetDrinkByIdHandler` (drink details with ingredients)
- [ ] 4.3 Implement `SearchDrinksHandler` (ILIKE search across name, description, ingredients)
- [ ] 4.4 Implement `GetCategoriesHandler` (list categories with drink counts)
- [ ] 4.5 Implement `GetIngredientsHandler` (list all ingredients)
- [ ] 4.6 Implement `CreateDrinkHandler` (with duplicate name check)
- [ ] 4.7 Implement `UpdateDrinkHandler` (update details + replace ingredients)
- [ ] 4.8 Implement `DeleteDrinkHandler` (soft delete)
- [ ] 4.9 Implement `CreateCategoryHandler` (with duplicate name check)
- [ ] 4.10 Implement `CreateIngredientHandler`

## 5. Module Registration & API Endpoints

- [ ] 5.1 Create `CatalogModule.cs` with `AddCatalogModule(this IServiceCollection)` extension method
- [ ] 5.2 Create `CatalogEndpoints.cs` with `MapCatalogEndpoints(this IEndpointRouteBuilder)` mapping all routes under `/api/catalog`
- [ ] 5.3 Register Catalog module and map endpoints in Host `Program.cs`

## 6. Testing

- [ ] 6.1 Create `AlCopilot.Catalog.Tests` project in `server/tests/` with TestContainers PostgreSQL fixture
- [ ] 6.2 Write integration tests for browse drinks (by category, all, pagination, empty)
- [ ] 6.3 Write integration tests for drink details (found, not found)
- [ ] 6.4 Write integration tests for search (by name, by ingredient, empty query, no results)
- [ ] 6.5 Write integration tests for drink CRUD (create, duplicate, update, soft delete)
- [ ] 6.6 Write integration tests for category and ingredient management

## 7. Architecture Tests

- [ ] 7.1 Create `AlCopilot.Architecture.Tests` project in `server/tests/`
- [ ] 7.2 Add test: Catalog module does not reference other module implementations
- [ ] 7.3 Add test: Contracts project contains only interfaces, DTOs, and Mediator message types
- [ ] 7.4 Add test: All classes in Catalog module are sealed (unless explicitly designed for inheritance)
