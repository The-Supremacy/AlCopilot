# ADR 0015: Recommendation Workflows With Agent Framework

## Status

Accepted

## Date

2026-04-17

## Context

ADR 0012 established the split between `CustomerProfile` and `Recommendation`, preserved deterministic candidate building outside the model, and chose a bounded first iteration for recommendation orchestration.

Since then, the project has clarified additional goals for the recommendation module:

- gain hands-on experience with current Microsoft AI orchestration tooling as part of the project's learning and portfolio value
- keep deterministic recommendation policy explicit and testable
- preserve the modular-monolith boundaries and `.Contracts`-only cross-module communication
- remain practical for CPU-oriented local development on Ollama-hosted models
- avoid turning recommendation behavior into an opaque framework-specific graph

The team considered staying with direct handler orchestration and a thin Semantic Kernel wrapper.
The team also considered adopting Microsoft Agent Framework more fully for deterministic multi-step recommendation execution.

## Decision

Adopt Microsoft Agent Framework workflows for `Recommendation` orchestration, but keep deterministic recommendation policy in normal module code.

Specifically:

- The `Recommendation` module SHALL use Agent Framework workflows as the orchestration layer for bounded recommendation execution.
- Workflow steps SHALL coordinate module-owned collaborators such as profile snapshot readers, candidate builders, retrieval services, narration services, and persistence services.
- Deterministic recommendation policy such as hard exclusions, candidate scoring, and make-now versus buy-next grouping SHALL remain in plain module services or aggregates rather than being embedded directly into workflow definitions.
- Agent Framework adoption in this module is explicitly motivated by both architectural fit and learning value.
- Recommendation persistence SHALL remain module-owned and SHALL stay outside model-owned execution.
- Recommendation narration SHALL use a stable Agent Framework `ChatClientAgent` with native `AgentSession` persistence as the primary carrier for model-visible conversation state.
- Recommendation narration SHALL keep per-conversation history inside framework-managed session-backed chat history rather than reconstructing it from the business transcript on each run.
- Recommendation narration SHALL pass deterministic per-run recommendation context as explicit `ChatMessage` input to the run rather than mutating request-scoped `AIContextProvider` state before invocation.
- Read-only model tools MAY still be used when helpful, but Agent Framework workflows replace Semantic Kernel tool calling as the primary orchestration mechanism.
- The recommendation module SHOULD prefer `ChatClientAgentOptions`, `AgentSession`, and framework-managed chat history for model-side conversation state.
- `AIContextProvider` SHOULD be reserved for durable agent memory or retrieval-style augmentation where the provider can resolve context from the session or backing services without request-scoped setter calls.
- Semantic Kernel MAY remain as an implementation detail for model integration where it adds value, but it is no longer the architectural center of recommendation execution.
- The default local CPU-oriented development model profile for recommendation SHALL be `gemma4:e4b` via Ollama unless a more suitable local default is later documented.

## Reason

This ADR is `Accepted` because the team intentionally wants real experience with current AI-native orchestration tooling, not only the minimal implementation required to ship the feature.

Agent Framework workflows provide a credible way to model the recommendation pipeline as explicit deterministic steps without forcing the project into a multi-agent design it does not currently need.
At the same time, keeping recommendation policy in plain module code limits framework coupling and preserves testability.

The chosen model direction reflects current local-development constraints.
The project can allocate roughly 16 GB of RAM to local model execution, which makes `gemma4:e4b` a better default fit than larger Ollama-hosted models for day-to-day development.

## Consequences

- The recommendation module will gain a new workflow dependency and orchestration layer.
- Recommendation code should be structured so workflow steps call plain services rather than becoming the sole home of business rules.
- The project gains hands-on experience with Agent Framework while limiting replacement cost if the framework changes significantly.
- Recommendation docs and specs should stop describing Semantic Kernel tool calling as the primary orchestration model.
- Recommendation code will keep a deliberate split where the domain aggregate stores business-visible turns/events, while the serialized Agent Framework session stores the canonical model-visible conversation state.
- Recommendation agent sessions may remain ephemeral objects per request, but the persisted framework session becomes the canonical carrier for model-visible history across requests.
- Recommendation narration input is now intentionally split between durable framework session state and explicit per-run context messages, which keeps transient recommendation snapshots out of long-term session memory.
- Local development defaults should be updated away from older model choices toward `gemma4:e4b`.
- Agent Framework is now on a stable release line, so future upgrades should track current stable packages rather than preview or RC builds by default.

## Alternatives Considered

### Keep direct handler orchestration and use Semantic Kernel as the main recommendation runtime

Rejected.
This would be simpler in the short term, but it would not provide the workflow-oriented learning value the team wants and would leave the module on a less future-facing orchestration path.

### Put deterministic recommendation logic directly inside Agent Framework workflow steps

Rejected.
This would make the workflow graph the real home of business policy and would increase framework coupling.

### Adopt AutoGen directly

Rejected.
Microsoft now positions Agent Framework as the direct successor path for both AutoGen and Semantic Kernel, so new project investment should target the newer framework rather than older AutoGen-first patterns.

## Supersedes

ADR 0012 in recommendation orchestration direction only.

## Superseded by

None.
