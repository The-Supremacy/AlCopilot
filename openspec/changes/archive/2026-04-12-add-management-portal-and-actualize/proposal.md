## Why

The Drinks Catalog backend now supports initial catalog behavior, but product teams cannot efficiently seed or curate production-quality data through a dedicated manager workflow.
We need a management portal and a controlled import sync flow now so canonical catalog data can be populated and maintained without speculative AI-specific storage changes.

## What Changes

- Add a new `management-portal` frontend app as a portal-level MFE in the pnpm workspace.
- Keep planned `web-portal` and `management-portal` as independent portal deployments, with host-based routing through Envoy Gateway API.
- Keep local development routing simple with Vite proxy behavior for backend API calls.
- Use persistent portal design guides as frontend design source of truth for this change:
  - `web/apps/management-portal/DESIGN.md`
- Implement manager-facing catalog operations for drinks, ingredients, and tags where CRUD behavior is currently partial.
- Introduce an import sync workflow with manual trigger only, where import start creates and validates immediately, review remains optional, and apply stays explicit.
- Support curated seed import through the `iba-cocktails-snapshot` strategy (preserved snapshot, no runtime fetch).
- Align the initial seed-data workflow with the curated public `rasmusab/iba-cocktails` repository, especially `iba-web/iba-cocktails-web.json`, while keeping AlCopilot's import payload and taxonomy canonical.
- Persist import provenance and decision audit history for traceability.

## Capabilities

### New Capabilities

- `catalog-management-import-sync`: Manager-driven import and import sync lifecycle with explicit conflict resolution.

### Modified Capabilities

- `drink-management`: Extend manager operations for drink and tag curation behavior needed by the management portal.
- `ingredient-management`: Extend manager operations for ingredient curation behavior needed by the management portal.

## Impact

- Affected code spans `web/` portal apps and packages, `server/src/Modules/AlCopilot.DrinkCatalog`, and deployment routing manifests for AKS Envoy Gateway API.
- Impacted portals for UI-affecting work:
  - `management-portal` (primary)
- New backend management endpoints are added under the drink-catalog module API surface.
- The change intentionally defers application-level access control and capability-level independently deployed MFEs inside management.
- Theme and visual design system expansion beyond a management baseline is deferred to follow-up scope.
- External source ingestion starts with curated file import, while PostgreSQL remains the canonical catalog source of truth.
- The `rasmusab/iba-cocktails` repository is treated as seed source material for curation and transformation, not as a live runtime integration.
