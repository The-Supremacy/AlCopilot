# Spec: Management Actor Traceability

## Feature: Management Audit Identity

### Requirement: Persist Stable Actor Identity For Management Mutations

The system SHALL persist authenticated management actor identity for management-originated audit records.

**Scenario: Authenticated manager creates an audit-producing management change**

- Given a management request is authenticated with a stable actor user ID and display name
- When the manager performs a management mutation that writes an audit entry
- Then the persisted audit entry SHALL include the stable actor user ID
- And the persisted audit entry SHALL preserve a human-readable actor display value

**Scenario: Authenticated admin performs a management mutation**

- Given a management request is authenticated as an admin
- When the admin performs a management mutation that writes an audit entry
- Then the persisted audit entry SHALL include the authenticated actor user ID

### Requirement: Persist Actor Identity For Import Workflow Actions

The system SHALL persist actor identity for operator-triggered import workflow history that is meant to be reviewed later.

**Scenario: Manager starts an import batch**

- Given an authenticated manager starts an import batch
- When the system persists import workflow metadata for that action
- Then the persisted import-owned record SHALL include the initiating actor user ID

**Scenario: Manager records import decisions or applies a batch**

- Given an authenticated manager records decisions or applies an import batch
- When the system persists the corresponding import workflow history
- Then the persisted record SHALL include the actor user ID for the operator who performed the action

### Requirement: Anonymous Fallback Remains Explicit

The system SHALL keep anonymous fallback explicit only for flows that do not run with an authenticated management actor.

**Scenario: Legacy or unauthenticated path writes an audit record**

- Given a write path executes without an authenticated management actor
- When the system persists an audit record
- Then the record SHALL still contain a non-empty actor display value
- And the stable actor user ID MAY be absent
