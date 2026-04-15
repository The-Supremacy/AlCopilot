## Context

The project uses a modular monolith backend with `AlCopilot.Host` as the external web boundary and a separate management portal frontend app in the `web/` workspace.
ADR 0007 already establishes portal-level separation and a dedicated management host direction.
ADR 0009 establishes Keycloak plus Host-managed cookie authentication as the accepted management-auth direction.
This change is UI-affecting and therefore uses the management portal design guide as required context:

- `web/apps/management-portal/DESIGN.md`

## Goals / Non-Goals

**Goals:**

- Require authentication for management portal access.
- Enforce role-based access control immediately for management routes and APIs.
- Keep the frontend token-blind by using Host-managed cookie authentication.
- Show a local "sign in required" management UX before redirecting to Keycloak.
- Keep local development host and cookie behavior close to production.

**Non-Goals:**

- Customer portal authentication or registration flows.
- Social login.
- A custom AlCopilot identity administration UI.
- Redis-backed custom session ticket storage in the first implementation slice unless the rollout explicitly expands scope.

## Decisions

### Decision 1: Management authentication lives at the Host boundary

`AlCopilot.Host` will own the OpenID Connect challenge, callback, logout, and cookie session lifecycle.
The management portal will not manage bearer tokens directly and will not call Keycloak from the browser.
Management APIs exposed through the Host will require authenticated sessions and matching authorization policies.

This keeps the current BFF/external-boundary direction intact and avoids teaching the management portal token lifecycle concerns that belong to the backend boundary.

### Decision 2: Use one dedicated Keycloak client for the management-first slice

The first implementation slice will add a dedicated confidential Keycloak client for management authentication only.
The customer portal remains out of scope and may later use a separate Keycloak client if its redirect, registration, or logout needs diverge.

The Host can support multiple authentication schemes later if needed, but this change keeps configuration intentionally narrow so the first rollout stays easy to reason about.

### Decision 3: Use authorization code flow with PKCE, client authentication, and cookie session

The Host will use ASP.NET Core OpenID Connect plus cookie authentication with authorization code flow and PKCE.
The management authentication cookie should be:

- HTTP-only
- secure in environments where HTTPS is available
- same-site `Lax`

Claims mapped into the cookie should remain minimal and focused on user identifier, display name, and role data needed for policy evaluation.
This avoids oversized cookies and preserves a clean path to server-side ticket storage if future operational needs justify it.

### Decision 4: Start with standard cookie ticket behavior and defer Redis-backed `ITicketStore`

The first rollout will use the standard cookie ticket behavior rather than a custom session store.
The implementation should keep the claim set intentionally small so this remains operationally safe.

Redis-backed `ITicketStore` remains an approved follow-up if any of the following become true:

- cookie size becomes problematic
- multi-instance management session coordination requires centralized server-side storage
- remote session invalidation becomes a concrete requirement

Deferring this choice keeps the initial management-auth rollout smaller without closing off the more production-shaped session model the team has already used successfully elsewhere.

### Decision 5: Sign-in-required is a dedicated management UI state

Unauthenticated access to protected management routes should land on a local "sign in required" view rather than immediately redirecting to Keycloak.
That screen should explain that management access requires an authenticated operator account and provide a clear sign-in action that starts the Host-managed challenge flow.

This behavior belongs to the management portal UX, while actual authorization remains enforced at the Host endpoint boundary as well.

### Decision 6: Role model and policy mapping

The management-first role model is:

- `admin`
- `manager`
- `user`

For this change:

- `manager` grants management portal access
- `admin` grants management portal access and all higher-privilege administrative access
- `user` does not grant management portal access

Application code should use app-owned policy names instead of scattering direct role checks throughout handlers and endpoints.
Expected policy baselines include:

- `CanAccessManagementPortal`
- `CanAdministerManagement`

Additional finer-grained policies can be introduced later if management workflows need them.

### Decision 7: Local auth parity uses a stable management host with proxy routing

Local development should access the management portal through a stable management host so cookies and OIDC callback paths behave like production.
Frontend dev proxying remains acceptable for local workflows as long as the browser experiences a coherent management host boundary for portal pages, auth routes, and backend API calls.

This keeps the current Vite-based dev ergonomics while avoiding a split-origin setup that would undermine cookie-backed auth confidence.

## Risks / Trade-offs

- [Management-only scope can tempt hard-coded assumptions] -> Keep the customer portal explicitly out of scope in this change and isolate management auth configuration behind named options and policies.
- [Cookie size can grow if claims are copied too broadly] -> Minimize claims and defer server-side ticket storage until justified.
- [Local auth flows can become brittle if hosts diverge] -> Keep stable management host naming and proxy behavior as a documented requirement.
- [Role checks can sprawl across routes and handlers] -> Centralize policy definitions at the Host boundary.

## Migration Plan

1. Add Keycloak resource wiring and management-auth configuration for local and environment-based setup.
2. Add Host authentication and authorization plumbing, including management-specific policies and login/logout endpoints or routes.
3. Add management portal sign-in-required UX and authenticated session affordances in the shell.
4. Protect management data access paths at both UI entry points and backend endpoints.
5. Verify the full login, logout, unauthorized, and forbidden flows in local parity configuration before broader rollout.

Rollback strategy:

- Disable management auth configuration and revert to the previous management deployment if rollout blocks operator access unexpectedly.
- Remove new Host auth middleware and management auth route protection if rollback is required.
- Leave Keycloak realm or client setup in place if harmless, but disconnect the app integration until the rollout is retried.

## Open Questions

- Whether customer-portal auth should reuse Host primitives with a separate Keycloak client remains future scope.
- Whether Redis-backed `ITicketStore` should become required depends on the first rollout's cookie size and operational needs.
- Whether the production management host should terminate HTTPS before or at the same boundary that issues auth cookies should be finalized with deployment implementation details.
