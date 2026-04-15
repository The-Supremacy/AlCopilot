## Why

The management portal is now the operator-facing surface for catalog and import workflows, but it still runs with anonymous access.
That leaves manager-only functionality exposed without application-level authentication or authorization, and it delays a security boundary the architecture has already anticipated at the Host layer.
We need to protect the management portal now with a practical identity and RBAC approach that fits the current modular monolith, keeps the SPA token-blind, and avoids pulling future customer-portal scope into this first change.

## What Changes

- Add management-portal authentication through Keycloak, with `AlCopilot.Host` owning OpenID Connect login, callback, logout, and cookie-backed session behavior.
- Protect management portal routes and management-facing backend endpoints with role-based authorization from the start.
- Use Keycloak as the initial admin surface for identity and role assignment rather than building a custom AlCopilot admin UI.
- Add a manager-facing "sign in required" screen before redirecting unauthenticated users to Keycloak.
- Keep local development auth behavior aligned with production host boundaries by using a stable management host and proxying backend routes through that host.
- Keep the first rollout focused on the management portal only; public and customer portal authentication remains future scope.
- Use persistent portal design guides as frontend design source of truth for this change:
  - `web/apps/management-portal/DESIGN.md`

## Capabilities

### New Capabilities

- `management-authentication`: authenticated access, sign-in gating, and RBAC enforcement for the management portal.

## Impact

- Affected code spans `server/src/AlCopilot.Host`, `server/src/AlCopilot.AppHost`, the management portal under `web/apps/management-portal`, and local or deployment routing configuration that affects auth host parity.
- Impacted portals for UI-affecting work:
  - `management-portal`
- The change introduces Keycloak as a new infrastructure dependency for authentication.
- The change keeps the browser free of bearer-token handling and centers session behavior in the Host.
- Redis-backed custom ticket storage is intentionally deferred unless the first rollout demonstrates a concrete need for server-side ticket persistence.
- Public anonymous product usage and future user self-registration remain intentionally out of scope for this management-first change.
