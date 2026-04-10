## Context

The project uses a modular monolith backend with PostgreSQL as canonical storage, plus a pnpm-based frontend workspace with React, TanStack Router, and TanStack Query.
ADR-0006 defers AI-specific catalog storage and keeps relational catalog data canonical, so import and curation workflows should operate on the existing DrinkCatalog bounded context.
The current frontend state does not provide a dedicated management experience, and the current backend endpoint surface is only partially aligned with full manager CRUD operations.
This change is UI-affecting and therefore uses persistent portal design guides as required context:

- `web/apps/management-portal/DESIGN.md`

## Goals / Non-Goals

**Goals:**

- Deliver a dedicated management portal as an independently deployable portal-level frontend app.
- Complete manager catalog operation coverage required for practical data curation.
- Add a manual-trigger actualization lifecycle with explicit preview and apply decisions.
- Support first import strategies from file (`csv/json`) and IBA source.
- Persist audit and provenance data for import runs.

**Non-Goals:**

- Keycloak authentication and authorization integration.
- Automated scheduling of actualization runs.
- Capability-level independently deployed MFEs inside management portal.
- Vector storage, embeddings, or MCP-based ingestion workflows.

## Decisions

### Decision 1: Portal-level MFE boundary for v1

The frontend will use portal-level microfrontends (`web-portal`, `management-portal`) as independent apps in the pnpm workspace.
Management capability slices remain modular inside the management app for now, with workspace packages used for stable cross-cutting concerns only.
This keeps complexity lower than runtime capability federation while preserving future extraction paths.

### Decision 2: Host-based routing on AKS via Envoy Gateway API

AKS ingress routing will use host-based routes, including `management.alcopilot.com` for the management portal.
Envoy Gateway API resources (`Gateway`, `HTTPRoute`) provide path forwarding and service binding without introducing Nginx-specific assumptions.
This creates clean security and operational boundaries for future Keycloak and RBAC rollout.
For local development, portals continue using Vite dev-server proxy for backend API routes so local workflows do not depend on AKS ingress resources.

### Decision 3: Import actualization lifecycle and strategy abstraction

A strategy interface will normalize source payloads into catalog domain inputs.
V1 strategies are `file-csv-json` and `iba-source`.
Every import run follows `Draft -> Validated -> Previewed -> Applied`, and conflicts require explicit manager decisions before apply.
No automatic destructive merge behavior is allowed by default.

### Decision 4: DrinkCatalog domain alignment

Import apply logic orchestrates existing DrinkCatalog aggregates and repositories through Mediator handlers.
Aggregate roots involved are Drink, Ingredient, IngredientCategory, and Tag.
Existing value objects remain authoritative for validation, including names, quantities, and image URL constraints.
Existing domain event infrastructure remains in place, and import apply behavior should continue persisting domain event records as part of normal unit-of-work save flows.

### Decision 5: UI baseline versus full design system scope

This change includes a practical v1 management UI baseline (layout shell, navigation, and consistent component usage aligned with existing stack conventions).
Large-scale branding, full design token system expansion, and broader visual-language redesign are out of scope for this change and should be handled as follow-up design-focused work if needed.
All UI flow and layout work in this change should stay aligned with the management portal design guide before implementation and before archive.

## Risks / Trade-offs

- [Import mapping ambiguity across sources] -> Define strict normalization and validation diagnostics before apply.
- [Large batch apply complexity] -> Apply in bounded transactional chunks with resumable batch state.
- [Deferred auth introduces temporary access risk] -> Restrict management host at ingress/network level until Keycloak is integrated.
- [Portal modularity can drift into over-abstraction] -> Limit shared packages to stable cross-cutting contracts and keep feature composition local to management app.

## Migration Plan

1. Add management portal app and shared workspace packages without changing public user portal routes.
2. Add backend management and actualization endpoints behind the existing drink-catalog module boundary.
3. Deploy Envoy host-based route for management host in non-public mode.
4. Roll out manual import actualization workflow and manager catalog curation UI.
5. Validate production-like import runs with IBA/file sources before broader operator usage.

Rollback strategy:

- Frontend rollback uses previous deployment image and route bindings for management host.
- Backend rollback disables new management routes and reverts application deployment.
- Database rollback uses additive migration discipline where possible; if a migration must be reverted, use standard EF migration rollback scripts and preserve import audit tables unless data corruption requires manual corrective action.

## Open Questions

- Final Keycloak role model for manager and future admin personas remains deferred.
- Schedule-based actualization and recurring sync policies remain deferred.
- Capability-level independently deployed MFEs inside management remain deferred pending demonstrated release pressure.
