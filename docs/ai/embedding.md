# Embedding In AlCopilot

## Purpose

This document explains how embeddings fit into AlCopilot at the architecture and behavior level.
It is intentionally distilled.
The goal is to capture the system mindset, the integration boundaries, the trust model, and the meaning of the main tuning parameters without turning the document into a code map.

---

## Core Model

AlCopilot treats embedding, vector search, and recommendation logic as three different layers.

| Layer                | Main responsibility                                                   |
| -------------------- | --------------------------------------------------------------------- |
| Embedding model      | Turn text into semantic vectors                                       |
| Vector store         | Persist vectors and retrieve nearest neighbors efficiently            |
| Recommendation logic | Decide what retrieved signals mean for intent, ranking, and narration |

This distinction matters.
The embedding model places text into vector space.
Qdrant searches inside that vector space.
The Recommendation module decides whether those search results are useful enough to influence behavior.

---

## Embedding Vs Vector Search

The embedding model is the part that creates vectors from text.
That is the semantic encoding step.

The vector database does not create meaning.
It stores vectors and runs nearest-neighbor search over them.
In this project Qdrant uses cosine distance, which means search is based on directional similarity in vector space.

So the practical split is:

- embedding model: language to vectors
- vector store: vectors to nearest candidates
- application logic: candidates to product behavior

That is why `SearchLimit` is not the approximation algorithm.
It only controls how many nearest candidates AlCopilot asks Qdrant to return.

---

## What Embeddings Are For Here

Embeddings are a derived retrieval layer.
They do not replace the relational catalog.

They are especially helpful when the customer describes meaning rather than exact catalog text, for example:

- "I want something sparkly and sweet"
- "Give me something citrusy and refreshing"
- "What should I make with prosecco?"

They are much less suited to be the first tool for:

- exact catalog truth
- hard business constraints
- typo recovery

Typo recovery is mostly a lexical problem.
Taste and style prompts are semantic problems.
That separation is deliberate.

---

## Canonical Boundary

PostgreSQL remains the canonical source of truth.
Drink data, recipes, ingredients, and curated descriptions live in normal module-owned relational storage.

Qdrant stores a projection derived from that canonical data.
It is not treated as a second source of truth.

That means:

- `DrinkCatalog` owns canonical catalog data
- `Recommendation` owns semantic retrieval behavior
- vector points are built from contracts-facing catalog reads
- deterministic rules still live in normal backend code

This keeps the modular-monolith boundary honest.

---

## Integration Shape

The current integration has two phases.

### Import-time indexing

When the catalog is refreshed, AlCopilot projects each drink into semantic texts and replaces the semantic catalog in Qdrant.

Today those texts are intentionally simple:

- drink name
- ingredient names
- drink description

This keeps indexing understandable while still providing useful semantic coverage.

### Request-time retrieval

When a customer sends a recommendation message, the system:

1. embeds the customer message
2. searches Qdrant for nearby vectors
3. aggregates point hits back into drink-level signals
4. uses those signals inside deterministic recommendation logic
5. exposes the grounded result to the narrator

The LLM does not perform raw vector search itself.
It speaks over already-grounded backend context.

---

## Retrieval Philosophy

AlCopilot uses a layered retrieval strategy:

1. exact mention detection where possible
2. lexical fuzzy matching for typo recovery
3. semantic retrieval for broader meaning recovery

That division of labor is important because it keeps each tool solving the kind of problem it is actually good at.

Semantic retrieval is therefore a complement, not the policy engine.
It can enrich intent and ranking, but it does not override hard exclusions, ownership constraints, or availability grouping.

---

## Trust

An embedding hit is not the same thing as a trusted decision.

Semantic search gives AlCopilot candidates by geometry.
The Recommendation module still has to decide whether those candidates are reliable enough to influence behavior.

That trust concept shows up in two different ways:

### Ranking trust

Drink-level weighted scores help compare retrieved candidates.
These are useful for ranking and for adding semantic hints into the final run context.

### Decision trust

Intent fallback is stricter.
For semantic drink-name or ingredient fallback, the system looks at facet-level confidence and requires the best match to clear both:

- a minimum score
- a margin over the runner-up

This prevents broad descriptive prompts from being over-interpreted as exact recipe lookup or ingredient lookup.

That is a core design principle in this system:
semantic retrieval may suggest, but behavior should only trust it when confidence is strong enough for the specific decision being made.

---

## Main Tuning Parameters

Most embedding-related tuning in this codebase is deterministic backend tuning rather than prompt tuning.

### Retrieval shape

`SearchLimit`

- Meaning: how many nearest vector hits to request from Qdrant for one search
- If you increase it: recall usually improves because more potentially relevant hits are available for aggregation, but the tail is more likely to introduce weakly related noise
- If you decrease it: retrieval becomes tighter and often cleaner, but you are more likely to miss supporting evidence for multi-facet or subtly phrased queries
- Why it exists: too few hits can miss useful supporting evidence, while too many hits add noise
- Current default reasoning: `18` is large enough to capture several drink facets without letting the tail dominate the aggregation step

### Facet weighting

`NameWeight`

- Meaning: multiplier applied to name hits in aggregation
- If you increase it: near-name matches become more influential in ranking, which helps exact-ish lookup behavior, but can over-favor shallow lexical resemblance
- If you decrease it: ranking relies less on name similarity and more on other evidence, which can help broader taste prompts, but may weaken direct drink-name recovery
- Why it exists: direct drink-name similarity should matter, but not drown out broader descriptive evidence
- Current default reasoning: `1.25` gives name matches a modest boost without making every near-name hit decisive

`IngredientWeight`

- Meaning: multiplier applied to ingredient hits
- If you increase it: ingredient-led requests have more impact on ranking, which can help "what can I make with X" style prompts, but broad ingredients may start matching too many drinks
- If you decrease it: ingredient evidence becomes less decisive, which can reduce noisy broad matches, but may under-serve users who are clearly asking from an ingredient constraint
- Why it exists: ingredient-led requests matter, but ingredient names are often broad and recurring across many drinks
- Current default reasoning: `1.0` keeps ingredient evidence as the neutral baseline

`DescriptionWeight`

- Meaning: multiplier applied to description hits
- If you increase it: vague taste, mood, and style language influences retrieval more strongly, which usually helps semantic discovery, but can also let poetic or fuzzy wording overpower more concrete signals
- If you decrease it: retrieval becomes less sensitive to descriptive language, which can improve precision for concrete queries, but makes the system less helpful for natural-language vibe requests
- Why it exists: description text is the strongest semantic source for vague natural-language taste and style prompts
- Current default reasoning: `1.5` intentionally favors descriptive meaning recovery over shallow lexical coincidence

### Trust thresholds

`NameMatchMinScore`

- Meaning: minimum facet score required before semantic name fallback is trusted
- If you increase it: semantic name fallback becomes stricter and safer, but more true positives will fail to trigger when phrasing is imperfect
- If you decrease it: fallback activates more often, which may feel more helpful, but also raises the chance of turning a vague prompt into the wrong exact-drink interpretation
- Why it exists: prevents weak near-name matches from becoming recipe lookup decisions
- Current default reasoning: `0.72` is intentionally selective rather than permissive

`IngredientMatchMinScore`

- Meaning: minimum facet score required before semantic ingredient fallback is trusted
- If you increase it: ingredient fallback only triggers on stronger evidence, which reduces false certainty, but may miss legitimate ingredient-led asks expressed casually
- If you decrease it: the system becomes more willing to infer an ingredient match, which improves recall, but can over-read broad cocktail vocabulary as a concrete ingredient request
- Why it exists: ingredient semantic space can be broad and noisy, especially for related cocktail vocabularies
- Current default reasoning: `0.72` matches the drink-name threshold for now and keeps fallback conservative

`FacetMatchMinScoreGap`

- Meaning: minimum lead over the runner-up before the top semantic facet match is trusted
- If you increase it: the top hit has to win more decisively before fallback is trusted, which improves caution, but rejects more ambiguous yet still useful matches
- If you decrease it: the system accepts narrower wins, which increases responsiveness, but also makes close contests look more certain than they really are
- Why it exists: a top hit that barely beats the next hit is often not decisive enough for intent fallback
- Current default reasoning: `0.05` gives the best hit room to be meaningfully better without requiring an unrealistic margin

### Model and store wiring

`EmbeddingModelId`

- Meaning: the embedding model used to create vectors
- If you switch to a stronger model: semantic quality may improve, but vector dimensions, latency, hardware needs, and reindexing costs can all change with it
- If you switch to a smaller or faster model: indexing and query-time embedding may get cheaper, but semantic nuance and separation between similar concepts may get worse
- Why it exists: embedding quality and vector dimensionality come from the model, not from Qdrant
- Current default reasoning: `nomic-embed-text` is a practical local default for semantic retrieval experiments

`CollectionName`

- Meaning: the Qdrant collection that stores the semantic catalog projection
- Why it exists: the vector store is projection storage and needs a stable named collection
- Current default reasoning: one explicit recommendation semantic collection keeps local development simple

`QdrantEndpoint`

- Meaning: the Qdrant service location
- Why it exists: vector storage is an external operational dependency, even though it is derived data
- Current default reasoning: local development uses the default local Qdrant endpoint

---

## Why These Knobs Live In Config

These settings are configuration because they are tuning parameters, not domain invariants.

They represent choices about:

- how broad retrieval should be
- which facets should matter most
- how much confidence a semantic fallback must earn

That makes them part of the retrieval harness around the embedding model rather than part of the catalog domain itself.

---

## Relationship To LLM Narration

Embeddings and LLM narration are related but distinct.

Embeddings help retrieve relevant drinks and semantic hints.
Deterministic backend logic preserves business constraints.
The LLM turns the grounded result into a useful conversational response.

This is much closer to retrieval-augmented application behavior than to "let the model decide everything."

---

## Related Guidance

- [LLM Development](llm.md)
- [ADR 0016](../adr/0016-recommendation-semantic-retrieval-with-qdrant.md)
