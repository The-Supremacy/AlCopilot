# Recommendation Refactor Step 2: MAF-Native Provider Architecture

## Purpose

This document defines the second refactor step for the recommendation module.
Its job is to move recommendation runtime orchestration to Microsoft Agent Framework native surfaces after Step 1 has simplified the request model and retrieval order.

This step is based on the earlier single-plan `mfa-native.md` document, but it assumes the updated two-flow model from Step 1 rather than preserving the old intent taxonomy.

---

## Architectural Goal

The evaluated and invoked recommendation agent should be a fully assembled `AIAgent` that already contains:

- bartender instruction set
- recommendation tools
- chat history loading and storing
- the ordered RAG pipeline expressed as context providers

The outer application layer should still own:

- business session lifecycle
- auth and ownership checks
- `AgentSession` serialization persistence
- feedback persistence
- diagnostics persistence
- recommendation business enrichments stored outside model-visible history

This keeps the agent MAF-native at the orchestration edge without moving recommendation policy into framework types.

---

## Core Principle

Each meaningful RAG stage becomes its own provider.
Each provider owns a strongly typed state contract.
Only the final provider should emit the model-visible recommendation context message unless there is a deliberate product reason to expose more.

Do not expose intermediate retrieval artifacts directly to the LLM by default.

---

## Target Runtime Shape

### Inside the `AIAgent`

- `ChatOptions.Instructions`
- `ChatOptions.Tools`
- `RecommendationChatHistoryProvider`
- ordered recommendation `MessageAIContextProvider`s
- optional reducers and filters

### Outside the `AIAgent`

- `ChatSession` aggregate lifecycle
- `AgentSessionStateJson` persistence
- recommendation-group persistence for the product UI
- tool-invocation summary persistence
- execution trace persistence
- feedback persistence
- semantic indexing and catalog refresh workflows

---

## Provider Pipeline

The recommendation agent should use the following provider order.

### 1. `RecommendationChatHistoryProvider`

Responsibilities:

- load prior user and assistant messages from `ChatSession`
- prepend them to the current request
- persist newly produced user and assistant history messages after invocation

Provider state:

- `RecommendationChatHistoryState`
  - `SessionId`
  - `CustomerId`

Notes:

- manage model-visible chat history only
- do not persist recommendation groups, diagnostics, feedback, or tool summaries here
- keep provider state limited to routing metadata rather than aggregate instances

### 2. `RecommendationRequestAnalysisProvider`

Responsibilities:

- inspect the current invocation messages
- derive lexical analysis from the latest user message

Provider state:

- `RecommendationRequestAnalysisState`
  - `CustomerMessage`
  - `NormalizedMessage`
  - `PreferenceSignals`
  - `DrinkSearchText`
  - `IngredientSearchText`
  - any wording flags still needed for answer framing or tool selection

Model-visible output:

- none

### 3. `RecommendationCatalogInputsProvider`

Responsibilities:

- load the current customer profile
- load the current recommendation catalog snapshot

Provider state:

- `RecommendationCatalogInputsState`
  - `Profile`
  - `Drinks`
  - optional lightweight metadata such as hash or count

Model-visible output:

- none

Notes:

- prefer re-querying per invocation rather than durably storing a large catalog payload in provider session state
- overwrite invocation-specific state on each run

### 4. `RecommendationIntentResolutionProvider`

Responsibilities:

- resolve the top-level flow
- resolve drink and ingredient entities
- combine lexical, fuzzy, and semantic-free exact recovery before deeper retrieval

Provider state:

- `RecommendationIntentResolutionState`
  - `Intent`

Model-visible output:

- none

Notes:

- this provider should implement the Step 1 two-flow model
- the result should distinguish `Recommendation` versus `DrinkDetails`
- ingredient mentions belong to request attributes, not peer intent kinds

### 5. `RecommendationEligibilityProvider`

Responsibilities:

- apply hard eligibility narrowing for `Recommendation` requests
- preserve the full catalog for `DrinkDetails` requests except where explicit product rules say otherwise

Provider state:

- `RecommendationEligibilityState`
  - `EligibleDrinkIds`
  - optional eligibility summary for diagnostics

Model-visible output:

- none

Notes:

- apply only hard constraints here
- do not prefilter on owned ingredients, favorites, or dislikes
- this provider may be folded into candidate-context assembly if the implementation stays clearer that way, but the conceptual stage should remain explicit

### 6. `RecommendationSemanticSearchProvider`

Responsibilities:

- embed the current customer message when semantic retrieval is useful
- perform semantic search over the derived catalog projection
- intersect or constrain semantic results against the eligible set for `Recommendation` requests when supported
- store semantic retrieval evidence for downstream stages

Provider state:

- `RecommendationSemanticSearchState`
  - `SemanticSearchResult`
  - optional compact retrieval summaries for diagnostics

Model-visible output:

- none

Notes:

- semantic indexing stays outside runtime orchestration
- query-time retrieval belongs here
- `DrinkDetails` requests may use lighter or fallback semantic recovery only when exact and fuzzy recovery were insufficient

### 7. `RecommendationCandidateContextProvider`

Responsibilities:

- build deterministic recommendation groups for `Recommendation`
- build final drink-detail context for `DrinkDetails`
- build the final run context object
- emit the model-visible system context message

Provider state:

- `RecommendationCandidateContextState`
  - `RecommendationGroups`
  - `RunContext`
  - optional candidate or detail diagnostics

Model-visible output:

- one system message built from the final run-context message builder

Notes:

- this should be the only provider that emits recommendation RAG context for the LLM
- deterministic scoring and detail shaping should stay in normal module services rather than in provider glue code

### 8. Optional `RecommendationDiagnosticsProvider`

Responsibilities:

- collect provider summaries and timing metadata
- expose compact diagnostics for outer persistence after the run

Provider state:

- `RecommendationDiagnosticsState`
  - retrieval summary
  - intent summary
  - candidate or detail summary
  - provider timings if available

Model-visible output:

- none

Notes:

- this provider is optional
- outer services may remain the primary diagnostics owners

---

## State Model

Create one explicit state type per provider.

Recommended types:

- `RecommendationChatHistoryState`
- `RecommendationRequestAnalysisState`
- `RecommendationCatalogInputsState`
- `RecommendationIntentResolutionState`
- `RecommendationEligibilityState`
- `RecommendationSemanticSearchState`
- `RecommendationCandidateContextState`
- optional `RecommendationDiagnosticsState`

Avoid a single giant state blob that mixes all stages together.

---

## Persistence Boundaries

### What `ChatHistoryProvider` should persist

- user message text
- assistant message text

### What outer application services should persist

- `AgentSessionStateJson`
- recommendation groups shown in the UI
- tool invocation summaries
- execution traces
- feedback ratings and comments
- future evaluation artifacts

### What should not be persisted as history

- recommendation candidate scores
- semantic hit internals
- provider-specific intermediate state
- diagnostics blobs unless explicitly chosen

This split prevents the history provider from becoming an awkward business-session repository.

---

## Refactor Plan

### Phase 1: Establish provider-state contracts

Tasks:

- create provider state types
- document which provider reads and writes which state
- decide which state is durable across turns versus overwrite-per-invocation

Decision:

- durable only for routing and identity state
- overwrite per invocation for RAG intermediate artifacts

### Phase 2: Implement `RecommendationChatHistoryProvider`

Tasks:

- replace `NoOpChatHistoryProvider`
- load prior user and assistant turns from `ChatSession`
- persist newly generated user and assistant history messages
- initialize provider state with `SessionId` and `CustomerId`

Important:

- do not hold a mutable aggregate instance inside the provider
- always load and store by identifier

### Phase 3: Remove `RecommendationCurrentRunContextAccessor`

Tasks:

- delete `RecommendationCurrentRunContextAccessor`
- delete the current accessor-based run-context handoff
- stop pre-building run context in `RecommendationConversationService`

This is the key step that makes direct `agent.RunAsync(...)` and `EvaluateAsync(...)` natural.

### Phase 4: Implement the provider chain

Tasks:

- create each provider in order
- wire typed dependencies directly where possible
- reuse existing recommendation services for the real business work

Important:

- providers are orchestration boundaries, not replacements for business services

### Phase 5: Simplify or retire `RecommendationRunContextService`

Tasks:

- remove its current role as the runtime pipeline spine
- keep lower-level services such as semantic search, intent resolution, candidate building, and run-context building
- optionally retain a façade only for non-agent or test scenarios

Preferred direction:

- the provider chain becomes the runtime spine

### Phase 6: Refactor `RecommendationNarratorAgentFactory`

Tasks:

- build the full provider pipeline during agent creation
- wire the real history provider
- keep instructions and tools unchanged unless improvement is needed

Dependency guidance:

- inject typed dependencies for scoped provider creation where possible
- avoid passing `IServiceProvider` everywhere
- if lifetime constraints force delayed scope creation later, prefer `IServiceScopeFactory`

### Phase 7: Simplify `RecommendationConversationService`

Tasks:

- remove manual history building for model invocation
- remove prebuilt run-context handoff
- keep session create and load logic
- keep `AgentSession` restore and serialize logic
- after invocation, persist business enrichments intentionally kept outside history

Expected result:

- the outer service becomes thinner and more application-oriented
- the agent becomes the real runtime surface

### Phase 8: Define post-run business enrichment extraction

Tasks:

- decide how outer persistence reads final provider states after invocation
- persist recommendation groups, tool summaries, and diagnostics from those states

Important:

- the UI still needs persisted structured recommendation groups
- drink-detail runs may need a lighter persisted enrichment shape

### Phase 9: Add MAF-native evaluation tests

Tasks:

- create real MAF evaluation tests over the fully assembled `AIAgent`
- keep deterministic recommendation tests
- add provider-state-focused integration tests where needed

Evaluation should cover:

- recommendation from descriptive vibe requests
- recommendation from ingredient-constrained requests
- prohibited ingredient exclusion
- drink details by explicit drink name
- typo-tolerant drink detail recovery
- tool-call expectations for detail and recipe-oriented requests

---

## Risks

### Risk: provider-state sprawl

Mitigation:

- one typed state per provider
- overwrite invocation-specific state unless there is a clear durability requirement

### Risk: duplicated persistence paths

Mitigation:

- history provider owns model-visible history only
- outer services own business enrichments

### Risk: overusing `IServiceProvider`

Mitigation:

- inject typed dependencies for provider creation
- use `IServiceScopeFactory` only when invocation-time scope creation is actually required

### Risk: stale intermediate state between turns

Mitigation:

- reset or overwrite run-specific provider state on every invocation

### Risk: carrying forward the old request taxonomy inside new providers

Mitigation:

- treat Step 1 as a prerequisite
- validate provider contracts against the two-flow model before implementation starts

---

## Recommended First Slice

The first implementation slice for this step should be:

1. add `RecommendationChatHistoryProvider`
2. remove `RecommendationCurrentRunContextAccessor`
3. add `RecommendationRequestAnalysisProvider`
4. add `RecommendationCatalogInputsProvider`
5. add `RecommendationIntentResolutionProvider`
6. add `RecommendationEligibilityProvider`
7. add `RecommendationSemanticSearchProvider`
8. add `RecommendationCandidateContextProvider`

Only after that should the team wire MAF-native evaluation tests.

---

## Exit Criteria

Step 2 is complete when:

- the recommendation agent runs through native MAF history and provider surfaces
- the provider chain is the runtime orchestration spine
- outer services only own business lifecycle and persistence concerns outside model-visible history
- evaluation can exercise the real assembled `AIAgent`

---

## Relationship To Step 1

This step depends on Step 1.
The provider architecture should encode the corrected two-flow business model and revised retrieval order rather than preserving the current runtime shape behind new abstractions.
