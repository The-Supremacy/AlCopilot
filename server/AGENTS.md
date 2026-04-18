# Server Conventions (.NET Backend)

## Architecture Reference

Read [docs/constitution.md](../docs/constitution.md) for project-wide governance and workflow rules.
Read [docs/constitution/server.md](../docs/constitution/server.md) for backend workflow and quality expectations.
Read [docs/architecture.md](../docs/architecture.md) for the thin project-wide architecture index.
Read [docs/architecture/server.md](../docs/architecture/server.md) for backend architecture details and rationale.
Read [docs/adr/0002-domain-driven-design-patterns.md](../docs/adr/0002-domain-driven-design-patterns.md) for the accepted backend DDD defaults.
Read [docs/testing.md](../docs/testing.md) for the thin project-wide testing index.
Read [docs/testing/server.md](../docs/testing/server.md) for backend test taxonomy, ownership, and placement rules.

## Stack

- **.NET 10** with **Aspire** for orchestration
- **EF Core** + **PostgreSQL** — one `DbContext` and schema per module
- **Mediator** (Martin Othamar, source-generated) — NOT MediatR
- Durable async messaging is deferred; if it becomes necessary, follow [ADR 0001](../docs/adr/0001-durable-intermodule-messaging.md)

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

## Project Structure Conventions

- `server/Directory.Build.props` — shared MSBuild properties and `<Version>`
- `server/Directory.Packages.props` — Central Package Management (all NuGet versions)
- `server/AlCopilot.slnx` — solution file
- Source projects: `server/src/AlCopilot.{ProjectName}/`
- Contract projects: `server/src/AlCopilot.{ProjectName}.Contracts/`
- Test projects: `server/tests/AlCopilot.{ProjectName}.Tests/`
- Inside module features, place feature-local interfaces under `Features/{FeatureName}/Abstractions`
- Add deeper subfolders such as `QueryServices`, `Repositories`, `Workflows`, or `{AggregateName}` only when feature complexity clearly benefits from them

## Code Style

- `TreatWarningsAsErrors` is enabled — all warnings must be resolved
- Nullable reference types enabled
- Follow .NET naming conventions (PascalCase for public, \_camelCase for private fields)
- Async methods suffixed with `Async`
- Use `sealed` on classes that are not designed for inheritance

## Domain-Driven Design (DDD)

### Aggregates & Entities

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
- Use one repository-local canonical aggregate query/helper for aggregate-returning load methods instead of repeating `Include` chains
- Repository implementations are `internal sealed` classes in the module, wrapping the module's `DbContext`
- Aggregate repositories stay command-focused; read-side DTO projection belongs in explicit query services

### Query Services

- Use explicit query services for contract DTOs, paging, and consumer-specific read models
- Query handlers should depend on query services rather than aggregate repositories
- Query services may project across aggregate boundaries inside the same module when needed for a read model
- Do not force list/detail read scenarios through aggregate-loading patterns just to mirror command-side DDD structure

### Application Services

- Prefer interface-first DI for orchestration-style application services that handlers coordinate through, such as workflow processors, audit writers, import processors, or other non-aggregate collaborators
- Aggregate repositories and aggregate methods remain the main exception because they already express domain boundaries directly
- If a handler only needs behavior from a collaborator, depend on the interface in the handler and register the concrete implementation behind that contract
- Do not add interfaces around trivial private helpers or one-off glue code without a real behavior boundary

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
- **Cross-module** async messaging is deferred until there is a concrete approved use case
- `DomainEventRecord` schema: `Id (long)`, `AggregateId (Guid)`, `AggregateType`, `EventType`, `Payload (jsonb)`, `OccurredAtUtc`
- `EventType` stores a logical name from `[DomainEventName]` attribute (e.g., `drink-catalog.drink-created.v1`), not the CLR type name
- `DomainEventTypeRegistry` provides bidirectional `Type ↔ string` lookup, built at startup from module assemblies
- Indexes: `(AggregateId, Id)` for per-aggregate audit/replay, `(OccurredAtUtc)` for time-range queries
- Treat preserved domain events as machine-readable history and a same-module reaction hook; they do not replace explicit operator-facing audit records when workflow audit needs richer semantics

### Cross-Module Communication

- Use Mediator commands and queries through Contracts projects for current cross-module orchestration.
- Do not add async outbox-based choreography until there is a concrete approved use case, following [ADR 0001](../docs/adr/0001-durable-intermodule-messaging.md).

### Shared Project Structure

```
server/src/AlCopilot.Shared/
  Domain/
    IAggregateRoot.cs
    AggregateRoot.cs
    Entity.cs
    IDomainEvent.cs
    IDomainEventHandler.cs
    DomainEventNameAttribute.cs
    DomainEventTypeRegistry.cs
    DomainEventRegistryServiceCollectionExtensions.cs
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
- Architecture tests live in `AlCopilot.Architecture.Tests` only
- Module tests in `AlCopilot.{Module}.Tests` cover unit, application, infrastructure integration, and module-owned HTTP integration tests
- Module tests mock or substitute other module boundaries by default
- Host-level auth, cross-module orchestration, and composition tests live in `AlCopilot.Host.Tests`
- Shared backend HTTP integration infrastructure lives in `server/tests/AlCopilot.Testing.Shared`
- Integration tests that use real infrastructure are marked with `[Trait("Category", "Integration")]`
- Prefer one file per handler or behavior entry point under test
- Test classes are `sealed`
- Use primary constructors for fixture injection

## Review Checklist

When reviewing .NET code, verify:

- [ ] No cross-module EF entity references (use IDs only)
- [ ] Cross-module communication uses Contracts projects
- [ ] Contracts contain only: interfaces, DTOs, events, shared models
- [ ] Module registration via `Add{Module}Module(this IServiceCollection)` extension
- [ ] Each module has its own DbContext with dedicated schema
- [ ] Uses correct libraries (Mediator, Shouldly, NSubstitute, TestContainers)
- [ ] Classes are `sealed` unless designed for inheritance
- [ ] Async methods suffixed with `Async`
- [ ] NuGet versions managed centrally in `Directory.Packages.props`
- [ ] Test classes are `sealed` with primary constructors
- [ ] Integration tests marked with `[Trait("Category", "Integration")]`
- [ ] Aggregates use `AggregateRoot<TId>` base, value objects use `ValueObject<T>` base
- [ ] Domain logic in aggregates/domain services — NOT in handlers
- [ ] Handlers use `IRepository` + `IUnitOfWork` — NOT `DbContext` directly
- [ ] Query handlers use query services instead of aggregate repositories for DTO projection paths
- [ ] Aggregate repositories do not return contract DTOs
- [ ] Value objects for validated primitives (names, quantities, etc.)
