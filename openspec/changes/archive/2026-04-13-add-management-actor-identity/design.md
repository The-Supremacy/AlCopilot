## Context

The project now has Host-managed Keycloak authentication for the management portal, which means authenticated management requests have stable identity claims available at the web boundary.
The DrinkCatalog module already persists management audit logs and import workflow metadata, but those records currently capture only a free-form actor string or no actor identity at all.
This change focuses on making authenticated operator identity durable in the records that matter for management traceability.

## Goals / Non-Goals

**Goals:**

- Persist a stable actor user ID for management audit entries.
- Preserve a readable actor display value for current operational UI and diagnostics.
- Capture actor identity for user-triggered import workflow records that are meant to be reviewed later.
- Keep identity capture scoped to management flows first.

**Non-Goals:**

- Rework every diagnostic record to carry actor identity directly.
- Add customer-portal user auditing in this change.
- Introduce a general-purpose cross-module identity abstraction beyond what current management flows need.

## Decisions

### Decision 1: Separate stable actor ID from display actor text

Audit records should store both:

- a stable actor user ID from authenticated claims
- a readable actor display string for current portal and operator review use

This avoids overloading one string field with both machine identity and user-facing display concerns.

### Decision 2: Capture actor identity at the Host boundary and pass it explicitly

The Host should derive management actor identity from the authenticated principal and pass it into backend flows explicitly rather than asking module code to read HTTP context directly everywhere.
That can be done through request DTOs, endpoint enrichment, or a narrowly scoped current-actor service registered for management requests.

The implementation should prefer the smallest option that stays aligned with current module boundaries and testing patterns.

### Decision 3: Record actor identity on import-owned workflow history, not every diagnostic row

Import diagnostics describe validation and review findings about imported content.
They are not actor-owned events themselves.
Actor identity belongs on the records representing human-triggered workflow actions, such as:

- audit log entries
- import decision audit entries
- import batch or provenance metadata representing who started or last reviewed/applied a batch

This keeps actor capture meaningful without polluting every low-level diagnostic object.

### Decision 4: Keep anonymous fallback only where necessary

If a management write path somehow executes without an authenticated actor, the system may still fall back to a safe anonymous value.
However, management-authenticated flows should prefer real actor identity and tests should verify that behavior.

## Risks / Trade-offs

- [Actor identity capture can leak HTTP concerns into modules] -> Prefer explicit Host-to-module propagation or a thin scoped current-actor abstraction rather than spreading `IHttpContextAccessor` through handlers.
- [New persistence fields can create migration churn] -> Use additive migration changes and preserve existing actor display behavior for backward compatibility.
- [Display names can change over time] -> Store stable user ID separately from display text.

## Migration Plan

1. Add persisted actor identity fields to audit and import-owned records that need durable operator traceability.
2. Add Host-level actor extraction for authenticated management requests.
3. Pass actor identity into management write paths and import lifecycle commands.
4. Update API mappings and tests to verify actor identity is persisted for authenticated management actions.

Rollback strategy:

- Roll back additive fields if necessary through standard EF migration rollback.
- Preserve the legacy display actor string behavior so rollback does not remove operator-facing audit readability.

## Open Questions

- Which claim should be the canonical stable actor ID for management users: subject (`sub`) or a provider-specific user ID alias? Current expectation is `sub`.
- Whether import provenance should store only the initiator actor or also last reviewer and last applier fields depends on how much lifecycle detail the team wants in the first slice.
