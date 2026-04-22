## 1. Shared Backend Conventions And Infrastructure

- [x] 1.1 Update backend structure guidance and architecture tests for the `Features/<Feature>/Abstractions` convention and any new workflow-related boundaries.
- [x] 1.2 Add and register the required Microsoft Agent Framework packages and module wiring for recommendation workflows.
- [x] 1.3 Update recommendation configuration defaults and local runtime guidance to target Ollama-hosted `gemma4:e4b`.

## 2. CustomerProfile Module Refactor

- [x] 2.1 Restructure `CustomerProfile` feature files to the new convention, moving interfaces into `Features/Profile/Abstractions`.
- [x] 2.2 Move profile normalization and state transitions into the aggregate and add preserved domain events for profile creation and updates.
- [x] 2.3 Update repositories, query services, handlers, and module registration to match the new structure and event-producing aggregate behavior.
- [x] 2.4 Add or update module migrations for any persistence changes and keep rollback viable during development.
- [x] 2.5 Add handler-level unit tests and TestContainers-backed integration tests covering normalized saves, scoped profile loading, and empty profile snapshots.

## 3. Recommendation Module Refactor

- [x] 3.1 Restructure `Recommendation` feature files to the new convention, including `Abstractions` placement and any justified workflow-specific subfolders.
- [x] 3.2 Introduce a bounded Agent Framework workflow that orchestrates profile loading, candidate preparation, narration, and session persistence.
- [x] 3.3 Keep deterministic candidate building, grouping, and policy validation in plain collaborators invoked by the workflow rather than embedding them directly in workflow steps.
- [x] 3.4 Update the chat session aggregate, repository, and persistence model to emit preserved domain events for session start and recorded turns.
- [x] 3.5 Replace the current Semantic Kernel-centered orchestration path with the workflow-based execution path while keeping any remaining model helper usage read-only.
- [x] 3.6 Add or update module migrations for workflow or aggregate persistence changes and keep rollback viable during development.
- [x] 3.7 Add handler/workflow unit tests and TestContainers-backed integration tests covering session creation, deterministic exclusions, grouped recommendations, bounded model execution, and persisted chat history.

## 4. Cross-Module Validation

- [x] 4.1 Update cross-module and architecture tests so `Recommendation` continues to depend only on `DrinkCatalog.Contracts` and `CustomerProfile.Contracts`.
- [x] 4.2 Run targeted module unit tests, integration tests, and architecture tests for `CustomerProfile`, `Recommendation`, and affected shared/backend projects.
