# ADR 0007: Management Portal Architecture And Envoy Host Routing

## Status

Accepted

## Date

2026-04-10

## Context

The project now needs a manager-facing portal to curate drinks catalog data and run manual import actualization workflows.
We need clear frontend boundary choices that keep complexity under control while preserving future growth.
We also need AKS ingress direction aligned with the confirmed Envoy Gateway API platform and future Keycloak security boundaries.

## Decision

Adopt a portal-level frontend boundary for v1:

- Keep user-facing and manager-facing portals as separate frontend apps in the pnpm workspace.
- Deploy `web-portal` and `management-portal` independently.
- Use workspace packages only for stable cross-cutting concerns such as shared API contracts, shared UI primitives, and reusable service clients.
- Keep management capability slices modular inside the management app for now.

Adopt host-based routing on AKS via Envoy Gateway API:

- Use Envoy `Gateway` and `HTTPRoute` resources for portal host routing.
- Route the management experience through `management.alcopilot.com`.
- Keep management host access operationally restricted until Keycloak is integrated.

## Reason

This ADR is `Accepted` because it balances delivery speed and operational clarity.
Portal-level separation creates clean product and security boundaries without introducing runtime composition overhead from capability-level MFEs.
Host-based routing fits Envoy Gateway API cleanly and reduces future auth/session ambiguity.

## Consequences

- Frontend delivery is split by portal audience with independent deployment paths.
- The management app can still evolve internal module boundaries without immediate runtime federation complexity.
- Ingress configuration and environment isolation for management become explicit platform concerns.
- Future auth rollout has cleaner host-based boundary alignment.

## Alternatives Considered

### Path-Based Routing Under The Main Domain

Rejected for now.
Path-based routing is feasible, but host-based boundaries provide cleaner future auth and operational separation for manager-only access.

### Capability-Level Independently Deployed MFEs Inside Management From Day One

Rejected for now.
This adds orchestration, versioning, and observability complexity before there is demonstrated team or release-pressure need.

### Single Frontend App For User And Management Experiences

Rejected.
This blurs audience boundaries and increases deployment blast radius for manager-only capabilities.

## Supersedes

None.

## Superseded by

None.
