# Server Conventions (.NET Backend)

## Architecture Reference

Read [docs/architecture.md](../docs/architecture.md) for full architecture, design decisions, and rationale.

## Stack

- **.NET 10** with **Aspire** for orchestration
- **EF Core** + **PostgreSQL** — one `DbContext` and schema per module
- **Mediator** (Martin Othamar, source-generated) — NOT MediatR
- **Rebus** for async messaging (when needed) — NOT MassTransit, NOT Wolverine

## Modular Monolith Rules

- Each module is a separate class library project under `server/src/`
- Each module owns its own `DbContext` with a dedicated Postgres schema
- Module entry point: `Add*Module(this IServiceCollection)` extension method in `*Module.cs`
- No cross-module EF entity references — use IDs only

### Contracts Pattern (Cross-Module Communication)

- Each module has a **Contracts** project: `AlCopilot.{Module}.Contracts`
- Contracts contain: interfaces, request/response DTOs, events, and shared models
- Contracts do NOT contain: EF entities, handlers, internal services, or implementation details
- Modules reference other modules' **Contracts** projects only — never the module implementation directly
- **Mediator** dispatches requests defined in Contracts — the handler lives in the module itself
- Example: `Catalog.Contracts` defines `GetDrinkByIdQuery` → `Catalog` contains the handler → `Recommendation` references `Catalog.Contracts` and sends the query via Mediator

### Container Publishing

- .NET SDK container support (`Microsoft.NET.Build.Containers`) — no Dockerfile needed
- Host project sets `<EnableSdkContainerSupport>true</EnableSdkContainerSupport>`
- Publish via `dotnet publish --os linux --arch x64 /t:PublishContainer`

### BFF & Authentication

- `AlCopilot.Host` is the BFF — it handles OIDC code flow with Keycloak and issues `HttpOnly; Secure; SameSite=Strict` cookies
- Tokens (access, refresh, id) are stored server-side — the SPA never sees them
- Keycloak runs locally via Aspire AppHost; in production as a service in AKS
- When a module is extracted, add YARP reverse proxy routes in the Host — the frontend changes nothing

## Project Structure Conventions

- `server/Directory.Build.props` — shared MSBuild properties and `<Version>`
- `server/Directory.Packages.props` — Central Package Management (all NuGet versions)
- `server/AlCopilot.slnx` — solution file
- Source projects: `server/src/AlCopilot.{ProjectName}/`
- Contract projects: `server/src/AlCopilot.{ProjectName}.Contracts/`
- Test projects: `server/tests/AlCopilot.{ProjectName}.Tests/`

## Code Style

- `TreatWarningsAsErrors` is enabled — all warnings must be resolved
- Nullable reference types enabled
- Follow .NET naming conventions (PascalCase for public, \_camelCase for private fields)
- Async methods suffixed with `Async`
- Use `sealed` on classes that are not designed for inheritance

## Domain-Driven Design (DDD)

### Aggregates & Entities

- Each module follows DDD principles: aggregates, value objects, repositories, domain services
- **Aggregate roots** are the only entities loaded/saved by repositories — loading always fetches the complete aggregate
- Child entities (e.g., `RecipeEntry` within `Drink`) are part of the aggregate and never accessed independently
- All domain logic lives in the aggregate or a domain service — NOT in handlers
- Handlers orchestrate: load aggregate via repository, call domain methods, save via unit of work
- Cross-aggregate references use IDs only — never navigation properties across aggregate boundaries

### Base Types (`AlCopilot.Shared`)

- `AggregateRoot<TId>` — base class with `Id`, `DomainEvents` list, protected `Raise(IDomainEvent)` method
- `Entity<TId>` — base class for child entities within an aggregate
- `ValueObject<T>` — base class with `Value` property, implicit conversion to `T`, equality by value. Used for validated primitives (e.g., `DrinkName`, `Quantity`)
- `IDomainEvent` — marker interface for domain events
- `IDomainEventHandler<T>` — handler interface for synchronous in-process domain event reactions

### Value Objects

- Prefer value objects for any property with validation rules (length limits, format, non-empty)
- Value objects validate in their constructor/factory — invalid values are structurally impossible
- EF Core mapping via `HasConversion(v => v.Value, raw => TypeName.Create(raw))`
- Keep validation close to the domain — do NOT rely on EF or API-level validation alone

### Repositories

- `IRepository<TRoot, TId>` — generic interface: `GetByIdAsync`, `Add`, `Remove`
- One repository per aggregate root
- Repository loads the **complete aggregate** (all children, value objects) — no lazy loading
- Repository implementations are `internal sealed` classes in the module, wrapping the module's `DbContext`
- Specific interfaces (e.g., `IDrinkRepository`) extend the generic when extra query methods are needed

### Unit of Work

- `IUnitOfWork` — interface with `SaveChangesAsync`
- Module `DbContext` implements `IUnitOfWork`
- Handlers call `IUnitOfWork.SaveChangesAsync` once at the end — no mid-handler saves
- Domain event dispatch happens inside the `SaveChanges` interceptor (before commit)

### Domain Events

- Aggregates raise domain events via `Raise(new SomeEvent(...))` — events accumulate in `DomainEvents` list
- A `SaveChangesInterceptor` processes events in a **dispatch-before-commit loop**:
  1. Collect events from tracked aggregates, clear their lists
  2. Persist `DomainEventRecord` rows to the module's `domain_events` table
  3. Dispatch each event to registered `IDomainEventHandler<T>` implementations (in-process, same scope)
  4. Repeat (handlers may cause new events) — max depth 5, throw if exceeded
  5. Final `SaveChanges` commits everything atomically (state + events)
- **Same-module** event handlers: synchronous, same transaction (via the interceptor loop)
- **Cross-module** event handlers: always eventual consistency via the outbox (`IsPublished = false` → background worker → Rebus)
- `DomainEventRecord` schema: `Id (long)`, `AggregateId (Guid)`, `AggregateType`, `EventType`, `Payload (jsonb)`, `OccurredAtUtc`, `Sequence`, `IsPublished`

### Shared Project Structure

```
server/src/AlCopilot.Shared/
  Domain/
    IAggregateRoot.cs
    AggregateRoot.cs
    Entity.cs
    IDomainEvent.cs
    IDomainEventHandler.cs
    ValueObject.cs
  Data/
    IRepository.cs
    IUnitOfWork.cs
    DomainEventInterceptor.cs
    DomainEventRecord.cs
```

## Testing

- **xUnit** for test framework
- **Shouldly** for assertions — NOT FluentAssertions (commercial)
- **NSubstitute** for mocking — NOT Moq (SponsorLink controversy)
- **TestContainers** (Postgres) for integration tests — NOT in-memory EF providers
- **NetArchTest.eNhanced** for architecture tests
- Unit + integration tests in the same project: `AlCopilot.{Module}.Tests`
- Integration tests marked with `[Trait("Category", "Integration")]`
- Architecture tests in `AlCopilot.Architecture.Tests`
- Test classes are `sealed`
- Use primary constructors for fixture injection

## Review Checklist

When reviewing .NET code, verify:

- [ ] No cross-module EF entity references (use IDs only)
- [ ] Cross-module communication uses Contracts projects
- [ ] Contracts contain only: interfaces, DTOs, events, shared models
- [ ] Module registration via `Add{Module}Module(this IServiceCollection)` extension
- [ ] Each module has its own DbContext with dedicated schema
- [ ] Uses correct libraries (Mediator, Rebus, Shouldly, NSubstitute, TestContainers)
- [ ] Classes are `sealed` unless designed for inheritance
- [ ] Async methods suffixed with `Async`
- [ ] NuGet versions managed centrally in `Directory.Packages.props`
- [ ] Test classes are `sealed` with primary constructors
- [ ] Integration tests marked with `[Trait("Category", "Integration")]`
- [ ] Aggregates use `AggregateRoot<TId>` base, value objects use `ValueObject<T>` base
- [ ] Domain logic in aggregates/domain services — NOT in handlers
- [ ] Handlers use `IRepository` + `IUnitOfWork` — NOT `DbContext` directly
- [ ] Value objects for validated primitives (names, quantities, etc.)
