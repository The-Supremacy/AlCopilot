# Recommendation RAG Pipeline

## Purpose

This document describes the runtime retrieval and narration pipeline used by AlCopilot recommendations.
It sits between the lower-level model guidance in [llm.md](llm.md) and the embedding-specific guidance in [embedding.md](embedding.md).

---

## Boundary

The recommendation LLM is a narrator over grounded backend context.
It does not own catalog truth, hard exclusions, profile policy, vector retrieval, or candidate scoring.

PostgreSQL remains the canonical source for drinks, recipes, ingredients, profile preferences, and chat sessions.
Qdrant stores a derived description-only recommendation projection for semantic retrieval.
The Recommendation module combines exact catalog reads, fuzzy lookup, semantic retrieval, and deterministic scoring before the model writes.

---

## Request-Time Flow

The request-time spine is the recommendation conversation service plus the Agent Framework provider pipeline.

1. Load current recommendation inputs, including catalog details and customer profile.
2. Capture and analyze the current customer message from the model-visible invocation messages.
3. Run semantic search over the derived Qdrant description projection only when lexical analysis suggests it will help.
4. Resolve request intent, requested entities, and request descriptors.
5. Build deterministic candidates with profile constraints, availability, exact, fuzzy, and semantic evidence.
6. Build a compact run context message for the narrator through the recommendation run context provider.
7. Invoke the Agent Framework narrator with optional read-only tools.
8. Persist the native agent messages, customer-visible turns, candidate groups, tool calls, and development execution trace when enabled.

This is a hybrid RAG pipeline.
It uses relational truth for canonical data, PostgreSQL fuzzy lookup for typo-tolerant entity recovery, vector retrieval for descriptive cues, deterministic scoring for policy, and the LLM for final conversational presentation.

---

## Intent And Retrieval Roles

Exact mention detection handles clear catalog names and ingredient names already present in the loaded catalog.
Fuzzy lookup handles lexical misspellings and partial entity recovery.
Semantic retrieval handles taste, texture, mood, and description-led requests such as "light sparkling" or "citrusy and refreshing".

Lexical request descriptors should represent words the user actually said.
Semantic hints should remain separate evidence so the system does not double count semantic retrieval as both intent and ranking input.

---

## Candidate Policy

`DeterministicRecommendationCandidateBuilder` owns the candidate policy.
It filters hard-prohibited ingredients, separates available-now from restock candidates, applies profile preferences, accounts for owned ingredients, and adds semantic score influence.

The candidate builder should explain matches with stable signals.
Literal signals come from the request and catalog text.
Semantic hints come from the semantic retrieval result.

---

## Tools

The narrator receives read-only tools for cases where the deterministic context is not enough:

- `search_drinks` resolves drink names before recipe lookup.
- `lookup_drinks_by_ingredient` finds catalog drinks containing a requested ingredient.
- `lookup_drink_recipe` retrieves exact measurements, method, garnish, and brand information.

These tools are support tools, not the main retrieval pipeline.
Common recommendation turns should usually be answerable from the run context.

---

## Diagnostics And Feedback

Assistant turns persist deterministic recommendation groups and tool invocations.
Development runs can also persist execution trace steps when `Recommendation:Observability:PersistExecutionTraceInDevelopment` is enabled.

User feedback should be attached to assistant turns.
Negative feedback should preserve enough context to reproduce the failure: original query, resolved intent, semantic and fuzzy evidence, candidate groups, tool calls, and final response.
The existing execution trace is the preferred place for that diagnostic shape until production usage proves a separate evaluation store is necessary.

---

## Validation Direction

Current validation starts with deterministic local checks:

- intent and entity resolution cases
- candidate ranking and exclusion cases
- tool-selection cases for recipe and ingredient requests
- response guardrails such as not recommending prohibited ingredients

These are currently maintained as recommendation regression tests rather than Agent Framework Eval artifacts.
Agent Framework evaluators fit a future inner loop once the team wants evaluator-backed scoring on top of the current regression corpus.
LLM-as-judge evaluators can be added later for groundedness, relevance, coherence, and response completeness once there is a small golden dataset from real feedback.

---

## Current Non-Goals

CodeAct is not currently part of the recommendation runtime.
The present workflow usually needs zero to two read-only tool calls, so direct tool calling remains simpler and easier to observe.

The system also does not fine-tune the narrator from user feedback yet.
Feedback first feeds evaluation, retrieval tuning, prompt tuning, and candidate scoring changes.
