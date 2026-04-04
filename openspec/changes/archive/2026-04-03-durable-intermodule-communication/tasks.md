## 1. Shared Outbox Infrastructure

- [x] 1.1 Add Rebus package versions to `server/Directory.Packages.props` and update the affected server project references
- [x] 1.2 Extend `AlCopilot.Shared` `DomainEventRecord` to include nullable `DispatchedAtUtc` dispatch tracking
- [x] 1.3 Update shared domain event persistence mappings and guidance so module `domain_events` tables support efficient undispatched-row queries
- [x] 1.4 Implement shared `OutboxSourceDescriptor` infrastructure and `AddOutboxSource(...)` registration extensions for module composition
- [x] 1.5 Add unit tests for logical event-name resolution and missing-type failure behavior in `DomainEventTypeRegistry`

## 2. Publishing Module Persistence

- [x] 2.1 Update the publishing module `DbContext` mapping for `domain_events` to persist `DispatchedAtUtc` and the undispatched lookup/index shape
- [x] 2.2 Register the publishing module outbox source from its `AddXxxModule()` entry point using the shared descriptor infrastructure
- [x] 2.3 Create and review the publishing module EF Core migration for the outbox schema changes
- [x] 2.4 Add unit tests covering new `DomainEventRecord` rows starting undispatched and remaining retryable after publish failures

## 3. Host Messaging Runtime

- [x] 3.1 Configure a single Rebus bus in `AlCopilot.Host` with Azure Service Bus transport and logical message type naming backed by `DomainEventTypeRegistry`
- [x] 3.2 Implement the Host-level `OutboxWorker` `BackgroundService` that resolves registered sources in scoped lifetimes, polls undispatched rows, publishes events, and marks successful rows with `DispatchedAtUtc`
- [x] 3.3 Add structured logging and failure handling so deserialization or publish errors leave rows undispatched without crashing the worker
- [x] 3.4 Register the worker and related messaging services in Host startup without changing the synchronous Mediator flow
- [x] 3.5 Add unit tests for worker dispatch success, skipped already-dispatched rows, deserialization failures, and publish retry behavior

## 4. Aspire And Local Transport Wiring

- [x] 4.1 Configure `AlCopilot.AppHost` to run the Azure Service Bus emulator and companion infrastructure for local development
- [x] 4.2 Pass the Service Bus connection details from AppHost into `AlCopilot.Host` so the production transport path is exercised locally
- [x] 4.3 Verify local startup documentation or configuration defaults still align with the approved architecture for transport-backed development

## 5. Integration Event Contracts

- [x] 5.1 Add the first durable cross-module integration event contract to the publishing module `Contracts` project with a stable logical event name
- [x] 5.2 Publish that contract event from the outbox pipeline without exposing publisher implementation types to future subscribers
- [x] 5.3 Add unit tests covering contract event publication and transport-facing payload shape

## 6. Integration And Architecture Verification

- [x] 6.1 Add transport-backed integration test fixtures using TestContainers Postgres plus the Azure Service Bus emulator for Host-level end-to-end tests
- [x] 6.2 Add an integration test proving a committed domain event is published later by the outbox worker and marked dispatched after successful delivery
- [x] 6.3 Add an integration test proving publish failures leave the outbox row eligible for retry and prefer duplicate delivery over message loss
- [x] 6.4 Add an integration test proving the published contract event reaches the transport with the expected logical name and payload shape
- [x] 6.5 Add or update architecture tests to enforce contracts-only integration event definitions and preserve module boundary rules
- [x] 6.6 Run the relevant server unit, integration, and architecture test suites to verify the full durable intermodule communication slice
