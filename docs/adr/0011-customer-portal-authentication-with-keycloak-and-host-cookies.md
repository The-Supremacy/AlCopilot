# ADR 0011: Customer Portal Authentication With Keycloak And Host Cookies

## Status

Accepted

## Date

2026-04-14

## Context

The project is ready to start the customer-facing portal after establishing the management portal and its Host-owned authentication flow.
The customer portal has a different audience, a different risk profile, and a different onboarding expectation than the management portal.
The team wants signed-in customer access in the first iteration so that bar inventory, taste preferences, and recommendation history can persist across sessions.
The Host is already the external web boundary and the accepted owner of session behavior.
The current Keycloak bootstrap only supports the management client and currently disables self-registration.

The chosen direction must support:

- customer self-signup and sign-in through Keycloak
- identity account administration through the Keycloak console rather than a custom AlCopilot account-management surface
- a browser experience that remains token-blind
- a session and cookie boundary separate from management
- local host parity for cookie and callback behavior
- future coexistence of management and customer auth without confusing shared state

## Decision

Adopt a dedicated customer authentication surface in `AlCopilot.Host` using Keycloak plus Host-managed cookie sessions.

Specifically:

- Use a dedicated Keycloak client for the customer portal rather than reusing the management client.
- Keep the customer portal on the main product host while management remains on its dedicated management host.
- Implement customer authentication in `AlCopilot.Host` using ASP.NET Core OpenID Connect plus cookie authentication.
- Use a dedicated customer cookie name, callback path, signed-out callback path, login endpoint, logout endpoint, and session endpoint.
- Preserve the browser token-blind model so the SPA receives only a secure HTTP-only session cookie and does not manage bearer tokens directly.
- Keep identity account lifecycle and administration in the Keycloak console rather than building customer account-management flows inside AlCopilot in this slice.
- Treat the realm `user` role as the baseline role that grants customer portal access.
- Enable customer self-registration in the local Keycloak realm bootstrap for this portal rollout.
- Keep customer and management authorization separate through app-owned policies and scheme-specific configuration rather than mixed route checks.

## Reason

This ADR is `Accepted` because it aligns with the Host-as-boundary architecture while keeping customer and management concerns clearly separated.
The customer portal needs self-service onboarding and durable session behavior now, and those needs should not be squeezed into the management authentication surface.
Using a separate Keycloak client and cookie keeps redirect behavior, logout semantics, and policy evolution easier to reason about as the product grows.
The decision is active architecture guidance for the first customer portal slice rather than a speculative future option.

## Consequences

- The Host will own two distinct interactive authentication surfaces: management and customer.
- The local Keycloak bootstrap must expand to include customer self-signup and a customer client configuration.
- The customer portal can persist customer-owned state without inventing temporary anonymous storage.
- Cookie naming, callback paths, and session endpoints remain audience-specific and easier to audit.
- Account credential, identity, and role administration remain centralized in Keycloak rather than duplicated in product code.
- Auth and deployment configuration grow more explicit, but that complexity is preferable to mixing portal concerns into one session boundary.

## Alternatives Considered

### Reuse the management client and cookie with separate policies only

Rejected.
This would technically work, but it would blur audience boundaries and make redirects, logout behavior, and future portal evolution harder to reason about.

### Put customer auth directly in the SPA with browser-managed tokens

Rejected.
This would contradict the accepted Host-owned session direction and would teach the frontend token-management concerns that do not belong there.

### Keep customer access anonymous for the first portal slice

Rejected.
That would undermine the first recommendation loop by removing durable profile, bar, and history state from the initial experience.

### Build customer account-management flows inside AlCopilot now

Rejected.
Keycloak already provides the required account-administration surface for this phase, and the product should focus on domain-owned preference and recommendation behavior instead.

## Supersedes

None.

## Superseded by

None.
