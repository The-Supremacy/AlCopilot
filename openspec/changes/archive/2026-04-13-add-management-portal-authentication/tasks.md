## 1. Backend: Host Authentication And Authorization

- [x] 1.1 Configure `AlCopilot.Host` to use ASP.NET Core OpenID Connect plus cookie authentication for management auth.
- [x] 1.2 Add management authorization policies that map Keycloak roles to app-owned policy names.
- [x] 1.3 Protect management-facing endpoints and route groups at the Host boundary with authentication and authorization requirements.
- [x] 1.4 Keep cookie claims minimal and document the deferred `ITicketStore` follow-up decision in code or configuration notes where appropriate.

## 2. Infrastructure: Keycloak And Local Routing

- [x] 2.1 Add Keycloak integration and environment configuration for the management-first slice in AppHost and runtime configuration.
- [x] 2.2 Configure a dedicated management Keycloak client for Host-managed OIDC login.
- [x] 2.3 Preserve local host parity for auth by routing management portal backend and auth requests through a stable management host and proxy setup.

## 3. Frontend: Management Portal Sign-In Experience

- [x] 3.1 Pre-apply gate: confirm impacted portal design guides are aligned (`web/apps/management-portal/DESIGN.md`).
- [x] 3.2 Add a dedicated "sign in required" management route or entry experience that starts the Host-managed login flow.
- [x] 3.3 Update the management shell to show authenticated session affordances instead of anonymous placeholders.
- [x] 3.4 Gate protected management navigation and bootstrap behavior so unauthenticated users do not enter operator workflows.
- [x] 3.5 Verify implementation remains aligned with Tailwind CSS and shadcn/ui conventions already established for the portal.

## 4. Testing

- [x] 4.1 Add Host-level tests for unauthenticated, forbidden, and authorized management access behavior.
- [x] 4.2 Add backend integration tests covering role-based protection for management endpoints.
- [x] 4.3 Add frontend tests for sign-in-required UX, authenticated shell state, and unauthorized navigation handling.

## 5. Documentation And Follow-Up Capture

- [x] 5.1 Keep ADR 0009 and this change aligned if implementation details force auth-boundary or session-strategy adjustments.
- [x] 5.2 Document management auth runtime assumptions, including stable host routing and Keycloak dependencies, in the relevant operations or architecture docs during implementation.
- [x] 5.3 Capture Redis-backed `ITicketStore` as a follow-up change if first-rollout validation shows cookie-size or session-coordination pressure.
