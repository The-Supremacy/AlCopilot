## 1. Backend: Catalog Management API Completion

- [x] 1.1 Add drink-catalog commands, handlers, and endpoints for tag update, ingredient delete, and fully editable ingredient management.
- [x] 1.2 Add validation and conflict handling rules for duplicate names and in-use references required by the new management scenarios.
- [x] 1.3 Extend contracts DTOs and request models to support management operations without breaking existing clients.

## 2. Backend: Import Sync Workflow

- [x] 2.1 Implement import batch domain model and persistence for statuses, provenance metadata, diagnostics, and decision audit.
- [x] 2.2 Implement strategy abstraction for source ingestion and add the `iba-cocktails-snapshot` strategy implementation.
- [x] 2.3 Implement management endpoints for import start, review, explicit apply, and history retrieval.
- [x] 2.4 Implement idempotency guard using source fingerprint with explicit override path.

## 3. Frontend: Management Portal

- [x] 3.1 Pre-apply gate: confirm impacted portal design guides exist and are aligned (`web/apps/management-portal/DESIGN.md`).
- [x] 3.2 Create `management-portal` app in pnpm workspace with TanStack Router and TanStack Query baseline.
- [x] 3.3 Configure local development API routing with Vite proxy for management portal backend calls.
- [x] 3.4 Define and implement management UI baseline (shell, navigation, page layout, and consistent component usage).
- [x] 3.5 Implement manager workflows for drink, ingredient, and tag curation against backend management endpoints.
- [x] 3.6 Implement import workflow UI for strategy selection, immediate validation diagnostics, review rows, explicit decisions, and apply results.
- [x] 3.7 Create or update shared workspace packages only for stable cross-cutting API client and UI primitives.

## 4. Infrastructure: AKS Envoy Routing

- [x] 4.1 Add Envoy Gateway API resources for host-based management routing (`management.alcopilot.com`) to management portal service.
- [x] 4.2 Restrict management host access operationally until product-level access control is implemented.

## 5. Testing

- [x] 5.1 Add unit tests per new backend handler and strategy mapping path using NSubstitute and Shouldly.
- [x] 5.2 Add integration tests with TestContainers PostgreSQL for manager CRUD extensions and full import lifecycle scenarios.
- [x] 5.3 Add frontend tests for management curation and import workflows using existing frontend testing conventions.

## 6. Documentation And Cleanup

- [x] 6.1 Keep portal design guides updated as implementation evolves and verify OpenSpec artifacts remain aligned.
- [x] 6.2 Document management portal runtime and import workflow behavior in relevant docs areas.
- [x] 6.3 Remove obsolete dead frontend template code as part of management portal rollout.
- [x] 6.4 Prepare follow-up access-control task references without implementing product-level access control in this change.
- [x] 6.5 Capture follow-up design-system expansion as a separate backlog/change item if broader theming is required.
