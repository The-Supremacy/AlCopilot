## Why

`CustomerProfile` and `Recommendation` were introduced before the backend settled on the refactored `DrinkCatalog` structure and before the team chose its current recommendation-runtime direction. The modules now lag behind the project's preferred feature layout, preserved-domain-event usage, and recommendation orchestration approach, which makes future work harder to reason about and less valuable as a learning-focused AI portfolio project.

## What Changes

- Refactor `CustomerProfile` to follow the current backend feature conventions used by `DrinkCatalog`, including `Abstractions` placement, aggregate-owned domain events, and clearer command/query separation.
- Refactor `Recommendation` around a bounded recommendation workflow that uses Microsoft Agent Framework for orchestration while keeping deterministic filtering, scoring, and grouping logic in plain module code.
- **BREAKING** Replace the current recommendation-runtime assumption that Semantic Kernel tool calling is the primary orchestration model.
- Update recommendation runtime defaults toward Ollama-hosted `gemma4:e4b` for local CPU-oriented development.
- Preserve the existing module split and `.Contracts` boundaries between `CustomerProfile`, `Recommendation`, and `DrinkCatalog`.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `customer-profile`: profile-management behavior is clarified around normalized customer-owned ingredient sets and stable profile snapshots for recommendation workflows.
- `recommendation-chat`: recommendation execution shifts from Semantic Kernel-centered orchestration to a bounded workflow that preserves deterministic preparation outside the model.

## Impact

- Affected modules: `server/src/Modules/AlCopilot.CustomerProfile`, `server/src/Modules/AlCopilot.Recommendation`, and related contracts/tests.
- Affected dependencies: Microsoft Agent Framework packages for recommendation orchestration and updated Ollama model/runtime configuration.
- Affected persistence: both modules will need migration updates to align aggregate/event storage and workflow persistence with the refactor.
- Affected documentation: ADRs, backend architecture guidance, and spec deltas for `customer-profile` and `recommendation-chat`.
