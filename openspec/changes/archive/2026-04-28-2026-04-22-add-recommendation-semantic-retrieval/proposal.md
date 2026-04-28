## Why

Recommendation chat currently handles exact drink names, fuzzy drink or ingredient mentions, and a small fixed set of preference signals, but it does not yet understand broader natural-language requests such as "I want a sparkly sweet drink" in a principled way.

The current preserved IBA snapshot also does not contain drink descriptions, which means the catalog lacks the strongest text field for semantic retrieval.

This change matters now because embeddings are the last planned foundation for the current recommendation learning track, and the team explicitly wants to implement that foundation with Qdrant while preserving the raw upstream dataset as an untouched external source.

## What Changes

- Add an AlCopilot-owned extended IBA snapshot that keeps the raw upstream snapshot intact while adding curated drink descriptions for the full current preset catalog.
- Update the `iba-cocktails-snapshot` import preset to use the extended preserved snapshot by default when no payload is provided.
- Add Qdrant-backed semantic retrieval in `Recommendation` over derived projection documents built from curated drink descriptions.
- Use semantic retrieval to enrich recommendation ranking for descriptive natural-language requests without replacing deterministic filtering and grouping.
- Use catalog-backed fuzzy lookup, not semantic retrieval, for slight drink-name or ingredient-name typos.
- Document the accepted Qdrant direction and the curated-seed relationship to the upstream dataset.

## Capabilities

### Modified Capabilities

- `recommendation-chat`: recommendation chat gains semantic retrieval support for descriptive customer requests while typo-tolerant drink or ingredient matching remains catalog-backed fuzzy lookup.
- `catalog-actualize-management`: the preserved `iba-cocktails-snapshot` preset uses an AlCopilot-owned extended snapshot that includes curated descriptions while preserving upstream provenance.

## Impact

- Affected modules: `server/src/Modules/AlCopilot.DrinkCatalog`, `server/src/Modules/AlCopilot.Recommendation`, and related tests.
- Affected dependencies: Qdrant becomes the accepted vector store for recommendation semantic retrieval, alongside the existing Ollama and Agent Framework stack.
- Affected documentation: a new OpenSpec behavior change under `recommendation-chat` and `catalog-actualize-management`, a new ADR for Qdrant-backed semantic retrieval, and synced architecture and operations guidance.
