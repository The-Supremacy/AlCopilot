# Web Architecture

## Purpose

This document is the detailed frontend architecture guide for AlCopilot.
Use it for human-readable frontend architecture context and current client-side decisions.

---

## Platform Shape

The frontend is built in the `web/` workspace with portal-level app boundaries.
The current architecture direction is independently deployable user and management portals.
Each portal talks to the Host over application APIs and remains unaware of future backend module extraction.

---

## Core Stack

| Concern           | Choice                   | Notes                                    |
| ----------------- | ------------------------ | ---------------------------------------- |
| Build tool        | Vite                     | Fast dev server and aligned test tooling |
| UI runtime        | React + TypeScript       | Strict mode frontend codebase            |
| Routing           | TanStack Router          | Type-safe route definitions              |
| Server state      | TanStack Query           | Shared pattern for backend data fetching |
| Client-only state | Zustand                  | Small focused client state when needed   |
| Styling           | Tailwind CSS + shadcn/ui | Required default for new portal UI work  |

The accepted frontend stack decision is recorded in [ADR 0003](../adr/0003-frontend-stack.md).
New portal UI work should be assumed to use this stack unless a change explicitly documents an approved exception.
Custom CSS-only component systems are not the default implementation path for new pages.
In this repo, shadcn/ui means app-owned local components under `src/components/ui/`, initialized via `components.json`, with Radix primitives used underneath when richer accessible behavior is needed.

---

## Backend Interaction

Each portal talks to the Host rather than directly to individual modules.
The Host owns the external web boundary, session behavior, and future reverse-proxy behavior.
Frontend code should depend on stable application APIs rather than backend implementation details.
The management portal authentication direction is Host-managed Keycloak login with secure cookie sessions rather than browser-managed bearer tokens, as accepted in [ADR 0009](../adr/0009-management-portal-authentication-with-keycloak-and-host-cookies.md).
The customer portal direction is Host-managed Keycloak login with a customer-specific session surface separate from management, as accepted in [ADR 0011](../adr/0011-customer-portal-authentication-with-keycloak-and-host-cookies.md).

---

## Deployment Routing

AKS ingress routing uses Envoy Gateway API resources.
The accepted direction is host-based routing with a dedicated management host (for example `management.alcopilot.com`) to keep operator-facing traffic isolated from user-facing routes.
This direction is captured in [ADR 0007](../adr/0007-management-portal-architecture-and-envoy-host-routing.md).

---

## Workspace Structure

- `web/apps/` — portal apps (user portal and management portal boundaries)
- `web/apps/web-portal/` — planned user-facing application path
- `web/packages/` — shared cross-cutting packages when needed (API contracts, shared UI primitives, reusable service clients, shared styling foundations)
- Route-level React files should live in `src/pages/`
- Large route files should decompose into feature sections and colocated page hooks rather than accumulating all logic in one page

---

## Portal Design Guides

- `web/apps/management-portal/DESIGN.md` — persistent UI and information-architecture guidance for the management portal
- `web/apps/web-portal/DESIGN.md` — persistent UI and information-architecture guidance for the customer portal
- `operations/management-portal-runtime.md` — current runtime and import workflow operating notes for the management portal

Cross-portal brand foundations should be shared from a pnpm workspace package and expressed through Tailwind theme tokens plus semantic roles rather than app-local duplicated palette logic.

For UI-affecting changes, update affected existing portal design guides before implementation starts and keep OpenSpec artifacts aligned with those guides.
Use `/design:new` to create new portal design guides, `/design:change` to evolve existing portal design invariants, and `/design:lint` to verify low-drift compliance.
OpenSpec proposal or design artifacts for new UI work should explicitly mention stack alignment with Tailwind CSS and shadcn/ui unless an approved exception exists.
Reviewers can verify alignment by checking for `components.json`, local primitives in `src/components/ui/`, and Radix packages only where interactive behavior needs them. There is no expectation of a runtime `shadcn/ui` npm package.

---

## Deferred Direction

Capability-level independently deployed MFEs inside the management portal are deferred.
Current guidance is to keep those capability slices modular within the management app until concrete release-pressure triggers appear.
This deferred direction is captured in [ADR 0008](../adr/0008-capability-level-microfrontends-inside-management-portal.md).

---

## Current Runtime Notes

The current management portal implementation uses polling-friendly persisted import batch status rather than SignalR or other live transport.
Local development starts from Aspire through `AlCopilot.Orchestration`, which composes the portal Vite apps with the Host, migrator, Keycloak, PostgreSQL, and Qdrant.
The Vite apps still own their local dev-server behavior and `/api` proxy shape, but Aspire supplies the composed Host endpoint for integrated local runs.
Production direction uses a dedicated management host and service wiring described in [operations/management-portal-runtime.md](../operations/management-portal-runtime.md).
The management portal now follows the accepted Tailwind CSS, shadcn/ui, and Zustand stack direction.
Management auth should preserve host parity across environments so cookie and OIDC callback behavior stay predictable.

---

## Related Guidance

- [../testing/web.md](../testing/web.md) — frontend testing strategy
- [../constitution/web.md](../constitution/web.md) — frontend workflow and quality expectations
- [../adr/0003-frontend-stack.md](../adr/0003-frontend-stack.md) — accepted frontend stack
- [../adr/0007-management-portal-architecture-and-envoy-host-routing.md](../adr/0007-management-portal-architecture-and-envoy-host-routing.md) — accepted portal boundary and routing
