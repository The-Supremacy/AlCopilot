# cross-module-integration-event-consumption Specification

## Purpose

TBD - created by archiving change durable-intermodule-communication. Update Purpose after archive.

## Requirements

### Requirement: Integration Event Contracts SHALL Be Defined In The Publishing Module Contracts Project

The system SHALL define durable integration event contracts in the publishing module's `Contracts` project so future subscribers depend on stable contracts rather than implementation assemblies.

#### Scenario: Future subscriber references only contract types

- **WHEN** another module later implements a durable consumer for the publishing module's integration event
- **THEN** that subscriber SHALL reference the publisher's `Contracts` project event type and SHALL NOT depend on the publisher's internal aggregate or persistence types

#### Scenario: Published event payload remains interoperable

- **WHEN** an integration event is emitted to the transport
- **THEN** the serialized payload SHALL contain portable JSON data that a non-.NET consumer could interpret without requiring Rebus-specific envelope types in the message body

### Requirement: Durable Publishing SHALL Not Depend On A Concrete Subscriber

The system SHALL publish integration events durably even when no concrete subscriber module is implemented yet in the current solution.

#### Scenario: Persisted publisher event is transport-ready without a subscriber

- **WHEN** a publishing module commits an aggregate change that emits an integration event mapped to a durable contract
- **THEN** the outbox worker SHALL publish the event through Rebus using the contract type without requiring any in-solution subscriber module to be present

#### Scenario: Subscriber idempotency remains a follow-up concern

- **WHEN** a future change adds one or more concrete subscribers for the published integration event
- **THEN** that change SHALL define the subscriber-side idempotency behavior needed for at-least-once delivery
