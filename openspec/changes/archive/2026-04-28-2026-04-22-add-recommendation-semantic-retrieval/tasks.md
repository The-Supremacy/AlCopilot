## 1. Seed Curation

- [x] 1.1 Add a new preserved extended IBA snapshot file with curated `description` values for every currently included cocktail while keeping the raw upstream-derived file unchanged.
- [x] 1.2 Update `iba-cocktails-snapshot` import normalization to read descriptions from the extended snapshot and record derivative-source provenance metadata.
- [x] 1.3 Add Drinks Catalog tests that prove the preset import preserves descriptions and still normalizes the rest of the payload correctly.

## 2. Recommendation Semantic Retrieval

- [x] 2.1 Add Qdrant-backed projection and retrieval abstractions inside `AlCopilot.Recommendation` using contracts-facing catalog reads only.
- [x] 2.2 Build description retrieval points and aggregate Qdrant hits back to drink-level semantic signals.
- [x] 2.3 Implement the current embedding runtime seam and use it to embed both projection texts and customer-message queries.
- [x] 2.4 Feed semantic signals into deterministic candidate ranking without changing request entity resolution, hard exclusion, or grouping ownership.
- [x] 2.5 Extend the recommendation run context so narration can see compact semantic-match hints without exposing vector-store access as a model tool.

## 3. Validation

- [x] 3.1 Add unit tests for description projection building, hit aggregation, and weighting.
- [x] 3.2 Add tests for descriptive prompts such as "sparkly sweet" and fuzzy lookup handling for slight ingredient or drink misspellings.
- [x] 3.3 Add recommendation tests that prove prohibited ingredients and availability rules still override semantic hits.
- [x] 3.4 Add integration coverage for projection rebuild plus Qdrant-backed retrieval.

## 4. Documentation

- [x] 4.1 Add an ADR that accepts Qdrant as the recommendation vector store and records the curated extended snapshot direction.
- [x] 4.2 Sync architecture, operations, and local AI guidance docs with the accepted semantic retrieval direction.
- [x] 4.3 Update OpenSpec delta specs for `recommendation-chat` and `catalog-actualize-management`.
