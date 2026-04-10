## Why

The Drinks Catalog backend now supports initial catalog behavior, but product teams cannot efficiently seed or curate production-quality data through a dedicated manager workflow.
We need a management portal and a controlled import actualization flow now so canonical catalog data can be populated and maintained without speculative AI-specific storage changes.

## What Changes

- Add a new `management-portal` frontend app as a portal-level MFE in the pnpm workspace.
- Keep planned `web-portal` and `management-portal` as independent portal deployments, with host-based routing through Envoy Gateway API.
- Keep local development routing simple with Vite proxy behavior for backend API calls.
- Use persistent portal design guides as frontend design source of truth for this change:
  - `web/apps/management-portal/DESIGN.md`
- Implement manager-facing catalog operations for drinks, ingredients, categories, and tags where CRUD behavior is currently partial.
- Introduce an import actualization workflow with manual trigger only in v1, including draft, validate, preview, and explicit apply phases.
- Add pluggable actualization strategies with `file-csv-json` and `iba-source` as the initial implementations.
- Persist import provenance and decision audit history for traceability.

## Capabilities

### New Capabilities

- `catalog-actualize-management`: Manager-driven import and actualization lifecycle with strategy adapters and explicit conflict resolution.

### Modified Capabilities

- `drink-management`: Extend manager operations for drink and tag curation behavior needed by the management portal.
- `ingredient-management`: Extend manager operations for ingredient and ingredient category curation behavior needed by the management portal.

## Impact

- Affected code spans `web/` portal apps and packages, `server/src/Modules/AlCopilot.DrinkCatalog`, and deployment routing manifests for AKS Envoy Gateway API.
- Impacted portals for UI-affecting work:
  - `management-portal` (primary)
- New backend management endpoints are added under the drink-catalog module API surface.
- The change intentionally defers Keycloak integration and capability-level independently deployed MFEs inside management.
- Theme and visual design system expansion beyond a v1 management baseline is deferred to follow-up scope.
- External source ingestion starts with IBA and file import, while PostgreSQL remains the canonical catalog source of truth.
