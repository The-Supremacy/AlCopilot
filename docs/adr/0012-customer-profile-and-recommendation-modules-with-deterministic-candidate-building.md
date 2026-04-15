# ADR 0012: Customer Profile And Recommendation Modules With Deterministic Candidate Building

## Status

Accepted

## Date

2026-04-14

## Context

The first customer portal slice needs to persist personal preference data, maintain a home bar, support conversational recommendation sessions, and prepare for later retrieval augmentation.
The existing backend has one domain module today, `DrinkCatalog`, and the team explicitly wants the customer portal work to establish real cross-module communication patterns instead of placing product logic in the Host.
The project also intends to use .NET Semantic Kernel for recommendation chat, but the first iteration should remain testable, deterministic in safety-critical areas, and practical on local Ollama-hosted models.

The chosen direction must support:

- persistent customer profile data that is clearly user-owned
- recommendation chat that can evolve independently from profile storage
- deterministic exclusion of prohibited ingredients
- a bounded candidate set before LLM ranking
- Semantic Kernel chat history that can be reconstructed from persisted state without storing connector-specific blobs
- future addition of embeddings and Qdrant without rewriting the canonical catalog

## Decision

Adopt two new backend modules for the customer portal slice: `CustomerProfile` and `Recommendation`.

Specifically:

- `CustomerProfile` owns customer preference and home-bar state keyed to the authenticated user identity.
- `CustomerProfile` persists favorite ingredient IDs, disliked ingredient IDs, prohibited ingredient IDs, and owned ingredient IDs.
- `Recommendation` owns chat sessions, chat turns, recommendation orchestration, and model invocation.
- `Recommendation` interacts with `DrinkCatalog` and `CustomerProfile` through their `.Contracts` boundaries only.
- The recommendation flow MUST apply deterministic filtering outside the LLM for hard constraints such as prohibited ingredients.
- Disliked ingredients are treated as soft ranking penalties rather than hard exclusion in the first iteration.
- Recommendation results are grouped into `make now` and `buy next` outcomes before or alongside LLM explanation.
- Semantic Kernel is used with a limited read-only tool-calling surface only.
- The first tool surface is limited to read-style helpers such as profile snapshot lookup, candidate lookup, and ingredient-gap analysis.
- Persist chat sessions and turns in an application-owned normalized format and reconstruct Semantic Kernel `ChatHistory` at runtime.
- Do not introduce vector retrieval in the first implementation slice.
- When semantic retrieval is later added, it should live in `Recommendation` as derived recommendation projections plus embeddings, with Qdrant as the intended vector store.

## Reason

This ADR is `Accepted` because it creates clear module ownership while keeping the first recommendation slice safe, inspectable, and practical to build.
Separating `CustomerProfile` from `Recommendation` protects user-owned state from orchestration churn and keeps recommendation logic free to evolve.
Deterministic candidate building reduces hallucination risk around forbidden ingredients and inventory-sensitive suggestions.
Limiting Semantic Kernel tool calling to read-only helpers preserves debuggability and avoids turning the first release into an open-ended agent loop.
The decision is active direction for the first customer portal implementation and establishes the module and orchestration boundaries the team explicitly wants now.

## Consequences

- Two new module and contracts projects will be introduced instead of placing customer logic in the Host.
- Architecture tests must expand to enforce boundaries across three domain modules.
- Recommendation persistence must account for both conversational turns and limited function-call metadata.
- The first recommendation UX will be more predictable and easier to debug than a fully agentic design.
- Semantic retrieval remains a follow-up concern and does not block the first customer portal milestone.

## Alternatives Considered

### Keep all customer recommendation logic in the Host initially

Rejected.
This would be faster in the short term, but it would weaken module boundaries and teach the wrong long-term pattern.

### Create one combined module for profile and recommendation

Rejected.
This would simplify setup, but it would mix stable customer-owned state with faster-moving orchestration and model behavior.

### Let the LLM decide all filtering and recommendation logic

Rejected.
This would reduce backend logic, but it would create unnecessary safety and correctness risk around prohibited ingredients and inventory-sensitive suggestions.

### Start with tool-first agent orchestration

Rejected for the first slice.
The team wants Semantic Kernel support, but the first implementation should stay bounded, read-oriented, and easy to inspect.

## Supersedes

None.

## Superseded by

None.
