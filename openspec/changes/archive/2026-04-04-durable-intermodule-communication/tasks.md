## 1. Shared Outbox Infrastructure

- [x] 1.1 Add Rebus and related durable messaging dependencies to central package management.
- [x] 1.2 Extend shared domain event persistence with `DispatchedAtUtc` and stable logical event name support.
- [x] 1.3 Add shared outbox source registration infrastructure for module `DbContext` discovery.

## 2. Host And AppHost Messaging Runtime

- [x] 2.1 Configure the Aspire AppHost to run the Azure Service Bus emulator and pass messaging connection details to the Host.
- [x] 2.2 Configure a single Host-level Rebus bus using Azure Service Bus with logical message type and topic naming conventions.
- [x] 2.3 Implement the Host `OutboxWorker` to poll registered sources, publish undispatched events, and mark rows dispatched after successful publish.

## 3. Module Integration And Persistence

- [x] 3.1 Register Drink Catalog as an outbox source from `AddDrinkCatalogModule()`.
- [x] 3.2 Update Drink Catalog persistence mapping and migrations for `DispatchedAtUtc` tracking and undispatched-row indexing.
- [x] 3.3 Wire domain event registry usage so persisted and published events use the contracts assembly event types consistently.

## 4. Verification

- [x] 4.1 Add unit tests for outbox source registration, logical naming, and durable messaging service registration.
- [x] 4.2 Add worker and transport integration tests covering publish success, dispatch marking, and retry-safe failure behavior.
- [x] 4.3 Verify the end-to-end server solution with build and test execution.
