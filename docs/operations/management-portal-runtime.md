# Management Portal Runtime

## Purpose

This document captures the current runtime behavior for the management portal and the import sync workflow.
It complements the portal design guide and the OpenSpec change artifacts with end-state operational notes.

---

## Portal Runtime Model

- The management portal is a standalone frontend app in `web/apps/management-portal`.
- Management authentication is owned by `AlCopilot.Host` through OpenID Connect plus a secure HTTP-only cookie session.
- Local development runs through Vite on the management host with `/api` auth and management routes proxied to the Host at `http://localhost:5243` by default.
- Production direction is a dedicated service fronted by Envoy Gateway on `management.alcopilot.com`.
- The backend Host remains the API boundary for all portal requests.
- Local auth parity depends on the browser using the management host shape so login callbacks and cookie behavior stay aligned with production expectations.

---

## Import Workflow Runtime Behavior

- Managers start an import by submitting a strategy key and source metadata; payload is optional for snapshot-based imports.
- Import start creates the batch, stores provenance metadata, normalizes the source payload, runs validation immediately, and persists the current review snapshot.
- Validation diagnostics are persisted on the batch as part of import start and remain visible on both the main Imports page and the Review workspace.
- Review is an explicit refresh command for the stored review snapshot, not a separate lifecycle stage.
- Apply is allowed only when batch apply-readiness is `Ready`.
- Update-containing batches return `RequiresReview` until the review workspace has been explicitly refreshed by a manager.
- Validation-error batches return `BlockedByValidationErrors` instead of using exception-driven control flow for a normal business path.
- The backend may recompute review data during apply if review data is missing, but the normal path is that import start and review refresh keep the stored snapshot current.
- Status is checked through persisted batch reads and history queries rather than SignalR or other push transport. Import batches remain `InProgress` until applied or cancelled, then become `Completed` or `Cancelled`.

### Current Seed Source Alignment

- The current curated seed source is the public repository [`rasmusab/iba-cocktails`](https://github.com/rasmusab/iba-cocktails).
- The preferred upstream file is `iba-web/iba-cocktails-web.json`.
- `iba-web/iba-cocktails-ingredients-web.csv` is a supporting inspection source when ingredient-row detail needs review during normalization.
- The portal and backend do not fetch this repository at runtime; the management portal uses a preserved snapshot for `iba-cocktails-snapshot`.
- The default preserved preset source may be an AlCopilot-owned extended derivative of that snapshot when project curation adds fields such as drink descriptions that do not exist in the raw upstream file.

### Current Snapshot Strategy Shape

The supported preset strategy aligns with the upstream `iba-web/iba-cocktails-web.json` structure from `rasmusab/iba-cocktails`.
The preserved snapshot follows the upstream list-of-cocktails shape rather than an AlCopilot-specific wrapper:

```json
[
  {
    "category": "Contemporary Classics",
    "name": "Daiquiri",
    "method": "Shake and strain.",
    "garnish": "Lime wedge",
    "ingredients": [
      { "direction": "2 oz White Rum", "quantity": "2", "unit": "oz", "ingredient": "White Rum" },
      {
        "direction": "1 oz Fresh Lime Juice",
        "quantity": "1",
        "unit": "oz",
        "ingredient": "Fresh Lime Juice"
      }
    ]
  }
]
```

When no payload is provided for `iba-cocktails-snapshot`, the preserved embedded extended snapshot is used.
The strategy normalizes that upstream-compatible shape into AlCopilot-owned drink, description, ingredient, recipe, method, garnish, and category fields before validation and apply.

---

## Current Access Posture

- Protected management workflows require an authenticated Keycloak session with the `manager` or `admin` role.
- The management portal shows a local "sign in required" state before redirecting unauthenticated users into the Host-managed login flow.
- Authenticated `user` accounts without management roles are denied access to management APIs and portal workflows.
- The first rollout keeps standard cookie ticket behavior and minimal claims; Redis-backed `ITicketStore` remains a follow-up only if operational pressure justifies it.

---

## Related Files

- `web/apps/management-portal/DESIGN.md`
- `deploy/flux/management-portal/`
- `openspec/changes/add-management-portal-authentication/`
