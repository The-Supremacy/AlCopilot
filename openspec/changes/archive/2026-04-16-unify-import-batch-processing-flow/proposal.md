## Why

The Drink Catalog import workflow is already simpler at the product level, but its internal processing model still exposes split public operations that only make sense when executed together.
That leaves handlers assembling `ImportBatch` state across multiple public calls and weakens the workflow boundary we want to preserve before the module grows more complex.

## What Changes

- Replace separate public import-processing steps with one public processing operation that returns diagnostics, review summary, and review rows together.
- Introduce a combined processing result object so prepared and reviewed snapshot recording always moves coherent workflow data as one unit.
- Refactor `ImportBatch` snapshot-recording methods to consume the combined processing result instead of loosely related arguments.
- Keep `ImportBatch.Create(...)` focused on identity, provenance, and raw import content while removing split public workflow assembly from handlers.
- Keep apply fallback behavior, but rebuild missing snapshot data through the same combined processing path and return non-ready results instead of using exception-driven business flow.
- Clarify batch-scoped naming such as `batchApplyReadiness` where current wording is ambiguous.
- Update backend, shared contract, and management-portal code where the refactor causes outward naming or result-shape ripple effects.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `catalog-actualize-management`: import processing requirements change so initialization, review refresh, and apply fallback all use one atomic processing path
- `management-portal-frontend-testing`: frontend coverage requirements change so import workspace tests remain stable if batch/apply result contracts or readiness naming are refined

## Impact

- Drink Catalog import aggregate, processing service, handlers, and tests
- Shared import DTOs or apply-result contracts if the refactor surfaces clearer naming outward
- Management API client and management portal import tests if contract changes ripple to the frontend
- Runtime and module documentation that currently describes the import workflow internals
