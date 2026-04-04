## ADDED Requirements

### Requirement: Modules SHALL Register Outbox Sources Through Shared Infrastructure

The system SHALL allow each participating module to register its own outbox source descriptor from `AddXxxModule()` so the Host can discover durable publishing sources without hardcoded module-specific branching.

#### Scenario: Module registers its outbox source during composition

- **WHEN** a module that persists domain events is added to the Host service collection
- **THEN** the module SHALL register an outbox source descriptor containing the owning `DbContext` type needed by the worker to resolve the module's mapped `DomainEventRecord` set

#### Scenario: Host resolves all registered sources at runtime

- **WHEN** the Host starts the outbox worker
- **THEN** the worker SHALL enumerate all registered outbox source descriptors and SHALL poll each source using a fresh scoped `DbContext` resolution

### Requirement: Domain Event Persistence SHALL Support Dispatch Tracking Queries

The system SHALL extend the shared domain event persistence model to record dispatch completion time and SHALL support efficient lookup of undispatched rows.

#### Scenario: Newly persisted rows start undispatched

- **WHEN** `DomainEventInterceptor` persists a new `DomainEventRecord`
- **THEN** the row SHALL have `DispatchedAtUtc` set to `NULL` until the outbox worker successfully publishes it

#### Scenario: Publishing query targets undispatched rows first

- **WHEN** the outbox worker queries a module outbox source for pending work
- **THEN** the database mapping SHALL support ordering and filtering by undispatched rows so the worker can process pending events without scanning only-by-time historical records
