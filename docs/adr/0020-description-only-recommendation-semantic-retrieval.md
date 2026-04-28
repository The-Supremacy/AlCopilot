# ADR 0020: Description-Only Recommendation Semantic Retrieval

## Status

Accepted

## Date

2026-04-28

## Context

ADR 0016 accepted Qdrant-backed semantic retrieval for recommendations and chose an initial projection shape with separate drink-name, ingredient-name, and drink-description points.
That implementation proved useful, but the name and ingredient facets overlap with deterministic catalog matching and PostgreSQL-backed fuzzy search.
Drink names and ingredient names primarily need exact matching and typo tolerance rather than semantic similarity.
Using embeddings for those facets can make intent resolution harder to reason about because semantically nearby ingredients or drink names are not necessarily valid substitutes.

The strongest semantic need is descriptive matching.
Prompts such as "sparkly sweet", "light and celebratory", or "spirit-forward and aromatic" benefit from embeddings because they often describe mood, flavor, or style rather than exact catalog text.

The current import apply flow also revealed a freshness concern for semantic rebuilds.
`ApplyImportBatchHandler` applies catalog changes, reads the full catalog through `DrinkQueryService.GetAllAsync()`, rebuilds the semantic catalog, and only then calls `SaveChangesAsync()`.
Because `DrinkQueryService` uses `AsNoTracking()` database queries, the rebuild may read the pre-apply persisted catalog rather than the newly applied batch state.

## Decision

Recommendation semantic retrieval SHALL index and search drink descriptions only.

Specifically:

- Qdrant remains the vector store for recommendation semantic retrieval.
- The semantic projection SHALL create description facet points only.
- Drink-name matching SHALL rely on exact catalog matching and fuzzy drink lookup.
- Ingredient matching SHALL rely on exact catalog matching and fuzzy ingredient lookup.
- Semantic search SHALL be used for descriptive recommendation cues and ranking hints, not as the primary path for drink-name or ingredient typo tolerance.
- The semantic rebuild triggered by catalog import SHALL run from a catalog view that reflects the applied import batch.

## Reason

This ADR is `Accepted` because the project already has semantic retrieval in place and the desired simplification is clear.
Description-only retrieval gives Qdrant a narrower and more valuable job: matching meaning in descriptive language.
Keeping names and ingredients in deterministic and fuzzy matching code makes intent resolution more predictable, cheaper, and easier to test.

The import freshness rule is included because full rebuilds are acceptable only when they rebuild from current canonical catalog data.
A stale full rebuild is simpler operationally but incorrect behaviorally.

## Consequences

- The semantic projection and aggregation code can become smaller because name and ingredient facets no longer need vector indexing.
- Intent resolution will be easier to explain: lexical/fuzzy handles entities, semantic retrieval handles descriptive meaning.
- Existing semantic tests that assert name or ingredient semantic fallback will need to be removed or rewritten around fuzzy matching.
- Semantic search will no longer rescue ingredient or drink-name misspellings through embeddings; fuzzy matching must cover those cases.
- Import apply flow must ensure catalog changes are persisted, or otherwise query-visible, before rebuilding the semantic catalog.

## Alternatives Considered

### Keep Name, Ingredient, and Description Semantic Facets

Rejected.

This preserves the current implementation but keeps overlap between semantic retrieval and fuzzy entity matching.
It also allows vector similarity to influence hard entity resolution in cases where exact or fuzzy matching is more appropriate.

### Keep Name and Ingredient Facets Only as Intent Fallbacks

Rejected.

This would reduce their ranking impact but still keeps two competing mechanisms for typo tolerance.
It leaves ambiguity over whether a name or ingredient was resolved because of lexical similarity, fuzzy trigram similarity, or embedding proximity.

### Remove Semantic Retrieval Entirely

Rejected.

Deterministic and fuzzy matching do not solve descriptive prompts well.
Description embeddings still add useful recommendation signal for flavor, mood, and style requests.

## Supersedes

Partially supersedes ADR 0016's initial projection-shape decision to store drink-name, ingredient-name, and drink-description semantic points.
ADR 0016 remains accepted for Qdrant as the recommendation semantic-retrieval vector store and for semantic retrieval as a Recommendation-owned derived projection.

## Superseded by

None.
