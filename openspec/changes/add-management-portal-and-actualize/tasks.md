## 1. Backend: Catalog Management API Completion

- [ ] 1.1 Add drink-catalog commands, handlers, and endpoints for tag update, ingredient delete, ingredient category update, and ingredient category delete.
- [ ] 1.2 Add validation and conflict handling rules for duplicate names and in-use references required by the new management scenarios.
- [ ] 1.3 Extend contracts DTOs and request models to support management operations without breaking existing clients.

## 2. Backend: Actualize Workflow

- [ ] 2.1 Implement import batch domain model and persistence for statuses, provenance metadata, diagnostics, and decision audit.
- [ ] 2.2 Implement strategy abstraction for source ingestion and add `file-csv-json` plus `iba-source` strategy implementations.
- [ ] 2.3 Implement management endpoints for draft creation, validation, preview, explicit apply, and history retrieval.
- [ ] 2.4 Implement idempotency guard using source fingerprint with explicit override path.

## 3. Frontend: Management Portal

- [ ] 3.1 Pre-apply gate: confirm impacted portal design guides exist and are aligned (`web/apps/management-portal/DESIGN.md`).
- [ ] 3.2 Create `management-portal` app in pnpm workspace with TanStack Router and TanStack Query baseline.
- [ ] 3.3 Configure local development API routing with Vite proxy for management portal backend calls.
- [ ] 3.4 Define and implement v1 management UI baseline (shell, navigation, page layout, and consistent component usage).
- [ ] 3.5 Implement manager workflows for drink, ingredient, category, and tag curation against backend management endpoints.
- [ ] 3.6 Implement import workflow UI for strategy selection, validation diagnostics, preview diff, explicit decisions, and apply results.
- [ ] 3.7 Create or update shared workspace packages only for stable cross-cutting API client and UI primitives.

## 4. Infrastructure: AKS Envoy Routing

- [ ] 4.1 Add Envoy Gateway API resources for host-based management routing (`management.alcopilot.com`) to management portal service.
- [ ] 4.2 Restrict management host access operationally until Keycloak integration is implemented.

## 5. Testing

- [ ] 5.1 Add unit tests per new backend handler and strategy mapping path using NSubstitute and Shouldly.
- [ ] 5.2 Add integration tests with TestContainers PostgreSQL for manager CRUD extensions and full import lifecycle scenarios.
- [ ] 5.3 Add frontend tests for management curation and import workflows using existing frontend testing conventions.

## 6. Documentation And Cleanup

- [ ] 6.1 Keep portal design guides updated as implementation evolves and verify OpenSpec artifacts remain aligned.
- [ ] 6.2 Document management portal runtime and import workflow behavior in relevant docs areas.
- [ ] 6.3 Remove obsolete dead frontend template code as part of management portal rollout.
- [ ] 6.4 Prepare follow-up auth integration task references without implementing Keycloak in this change.
- [ ] 6.5 Capture follow-up design-system expansion as a separate backlog/change item if broader theming is required.
