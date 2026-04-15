# ADR 0009: Management Portal Authentication With Keycloak And Host Cookies

## Status

Accepted

## Date

2026-04-13

## Context

The management portal now exists as a separate operator-facing frontend, but it is still anonymous.
The project needs application-level authentication and authorization for management workflows now, while keeping the public product surface available to anonymous users.
The Host is already the external web boundary and the documented owner of session behavior.
The team also wants to use Keycloak as the initial identity administration surface because its current admin UI is sufficient for manager and end-user account administration in the near term.

The chosen direction must support:

- manager-only access to the management portal
- immediate role-based access control using `admin`, `manager`, and `user`
- browser clients that receive only a secure session cookie rather than bearer tokens
- local-development behavior that stays close to production host and cookie boundaries
- future addition of a separate user-facing portal auth flow without forcing that scope into the current implementation

## Decision

Adopt Keycloak as the identity provider for management authentication, with the Host owning the interactive authentication flow and cookie-backed session.

Specifically:

- Use Keycloak as the initial identity administration surface for user, manager, and admin account management.
- Implement management authentication in `AlCopilot.Host` using ASP.NET Core OpenID Connect plus cookie authentication.
- Use a dedicated confidential Keycloak client for the management portal flow in this first slice.
- Enable authorization code flow with PKCE and client authentication for the Host-managed login flow.
- Keep the frontend token-blind: browser clients receive a secure HTTP-only session cookie and do not receive access tokens directly.
- Enforce management authorization at the Host boundary with app-owned policies that map to Keycloak roles.
- Start with standard cookie ticket behavior and minimal claims in the authentication cookie.
- Defer custom server-side ticket storage such as Redis-backed `ITicketStore` unless ticket size, replica coordination, or central session invalidation needs make it necessary.
- Keep local development aligned with production host behavior by accessing the management portal through a stable management host and proxying backend routes behind that host.

## Reason

This ADR is `Accepted` because it fits the current architecture and keeps the first auth rollout focused.
Keycloak provides a strong enough v1 administration experience that the project does not need to build its own identity admin UI yet.
Host-managed OIDC and cookie sessions align with the documented BFF/external-boundary role of `AlCopilot.Host` and avoid putting token management into the SPA.
Deferring Redis-backed ticket storage reduces first-rollout complexity while preserving a clean upgrade path if operational needs later justify server-side ticket storage.

## Consequences

- Management routes and APIs can be protected immediately without changing the modular-monolith backend shape.
- The management portal can show a local "sign in required" state before redirecting to Keycloak.
- Keycloak becomes the initial source of truth for identity credentials, role assignment, and manager/admin provisioning.
- Future user-portal authentication can be added later, potentially with a separate Keycloak client, without invalidating the management-first approach.
- Local development must preserve host and cookie parity closely enough for OIDC callback and session-cookie behavior to remain predictable.
- If cookie payload size, session revocation, or multi-instance session coordination becomes a concrete problem, Redis-backed `ITicketStore` remains an approved follow-up direction rather than a redesign.

## Alternatives Considered

### SPA-Managed Tokens With Keycloak JavaScript Integration

Rejected for now.
This would move token handling into the frontend and work against the current Host-as-boundary direction.

### Introduce Redis-Backed Ticket Storage Immediately

Deferred within this decision.
This is a viable follow-up if claims size or operational requirements justify it, but it adds moving parts to the first authentication rollout.

### Build A Custom AlCopilot Identity Admin UI Now

Rejected for now.
Keycloak's admin console is sufficient for the first phase and avoids spending time on non-differentiating internal tooling.

## Supersedes

None.

## Superseded by

None.
