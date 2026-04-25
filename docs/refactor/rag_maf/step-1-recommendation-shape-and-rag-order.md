# Recommendation Refactor Step 1: Request Shape And RAG Order

## Purpose

This document defines the first refactor step for recommendation runtime behavior before the Microsoft Agent Framework orchestration refactor.
Its job is to simplify the business shape of recommendation requests, clarify the top-level flows, and correct the retrieval order so later MAF-native provider work is built on the right model.

---

## Why This Step Exists

The current runtime shape mixes business request taxonomy with implementation shortcuts.
That creates extra complexity in intent resolution, candidate building, and future provider design.

In particular:

- ingredient-led requests are currently modeled as a separate peer intent even though they behave like constrained recommendation requests
- `Hybrid` exists mainly because the top-level intent taxonomy is carrying too much meaning
- semantic retrieval currently runs before the system knows which flow it is serving
- the current runtime spine makes it harder to see which retrieval stages are universally required versus flow-specific

This step fixes the business model first.
The later MAF-native refactor should then express that corrected model through providers rather than preserving today's awkward split.

---

## Final Goal

The runtime should support two top-level flows only:

1. `Recommendation`
2. `DrinkDetails`

Everything else should become request attributes, constraints, or evidence used within one of those flows.

---

## Target Request Model

### Top-Level Flow

Replace the current top-level intent split with:

- `Recommendation`
- `DrinkDetails`

Remove:

- `IngredientDiscovery`
- `RecipeLookup`
- `Hybrid`

### Request Attributes

The request model should preserve the details needed for deterministic policy and narration without turning those details into top-level flows.

Recommended attributes:

- `RequestedDrinkName`
- `RequestedIngredientNames` or `RequiredIngredientNames`
- `PreferenceSignals`
- `LooksLikeDetailsLookup`
- `LooksLikeHowToMakeRequest`

Notes:

- `RequestedDrinkName` is an entity-resolution result, not a top-level flow
- ingredient mentions in recommendation requests are constraints or ranking signals, not a separate flow
- wording such as "how do I make" can still matter for narration and tool choice without requiring a separate intent kind

---

## Flow Definitions

### Flow 1: `Recommendation`

Use this flow when the customer is asking what to drink, what fits their preferences, or what they can make with ingredients they mention in chat.

Examples:

- "I want something citrusy and refreshing."
- "What can I make with gin and lime?"
- "Recommend me a tequila drink that isn't too sweet."
- "I have rum, pineapple, and mint."

Behavior:

- build a candidate set
- apply profile policy and request constraints
- rank candidate drinks
- group results for the UI
- let the narrator explain the grouped outcome

### Flow 2: `DrinkDetails`

Use this flow when the customer is asking about a specific drink by name.

Examples:

- "Tell me about a Negroni."
- "What's in a Margarita?"
- "How do I make an Old Fashioned?"
- "Give me the recipe for mojito."

Behavior:

- resolve the requested drink entity
- recover from typos or partial names when possible
- retrieve the drink details needed for the answer
- allow detail-oriented narration and recipe explanation
- avoid recommendation-group behavior unless there is an explicit product reason

---

## Target RAG Order

### Current Problem

The current runtime runs semantic search before intent resolution.
That is acceptable for some recommendation turns, but it is not the cleanest order for the product's common flows.

### Recommended Order

The runtime should move toward this request-time order:

1. Load bounded catalog and profile inputs.
2. Run cheap lexical analysis over the latest customer message.
3. Resolve the top-level flow and obvious entity mentions.
4. Apply hard eligibility narrowing for `Recommendation` requests.
5. Run semantic retrieval only when it materially helps the current flow.
6. Build deterministic recommendation groups or drink-detail context.
7. Emit the final model-visible context.
8. Invoke the narrator and persist business enrichments outside model-owned execution.

---

## Hard Eligibility Before Semantics

For `Recommendation` requests, some constraints should narrow the candidate universe before semantic ranking.

Apply before semantic scoring:

- prohibited ingredients from the customer profile
- explicit required ingredients from the current customer message
- any future hard exclusion rules with clear policy meaning

Do not treat these as pre-semantic hard filters by default:

- owned ingredients
- favorite ingredients
- disliked ingredients

Rationale:

- owned ingredients should still support both "available now" and "restock" groups
- favorites and dislikes are ranking signals, not hard eligibility rules
- hard filters reduce noise in semantic ranking and make retrieval easier to explain

### Important Exception

Do not prefilter `DrinkDetails` requests by profile policy before entity resolution.

If the customer asks for details about a specific drink, the system should still resolve and explain that drink even if it contains disliked or prohibited ingredients.
Policy conflicts should inform the answer, not hide the drink.

---

## Intent Resolution Guidance

The intent resolver should become simpler after this change.

It should answer these questions:

1. Is this a `Recommendation` request or a `DrinkDetails` request?
2. Is there a resolved drink name?
3. Are there resolved ingredient constraints?
4. What lexical preference signals did the customer actually say?
5. Is there wording that should influence tool choice or answer framing?

It should no longer need to invent a `Hybrid` category just to express multiple attributes at once.

---

## Candidate Building Guidance

`DeterministicRecommendationCandidateBuilder` should become more explicit about which responsibilities belong to recommendation ranking versus drink-detail handling.

### For `Recommendation`

The builder should:

- scope candidates from the eligible catalog
- apply hard exclusions
- incorporate ingredient constraints
- score using preference signals, owned ingredients, profile preferences, and semantic evidence
- produce stable grouped recommendation output

### For `DrinkDetails`

The runtime should not force detail requests through recommendation grouping just to reuse the same shape.
If reuse is convenient, keep it shallow and explicit, but the conceptual path should be entity resolution plus detail retrieval rather than recommendation ranking.

---

## Suggested Implementation Changes

### Intent Model

- replace `RecommendationRequestIntentKind` values with `Recommendation` and `DrinkDetails`
- move ingredient-led and recipe-phrasing information into intent attributes

### Resolver

- update `RecommendationRequestIntentResolver` to classify the two flows
- keep exact, fuzzy, and semantic entity recovery
- treat ingredient mentions as recommendation constraints

### Deterministic Policy

- separate recommendation candidate work from drink-detail lookup behavior
- make hard pre-semantic filters explicit
- keep semantic evidence separate from lexical signals to avoid double counting

### Documentation

- update recommendation RAG docs to describe the two-flow model
- update tests and evaluation cases to reflect the new taxonomy

---

## Validation Targets

This step should be considered complete when the system can cleanly support:

- recommendation from vibe-only requests
- recommendation from vibe plus ingredient requests
- recommendation with hard profile exclusions
- drink details by explicit drink name
- drink details by typo-tolerant drink lookup
- recommendation ranking that does not rely on `Hybrid` or `IngredientDiscovery`

---

## Risks

### Risk: hidden dependency on the old enum split

Mitigation:

- search for all uses of `RecommendationRequestIntentKind`
- update tests before deleting legacy values

### Risk: over-filtering before semantic retrieval

Mitigation:

- limit pre-semantic narrowing to truly hard constraints
- keep owned-ingredient grouping and preference ranking out of the hard-filter stage

### Risk: forcing drink details through recommendation shaping

Mitigation:

- keep `DrinkDetails` as a distinct flow with its own context-building expectations

---

## Exit Criteria

Step 1 is complete when:

- the runtime taxonomy is reduced to `Recommendation` and `DrinkDetails`
- ingredient-led requests are modeled as recommendation constraints
- semantic retrieval order is clarified around flow-specific needs
- deterministic services expose a cleaner foundation for MAF-native provider orchestration

---

## Relationship To Step 2

This step intentionally precedes the MAF-native refactor.
Step 2 should express this corrected business model through ordered provider boundaries rather than preserving the current runtime shape inside new framework abstractions.
