## ADDED Requirements

### Requirement: Persisted Domain Events SHALL Be Published Durably

The system SHALL treat persisted `DomainEventRecord` rows with `DispatchedAtUtc` equal to `NULL` as pending outbox messages and SHALL publish them asynchronously after the originating database transaction commits.

#### Scenario: Worker publishes committed domain events

- **WHEN** a module commits aggregate state together with one or more new `DomainEventRecord` rows
- **THEN** the Host-level outbox worker SHALL read the undispatched rows later and publish the corresponding integration events through Rebus without requiring the original request scope to remain alive

#### Scenario: Worker marks a row dispatched only after publish succeeds

- **WHEN** the outbox worker successfully publishes an event to the configured transport
- **THEN** the worker SHALL set `DispatchedAtUtc` for that row and persist the update in the publishing module's database

#### Scenario: Publish failure leaves a row eligible for retry

- **WHEN** transport publication fails or the worker cannot persist the dispatch timestamp
- **THEN** the `DomainEventRecord` row SHALL remain eligible for later reprocessing and the system SHALL prefer duplicate delivery over message loss

### Requirement: Published Integration Events SHALL Use Stable Logical Names

The system SHALL derive transport-facing message type names from the logical domain event naming infrastructure rather than CLR type names or assembly-qualified names.

#### Scenario: Message naming uses the registered logical event name

- **WHEN** the outbox worker publishes an integration event whose CLR type is annotated with a logical domain event name and version
- **THEN** the transport metadata SHALL use the resolved logical name in the form `<domain-event-name>.v<version>` so refactoring namespaces or assemblies does not change the wire contract

#### Scenario: Unregistered event types fail deterministically

- **WHEN** the outbox worker attempts to deserialize or publish a persisted event type that is missing from `DomainEventTypeRegistry`
- **THEN** the worker SHALL log the failure and SHALL leave the row undispatched instead of publishing an event with an unstable fallback name
