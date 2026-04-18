## Context

`CustomerProfile` and `Recommendation` currently work, but they still reflect the earlier backend shape that predates the `DrinkCatalog` refactor. `CustomerProfile` uses the older flat feature layout and does not yet emit meaningful preserved domain events even though the module already registers the shared interceptor and event storage. `Recommendation` persists chat sessions, but its runtime still centers on direct handler orchestration and a thin Semantic Kernel wrapper rather than a workflow-oriented execution model.

This change updates both modules to match the backend direction already established by ADR 0010 and `DrinkCatalog`, while also adopting the newly accepted recommendation-runtime direction from ADR 0015. The implementation remains inside the modular monolith and preserves `.Contracts` boundaries, one `DbContext` per module, and module-owned persistence.

## Goals / Non-Goals

**Goals:**

- Align `CustomerProfile` and `Recommendation` with the current backend feature structure, especially `Features/<Feature>/Abstractions`.
- Make each aggregate in the affected modules emit meaningful preserved domain events into module-owned domain-event storage.
- Introduce Microsoft Agent Framework workflows as the orchestration layer for recommendation execution without moving recommendation policy into the workflow graph.
- Keep deterministic hard filtering, candidate scoring, and make-now versus buy-next grouping explicit, testable, and framework-agnostic.
- Update local recommendation runtime defaults toward a CPU-friendly Ollama-hosted model profile based on `gemma4:e4b`.

**Non-Goals:**

- Introduce true multi-agent recommendation behavior.
- Introduce vector retrieval into the live recommendation request path in this change.
- Rewrite the frontend portals as part of this backend refactor.
- Relax module boundaries or bypass `.Contracts`-based cross-module communication.

## Decisions

### 1. Feature structure follows the refactored backend convention

`CustomerProfile` and `Recommendation` will both move to the `DrinkCatalog`-style feature structure.
Feature-local interfaces move into `Features/<Feature>/Abstractions`.
Additional folders such as `QueryServices`, `Workflows`, `Narration`, or aggregate-named subfolders are optional and should appear only where the feature is crowded enough to justify them.

Why this over a mandatory deep folder hierarchy:
the project wants consistency with low ceremony, and these modules are still small enough that forced extra nesting would add noise rather than clarity.

### 2. CustomerProfile stays single-aggregate but becomes a first-class event-producing module

`CustomerProfile` continues to center on a single profile aggregate keyed to the authenticated customer identity.
The aggregate remains the source of truth for customer-owned ingredient preferences and home-bar state.
The refactor adds explicit domain events such as profile creation and profile updates and normalizes ingredient sets inside the aggregate so handlers remain orchestration-only.

Aggregate roots and value objects:

- Aggregate root: `CustomerProfile`
- Value objects: `CustomerIdentity`, plus any refactor-local value object introduced for normalized ingredient-set semantics if that improves clarity
- Domain events: profile created and profile updated events with logical versioned names

Why this over splitting profile state into multiple aggregates:
the current behavior is cohesive, customer-owned, and does not yet justify additional transactional boundaries.

### 3. Recommendation uses Agent Framework workflows only for orchestration

The recommendation request path will be modeled as a bounded workflow with typed steps.
The workflow coordinates:

- loading the current customer profile snapshot
- loading the recommendation catalog or future derived recommendation projection
- applying hard exclusions and candidate scoring
- grouping results into make-now and buy-next outcomes
- invoking the narration/model step
- appending assistant output and persisting the session

The workflow does not become the home of business rules.
Deterministic policy lives in normal collaborators such as candidate builders, retrieval services, and narration services.

Aggregate roots and value objects:

- Aggregate root: `ChatSession`
- Value objects: session title and any chat/message role types introduced during the refactor if they improve invariants
- Domain events: session started, customer message recorded, and assistant message recorded events with logical versioned names

Why this over direct handler orchestration:
the workflow model is a better fit for the team's learning goals and future AI-oriented pipeline work while still supporting bounded deterministic execution.

Why this over putting all logic inside workflow nodes:
keeping business policy outside the workflow graph preserves reuse, testability, and replaceability.

### 4. Semantic Kernel becomes optional implementation detail rather than architectural center

Semantic Kernel may still be used where it helps with Ollama integration, prompt composition, or model abstractions, but the architectural center of recommendation execution becomes the Agent Framework workflow.
Read-only tools remain optional helpers rather than the primary runtime model.

Why this over keeping Semantic Kernel as the primary orchestration story:
the new workflow direction should be reflected explicitly in the module architecture instead of hidden behind a handler plus a single model-call wrapper.

### 5. Local recommendation runtime defaults to a CPU-friendly Gemma 4 profile

The default local recommendation model target becomes `gemma4:e4b` on Ollama.
Configuration remains module-owned and overridable, but the documented default should match the team's realistic local RAM budget.

Why this over larger local defaults:
the project can allocate roughly 16 GB of RAM for the model on a 32 GB development VM, so the smaller Gemma 4 profile is the practical day-to-day choice.

## Risks / Trade-offs

- [Preview framework churn] -> Isolate Agent Framework usage behind workflow-specific seams and avoid scattering framework types through unrelated domain code.
- [Workflow over-engineering] -> Keep deterministic policy in plain services and use workflow steps mainly for sequencing, branching, and state handoff.
- [Breaking persistence changes] -> Treat the refactor as a breaking backend change, generate explicit migrations per module, and keep rollback to the previous migration set possible during development.
- [Recommendation quality regressions during model/runtime swap] -> Preserve deterministic candidate preparation and add unit plus integration coverage around candidate grouping and response persistence.
- [Structure drift during refactor] -> Apply ADR 0014 conventions consistently and update backend guidance documents alongside the code changes.

## Migration Plan

1. Refactor `CustomerProfile` structure and aggregate behavior first, including domain events, repository/query-service placement, and migration updates.
2. Introduce the new recommendation workflow alongside the current module shape, then migrate handlers to the workflow path.
3. Replace the Semantic Kernel-centered orchestration path after the workflow can produce equivalent persisted session behavior.
4. Update module configuration defaults to the new Ollama model profile.
5. Run module unit tests, integration tests, and architecture tests before treating the refactor as complete.

Rollback strategy:

- Because the project is not live, rollback during implementation can use normal code reversion plus reverting the newest per-module migrations if needed.
- Keep module database migrations isolated per module schema so either module can be rolled back independently during development.

## Open Questions

- Which concrete Agent Framework package set best fits the recommendation workflow while keeping Ollama integration straightforward?
- Should `Recommendation` keep all session-related types in one feature root or introduce an additional aggregate-specific level during implementation?
- Does `CustomerProfile` benefit from a dedicated normalized ingredient-set value object, or is aggregate-local normalization sufficient?
