## Context

The codebase already persists `DomainEventRecord` rows atomically during `SaveChangesAsync` through `DomainEventInterceptor`.
That gives us the producer-side half of an outbox, but not durable delivery.
There is currently no message bus configured in `AlCopilot.Host`, no Azure Service Bus emulator wiring in `AlCopilot.AppHost`, no worker that publishes undispatched events, and no module-level registration pattern for outbox sources.

At the same time, the shared DDD infrastructure is already moving toward the target design.
`DomainEventTypeRegistry` and `[DomainEventName]` provide stable logical event names, and `DomainEventRecord` has already been simplified away from the older `Sequence` and `IsPublished` fields.
This change completes that transition by adding explicit dispatch tracking and the runtime plumbing required to move persisted domain events across module boundaries durably.

This is a cross-cutting change.
It affects shared abstractions, Host startup, Aspire orchestration, EF Core persistence, and test infrastructure.
Because it introduces a new transport and modifies the persistence contract of domain events, a design artifact is warranted before implementation.

## Goals / Non-Goals

**Goals:**

- Establish one durable async intermodule communication path for the modular monolith
- Preserve the architecture split between Mediator for synchronous in-process calls and Rebus for durable cross-module choreography
- Reuse the existing `domain_events` table as the outbox instead of introducing a second persistence mechanism
- Standardize module registration of outbox sources so future modules can participate without Host-specific branching
- Use stable logical event names for serialization and transport metadata
- Keep local development and automated tests aligned with production transport choices by using the Azure Service Bus emulator
- Introduce the smallest viable delivery model: single Host-level worker, at-least-once delivery, and consumer-side idempotency

**Non-Goals:**

- Replacing Mediator-based orchestration for request/response module interactions
- Introducing sagas, workflow engines, or distributed transaction semantics
- Adding a dedicated per-consumer dispatch ledger in the publishing database
- Designing a generalized poison-message recovery system in this iteration
- Refactoring repository patterns or feature-folder organization beyond what this capability needs
- Adding speculative integration-event consumers where no actual subscriber use case exists yet

## Decisions

### 1. Use `DomainEventRecord` as the only publisher outbox

The existing interceptor already persists domain events atomically in the same transaction as aggregate state changes.
Rather than duplicating that data into a separate outbox table, this design treats `domain_events` as the outbox itself.

`DomainEventRecord` will gain a nullable `DispatchedAtUtc` column.
Records with `DispatchedAtUtc IS NULL` are pending publication.
The worker sets the timestamp only after a successful publish call completes.

This keeps write-side behavior simple:

1. Aggregate raises domain events
2. `DomainEventInterceptor` persists them as `DomainEventRecord`
3. Transaction commits aggregate state and outbox rows atomically
4. `OutboxWorker` publishes committed rows later

Why this over a separate outbox table:

- The persisted event payload is already the transport-ready source of truth
- It avoids dual-write logic inside the interceptor
- It keeps audit/history and outbox concerns in one record stream

Alternative considered:

- Reintroducing `IsPublished` as a boolean flag. Rejected because `DispatchedAtUtc` gives the same filter shape plus useful observability.

### 2. Keep one `OutboxWorker` in `AlCopilot.Host`

The worker will live in `AlCopilot.Host` as a single `BackgroundService`.
It will resolve all registered outbox sources, poll each source in round-robin fashion, publish eligible events, and update dispatch timestamps.

This centralizes cross-module delivery concerns in the Host, which already composes modules and owns infrastructure-facing runtime concerns.
Modules remain responsible only for:

- raising domain events
- persisting them through their own `DbContext`
- registering themselves as an outbox source

Why this over per-module workers:

- consistent polling and retry behavior in one place
- no duplicated background-service infrastructure per module
- easier observability and transport configuration

Alternative considered:

- One worker per module. Rejected because it adds boilerplate and operational sprawl without any scaling benefit at this stage.

### 3. Introduce explicit outbox source registration in `AlCopilot.Shared`

`AlCopilot.Shared` will define an `OutboxSourceDescriptor` containing:

- `DbContextType`
- a logical source `Name` for logging and diagnostics

An `AddOutboxSource` service-collection extension will register descriptors from each module.
For example, `DrinkCatalogModule` registers its `DrinkCatalogDbContext` and `drink_catalog.domain_events` table.

This gives the Host a stable discovery model for all modules that participate in durable publishing without creating compile-time coupling from Host to module internals. The worker relies on the resolved module `DbContext` and its EF mapping for table/schema details rather than duplicating that metadata in shared registration.

Why this over hardcoding module knowledge in the Host:

- future modules can opt in by extending their own `AddXxxModule()`
- Host remains infrastructure-focused rather than module-specific
- matches the existing assembly-marker pattern used for domain event registration

Alternative considered:

- Having the Host enumerate known module `DbContext` types manually. Rejected because it violates the modular registration pattern and becomes brittle as modules evolve.

### 4. Rebus is configured once in the Host, using Azure Service Bus topics

`AlCopilot.Host` will own the single Rebus bus instance.
The transport is Azure Service Bus.
Events are published to topics named through Rebus message-type conventions backed by `DomainEventTypeRegistry`.

The logical event name flow is:

- event CLR type has `[DomainEventName("drink-catalog.drink-created", version: 1)]`
- registry resolves this to `drink-catalog.drink-created.v1`
- Rebus uses that logical name for message-type metadata and topic resolution

This keeps contracts stable across refactors and avoids leaking CLR type names into the wire contract.

Why this over CLR-based naming:

- wire names remain stable if namespaces or assemblies move
- non-.NET consumers can understand event names
- aligns with the architecture document and existing registry design

Alternative considered:

- letting Rebus use default type naming. Rejected because it produces transport-facing names that are refactor-sensitive and implementation-specific.

### 5. `DomainEventInterceptor` remains responsible only for persistence plus in-process dispatch

The interceptor will continue to:

- collect domain events from tracked aggregates
- persist them into `DomainEventRecord`
- invoke synchronous in-process `IDomainEventHandler<T>` handlers before commit

It will not publish to Rebus directly.
Publishing remains an after-commit concern handled by `OutboxWorker`.

This preserves transaction safety.
If the database transaction rolls back, no external publish should have occurred.
If transport publish fails, the committed row remains pending for retry.

Why this over publishing directly inside the interceptor:

- avoids external side effects before transaction commit
- preserves clean retry semantics
- keeps same-module synchronous handlers separate from cross-module eventual consistency

Alternative considered:

- fire-and-forget publishing during save. Rejected because it risks publishing events for state that never committed.

### 6. Modules stay isolated by `DbContext`, but the worker uses scoped resolution per source

For each polling cycle, the worker will:

1. create a new DI scope
2. resolve the source `DbContext` type from that scope
3. query undispatched `DomainEventRecord` rows ordered by `Id`
4. deserialize payloads through `DomainEventTypeRegistry`
5. publish each event through Rebus
6. set `DispatchedAtUtc`
7. save changes on that `DbContext`

Failures are isolated per record and per source.
If one module database or one event row fails, the worker logs and moves on instead of crashing globally.

Why this over sharing a long-lived context instance:

- EF Core contexts are scoped and not thread-safe
- fresh scopes avoid stale tracking state
- source isolation maps well to module ownership boundaries

Alternative considered:

- using raw SQL and bypassing EF for the worker. Rejected for the first iteration because the data model is already mapped and the worker benefits from consistent module `DbContext` configuration.

### 7. Testing uses the real transport shape, not an alternate production path

Integration tests will use:

- Postgres via TestContainers for the outbox store
- RabbitMQ via TestContainers for automated transport-backed pub/sub verification
- `WebApplicationFactory<Program>` or equivalent test-time host wiring configured with test-time connection strings

Local development and production-oriented runtime wiring continue to use Azure Service Bus. RabbitMQ is limited to automated integration tests because the current Azure Service Bus emulator path is not yet reliable enough for topic/subscription verification in this workflow.

Unit tests will cover:

- event name resolution
- worker dispatch behavior
- skipped already-dispatched rows
- graceful handling of deserialization and publish failures

This keeps test behavior aligned with production transport semantics and avoids introducing test-only service registration branches in the real app startup.

Why this over swapping to an in-memory transport in integration tests:

- Rebus transport replacement in app startup is awkward and fragile
- RabbitMQ-backed tests still exercise real pub/sub behavior, routing, and message serialization
- it provides stable automated coverage for choreography scenarios while Azure Service Bus remains the production transport

Alternative considered:

- Azure Service Bus emulator for automated topic/subscription tests. Deferred because the current emulator path is not reliable enough for this verification target.
- in-memory Rebus for integration tests. Rejected for end-to-end verification, though still acceptable for small unit-level handler tests.

## Risks / Trade-offs

- [Single worker throughput ceiling] -> Accept the initial limit and keep batching plus polling configurable so scaling decisions can be deferred until real load appears.
- [At-least-once delivery can produce duplicates] -> Require consumer idempotency and document it in future integration-event consumer specs.
- [A publish may succeed but the dispatch timestamp save may fail] -> Accept duplicate republish as the safer failure mode; this is consistent with at-least-once semantics.
- [Deserialization failures can strand rows indefinitely] -> Log clearly, leave rows undispatched, and treat poison-message handling as a follow-up capability if needed.
- [Module-by-module migration changes may drift] -> Standardize the `DomainEventRecord` mapping and migration expectations in shared guidance before adding more publishing modules.
- [Service Bus emulator complexity increases test setup] -> Contain that complexity in shared test fixtures and AppHost wiring rather than pushing it into production code paths.

## Migration Plan

1. Add Rebus package references in central package management.
2. Extend shared outbox abstractions in `AlCopilot.Shared`.
3. Update `DomainEventRecord` to include `DispatchedAtUtc`.
4. Update module EF configurations to add the undispatched partial index.
5. Generate and review per-module migrations, starting with `DrinkCatalog`.
6. Register outbox sources from participating modules.
7. Configure Azure Service Bus emulator in `AlCopilot.AppHost`.
8. Configure Rebus and register `OutboxWorker` in `AlCopilot.Host`.
9. Add unit tests, then transport-backed integration tests.
10. Verify end-to-end flow: save aggregate, persist domain event, publish, mark dispatched.

Rollback strategy:

- Remove Host-level Rebus and worker wiring if runtime issues appear before rollout is stabilized.
- Revert the migration that adds `DispatchedAtUtc` and related indexes if the schema change must be backed out in development.
- If rollback occurs after the column exists, leaving `DispatchedAtUtc` unused is safe because it is additive and nullable.

## Open Questions

- Do we want the first implementation to include a real consumer in the same change, or stop at publisher infrastructure plus tests?
- Should dispatch batching be configured globally only, or per source for future tuning?
- Do we want a lightweight operational query or endpoint for undispatched-row counts, or is structured logging sufficient for the first iteration?
