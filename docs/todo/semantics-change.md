# Recommendation Semantic Retrieval Cleanup

## Goal

Simplify recommendation semantic retrieval so Qdrant handles descriptive meaning only.
Drink names and ingredients should be resolved through exact catalog matching and fuzzy lookup.

## Current Finding

The current import apply flow can rebuild the semantic catalog from stale data.
`ApplyImportBatchHandler` calls `applyService.ApplyAsync(...)`, then `drinkQueryService.GetAllAsync(...)`, then `ReplaceRecommendationSemanticCatalogCommand`, and only afterward `unitOfWork.SaveChangesAsync(...)`.
`DrinkQueryService.GetAllAsync(...)` uses `AsNoTracking()` database queries, so it may not see uncommitted tracked import changes.

Full rebuilds are acceptable, but the rebuild must happen after the partial import data is applied and visible to the query used for indexing.

## Plan

1. Fix catalog import freshness.
   Persist the applied import batch before reading the full catalog for semantic rebuild, or provide an explicit post-apply read model that includes the applied changes.
   Keep audit and batch completion semantics coherent when moving the save boundary.

2. Change semantic projection to description-only.
   Update `RecommendationSemanticProjectionBuilder` so it creates only `Description` points and skips drinks without descriptions.
   Keep stable point IDs based on drink id, facet kind, and description text.

3. Simplify semantic result models.
   Remove name and ingredient semantic fallback concepts where they are no longer needed.
   Keep drink-level weighted description evidence and summary hints.

4. Simplify intent resolution.
   Remove semantic name and ingredient resolution from `RecommendationRequestIntentResolver`.
   Keep exact mention detection and fuzzy lookup as the supported typo-tolerance path for names and ingredients.

5. Simplify options.
   Remove or deprecate `NameWeight`, `IngredientWeight`, `NameMatchMinScore`, and `IngredientMatchMinScore`.
   Keep description weighting if it still helps ranking calibration, or replace it with a single semantic boost factor.

6. Update candidate scoring.
   Keep semantic score as a ranking boost from description matches.
   Ensure disliked, prohibited, excluded, and missing-ingredient rules still outrank semantic relevance where appropriate.

7. Update tests.
   Replace semantic name/ingredient fallback tests with fuzzy resolver tests.
   Keep semantic integration coverage focused on description matches such as "sparkly sweet" or "light celebratory".
   Run recommendation unit, integration, and eval suites.

8. Update documentation.
   Reference ADR 0020 from any active server architecture guidance if semantic retrieval guidance is documented outside ADRs.
   Keep `docs/ai/embedding.md` aligned if it describes recommendation semantic facets.

## Acceptance Criteria

- Qdrant contains only description-derived recommendation points.
- Name and ingredient typo tolerance works through fuzzy lookup.
- Description-led prompts still improve recommendation ranking.
- Import-triggered semantic rebuilds use catalog data that includes the applied import.
- Recommendation unit tests, recommendation integration tests, and recommendation eval tests pass.
