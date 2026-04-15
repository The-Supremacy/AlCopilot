## ADDED Requirements

### Requirement: Customer Portal Session And Sign-In Flow

The system SHALL provide a dedicated customer authentication flow at the Host boundary for the customer web portal.

#### Scenario: Anonymous customer session check returns signed-out state

- **WHEN** an unauthenticated browser requests the customer session endpoint
- **THEN** the system SHALL return an anonymous customer session payload

#### Scenario: Customer sign-in starts through the Host

- **WHEN** an unauthenticated customer starts sign-in from the customer portal
- **THEN** the system SHALL start a Host-managed OpenID Connect login flow against the customer Keycloak client

#### Scenario: Customer sign-out clears only the customer session

- **WHEN** an authenticated customer signs out from the customer portal
- **THEN** the system SHALL clear the customer authentication session without requiring management-session state to be shared

### Requirement: Customer Portal Access Policy

The system SHALL enforce customer portal access through a customer-specific Host authorization policy.

#### Scenario: Authenticated customer with user access can enter the portal

- **WHEN** the Host evaluates the customer portal access policy for an authenticated customer account with the `user` role
- **THEN** the system SHALL allow access to customer portal routes and APIs protected by the customer policy

#### Scenario: Unauthenticated customer is denied protected customer APIs

- **WHEN** an unauthenticated browser calls a protected customer API
- **THEN** the system SHALL return an unauthorized response

### Requirement: Customer Self-Service Onboarding

The system SHALL support customer self-registration through the configured customer identity flow.

#### Scenario: New customer account can register for portal access

- **WHEN** a new user completes the configured self-registration flow for the customer portal
- **THEN** the system SHALL allow that account to establish a customer session and access the signed-in customer portal experience
