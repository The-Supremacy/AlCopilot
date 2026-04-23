# ADR 0016: Recommendation Semantic Retrieval With Qdrant

## Status

Accepted

## Date

2026-04-22

## Context

ADR 0006 deferred the concrete choice of embeddings pipeline and vector storage until recommendation behavior was real enough to justify it.

ADR 0012 then preserved the intended future direction more concretely:
semantic retrieval should live in `Recommendation` as derived recommendation projections plus embeddings, with Qdrant as the intended vector store.

The recommendation module now exists, deterministic candidate preparation is implemented, Agent Framework orchestration is in place, and the codebase already carries an explicit embedding runtime seam.

At the same time, the current preserved IBA snapshot does not contain drink descriptions, which leaves the catalog without the strongest text source for descriptive requests such as "sparkly sweet" or "refreshing citrusy".

The project now needs an accepted end-state decision for:

- the concrete vector store
- the boundary between canonical catalog storage and derived semantic-retrieval storage
- how curated descriptions fit with the preserved upstream-derived dataset

## Decision

Adopt Qdrant as the vector store for recommendation semantic retrieval, and keep that retrieval as a `Recommendation`-owned derived projection layered on top of canonical catalog data.

Specifically:

- PostgreSQL remains the canonical source of truth for drinks, ingredients, categories, tags, recipes, and curated drink descriptions.
- `Recommendation` owns the semantic-retrieval projection and SHALL build it from contracts-facing catalog reads rather than from direct `DrinkCatalog` implementation coupling.
- Qdrant is the accepted vector store for this semantic-retrieval projection.
- The first retrieval shape SHALL store separate projection points for drink-name text, ingredient-name text, and drink-description text rather than using a vector-first rewrite of the catalog model.
- Semantic retrieval SHALL enrich recommendation intent and ranking, but deterministic hard exclusions, availability grouping, and persistence SHALL remain in normal module code.
- The raw upstream-derived IBA snapshot SHALL remain preserved unchanged.
- AlCopilot SHALL maintain a separate project-owned extended preserved snapshot for curated description text and similar curation fields that do not exist in the raw upstream file.
- The default `iba-cocktails-snapshot` preset MAY use that extended preserved snapshot, but provenance SHALL stay explicit that the file is an AlCopilot-owned derivative of the upstream dataset rather than the raw upstream source.

## Reason

This ADR is `Accepted` because the project has now reached the point where semantic retrieval is an implementation goal rather than a deferred possibility.

Choosing Qdrant follows the direction already signaled in ADR 0012, keeps the learning path aligned with the team's goals, and avoids reopening the vector-store decision in the middle of implementation.

Keeping Qdrant in `Recommendation` as derived projection storage preserves the earlier architectural rule that the catalog remains canonical and relational.

Separating the extended snapshot from the raw upstream-derived snapshot keeps provenance honest and makes future upstream refresh or re-curation safer.

## Consequences

- Local and test environments will eventually need a Qdrant service in addition to PostgreSQL and Ollama for full semantic-retrieval coverage.
- Recommendation implementation will need projection-build, rebuild, and freshness handling on top of the existing catalog read path.
- Catalog curation now explicitly includes project-owned description authoring for the preserved preset dataset.
- The project keeps a clear distinction between external source material, curated preserved seed data, canonical relational storage, derived vector projections, and runtime narration.

## Alternatives Considered

### Use PostgreSQL with pgvector instead of Qdrant

Rejected.

This would likely reduce operational moving parts, but it would diverge from the intended direction already recorded in ADR 0012 and provide less learning value for the current project goal.

### Keep the vector-store choice deferred and implement only abstractions now

Rejected.

The current change is explicitly about implementing semantic retrieval, so leaving the concrete store undecided would shift the hardest design choice into the implementation phase.

### Edit the raw upstream-derived snapshot in place

Rejected.

That would blur the line between external source material and AlCopilot-owned curation, making provenance and future refreshes harder to reason about.

## Supersedes

None.

## Superseded by

None.
