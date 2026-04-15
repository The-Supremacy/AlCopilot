# Spec: Management Authentication

## Feature: Management Portal Access Control

### Requirement: Protected Management Access

The system SHALL require an authenticated operator session for access to protected management portal behavior.

**Scenario: Unauthenticated manager reaches a protected management page**

- Given a browser requests a protected management portal route without an authenticated session
- When the portal loads the route
- Then the system SHALL present a local "sign in required" experience
- And the experience SHALL provide a sign-in action that starts the Host-managed login flow

**Scenario: Authenticated manager accesses the management portal**

- Given a browser has an authenticated session with the `manager` role
- When the browser requests a protected management portal route
- Then the system SHALL allow access to management portal workflows

**Scenario: Authenticated admin accesses the management portal**

- Given a browser has an authenticated session with the `admin` role
- When the browser requests a protected management portal route
- Then the system SHALL allow access to management portal workflows

**Scenario: Authenticated end user is denied management access**

- Given a browser has an authenticated session with the `user` role and no management role
- When the browser requests a protected management portal route or endpoint
- Then the system SHALL deny access

### Requirement: Host-Owned Authentication Session

The system SHALL keep management authentication at the Host boundary and SHALL not require the frontend to manage bearer tokens directly.

**Scenario: Browser signs in to management portal**

- Given a browser is on the management portal sign-in-required experience
- When the browser chooses to sign in
- Then the Host SHALL start an OpenID Connect login flow against Keycloak
- And successful login SHALL result in a secure authenticated session cookie

**Scenario: Browser returns from successful login**

- Given a browser successfully completes Keycloak login for management access
- When the Host finalizes the login flow
- Then the system SHALL redirect the browser to `/`
- And the management portal SHALL render an authenticated session state

**Scenario: Browser signs out from management portal**

- Given a browser has an authenticated management session
- When the browser signs out
- Then the Host SHALL clear the local authenticated session
- And the management portal SHALL return to a non-authenticated state

### Requirement: Role-Based Management Authorization

The system SHALL enforce role-based management authorization through Host-defined application policies.

**Scenario: Manager role satisfies management access policy**

- Given an authenticated session contains the `manager` role
- When the Host evaluates the management-access policy
- Then the policy SHALL succeed

**Scenario: Admin role satisfies management access policy**

- Given an authenticated session contains the `admin` role
- When the Host evaluates the management-access policy
- Then the policy SHALL succeed

**Scenario: User role fails management access policy**

- Given an authenticated session contains the `user` role and no management role
- When the Host evaluates the management-access policy
- Then the policy SHALL fail
