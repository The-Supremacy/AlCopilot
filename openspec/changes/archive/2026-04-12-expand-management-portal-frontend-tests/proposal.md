## Why

The management portal now covers drink, ingredient, tag, import, and audit workflows, but the frontend test suite only exercises a small portion of those page-level paths.
We need stronger page-level coverage now so manager workflows stay safe to refactor and regressions in import review or catalog editing are caught before archive and release.

## What Changes

- Add a follow-up OpenSpec change focused on management-portal frontend confidence rather than user-visible behavior changes.
- Expand page-level Vitest and React Testing Library coverage for ingredient, drink, and import workflows.
- Align the import workspace tests with the explicit-decision workflow already enforced by the backend apply path.
- Keep the management portal design guide unchanged unless testing reveals actual UI invariant drift.

## Capabilities

### New Capabilities

- `management-portal-frontend-testing`: Delivery-confidence expectations for manager workflow page coverage in the management portal.

## Impact

- Affected code is limited to `web/apps/management-portal` tests and a small import workspace state adjustment needed to match explicit-review decision behavior.
- Impacted portals for UI-affecting work:
  - `management-portal` (primary)
- This follow-up does not change backend contracts, deployment routing, or supported manager-facing features.
