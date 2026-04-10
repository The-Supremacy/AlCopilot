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

| Concern           | Choice                   | Notes                                            |
| ----------------- | ------------------------ | ------------------------------------------------ |
| Build tool        | Vite                     | Fast dev server and aligned test tooling         |
| UI runtime        | React + TypeScript       | Strict mode frontend codebase                    |
| Routing           | TanStack Router          | Type-safe route definitions                      |
| Server state      | TanStack Query           | Shared pattern for backend data fetching         |
| Client-only state | Zustand                  | Small focused client state when needed           |
| Styling           | Tailwind CSS + shadcn/ui | Preferred UI direction once components are added |

The accepted frontend stack decision is recorded in [ADR 0003](../adr/0003-frontend-stack.md).

---

## Backend Interaction

Each portal talks to the Host rather than directly to individual modules.
The Host owns authentication, session management, and future reverse-proxy behavior.
Frontend code should depend on stable application APIs rather than backend implementation details.

---

## Deployment Routing

AKS ingress routing uses Envoy Gateway API resources.
The accepted direction is host-based routing with a dedicated management host (for example `management.alcopilot.com`) to keep operator-facing traffic isolated from user-facing routes.
This direction is captured in [ADR 0007](../adr/0007-management-portal-architecture-and-envoy-host-routing.md).

---

## Workspace Structure

- `web/apps/` — portal apps (user portal and management portal boundaries)
- `web/apps/web-portal/` — planned user-facing application path
- `web/packages/` — shared cross-cutting packages when needed (API contracts, shared UI primitives, reusable service clients)

---

## Portal Design Guides

- `web/apps/management-portal/DESIGN.md` — persistent UI and information-architecture guidance for the management portal

For UI-affecting changes, update affected existing portal design guides before implementation starts and keep OpenSpec artifacts aligned with those guides.
User portal design guidance for `web-portal` can be added when that app implementation starts.
Use `/design:new` to create new portal design guides, `/design:change` to evolve existing portal design invariants, and `/design:lint` to verify low-drift compliance.

---

## Deferred Direction

Capability-level independently deployed MFEs inside the management portal are deferred.
Current guidance is to keep those capability slices modular within the management app until concrete release-pressure triggers appear.
This deferred direction is captured in [ADR 0008](../adr/0008-capability-level-microfrontends-inside-management-portal.md).

---

## Related Guidance

- [../testing/web.md](../testing/web.md) — frontend testing strategy
- [../constitution/web.md](../constitution/web.md) — frontend workflow and quality expectations
- [../adr/0003-frontend-stack.md](../adr/0003-frontend-stack.md) — accepted frontend stack
- [../adr/0007-management-portal-architecture-and-envoy-host-routing.md](../adr/0007-management-portal-architecture-and-envoy-host-routing.md) — accepted portal boundary and routing
