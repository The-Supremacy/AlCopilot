## Context

The management portal already uses React, TanStack Router, TanStack Query, Zustand, Tailwind CSS, and shadcn-managed local primitives.
Existing frontend tests use Vitest with React Testing Library and mock page dependencies rather than hitting real network boundaries.
This follow-up is not introducing new product behavior; it raises confidence around the current manager workflows and keeps the import workspace aligned with the backend's explicit conflict-decision model.

This change is UI-affecting and therefore references the existing persistent design guide:

- `web/apps/management-portal/DESIGN.md`

## Goals / Non-Goals

**Goals:**

- Add high-signal page and page-hook tests for ingredient create/edit/delete, drink create/edit/delete, import review, and import apply/cancel behavior.
- Keep tests colocated with the current portal pages and state hook.
- Ensure the Imports workspace only enables apply when conflicts have explicit stored decisions.

**Non-Goals:**

- New manager-facing features.
- Design-guide changes unless a test reveals real invariant drift.
- Real network, Playwright, or snapshot coverage.

## Decisions

### Decision 1: Keep tests page-level and mock portal data hooks

Tests will stay close to existing page behavior by rendering page components or the page state hook with mocked `@/lib/usePortalData` dependencies.
This keeps assertions focused on mutation payloads, navigation, visible messages, and apply gating without duplicating lower-level component tests.

### Decision 2: Use the existing import decision store as the review/apply bridge

The review page already persists decisions in the Zustand-backed import decision store.
The Imports workspace will treat those stored entries as the explicit decision source for apply and will only enable apply when every current conflict has a stored decision.
This matches the backend workflow, where apply requires a decision for each conflict entry.

### Decision 3: Keep delivery-confidence expectations in a dedicated follow-up capability

Because this change affects delivery confidence rather than product behavior, it will use a dedicated OpenSpec capability for management-portal frontend testing.
The design and tasks will describe the expected test coverage and the small UI-state alignment required to support it.

## Risks / Trade-offs

- [Over-mocking can hide integration gaps] -> Keep tests page-shaped and assert concrete mutation payloads plus navigation and error states.
- [Import decision store can leak state across tests] -> Use unique batch IDs per test and clear decision state after apply or cancel flows.
- [Test-only follow-up can drift from current behavior] -> Keep the imports workspace code adjustment in the same change as the tests that prove it.

## Migration Plan

1. Add the follow-up OpenSpec artifacts for frontend confidence.
2. Adjust import workspace gating so explicit stored decisions unlock apply.
3. Add colocated page and page-hook tests for the targeted manager workflows.
4. Run the management-portal Vitest suite and confirm the active ingredient-management spec drift is resolved separately.

Rollback strategy:

- Frontend rollback is limited to reverting the import workspace state adjustment and the colocated tests.
- No backend or database rollback is required because the change does not alter persisted models or contracts.
