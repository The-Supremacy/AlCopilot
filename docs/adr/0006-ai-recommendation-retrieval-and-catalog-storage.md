# ADR 0006: AI Recommendation Retrieval And Catalog Storage

## Status

Deferred

## Date

2026-04-09

## Context

AlCopilot's first implemented drinks capability is the `DrinkCatalog` module.
It currently provides a relational catalog for drinks, ingredients, ingredient categories, tags, recipes, browse, search, and CRUD behavior backed by PostgreSQL.

The product direction is for future drink recommendation to use an LLM for reasoning over user context such as preferences, home bar inventory, venue menu availability, and natural-language request input.
That raised a design question early: whether the Drinks Catalog needs to be modeled in a special AI-first way now, such as storing the catalog primarily in markdown files, shaping the module around a vector database from the start, or introducing MCP-specific storage and access patterns before recommendation work begins.

The team wants to preserve the preferred direction without forcing speculative implementation before the recommendation module, retrieval pipeline, and evaluation needs are better understood.

## Decision

The Drinks Catalog remains a normal relational module backed by PostgreSQL.
No AI-specific storage model is required in the catalog at this stage.

The preferred future direction for AI-driven recommendation is:

- PostgreSQL remains the canonical source of truth for drinks, ingredients, categories, tags, recipes, and future manager edits.
- Recommendation flows SHALL apply deterministic filtering outside the LLM for hard constraints such as ingredient availability, forbidden ingredients, and venue or home-bar eligibility.
- The LLM SHALL reason over a bounded candidate set rather than over the entire raw catalog.
- If AI-oriented read models are needed later, they SHOULD be implemented as derived recommendation projections built from canonical catalog data rather than as a replacement for the relational model.
- Vector embeddings and a vector database MAY be added later to support semantic retrieval over those recommendation projections, but they are not required for the first recommendation iteration.
- MCP is not required for the first in-process recommendation workflow inside the AlCopilot application. Standard application services, queries, and Semantic Kernel plugin or tool-calling patterns are sufficient unless future cross-process interoperability creates a concrete need for MCP.
- Markdown or similar document banks MAY be used for prompt templates, guidance, or operator notes, but they SHALL NOT become the primary source of truth for the live drinks catalog.

## Reason

This ADR is `Deferred` because the direction is useful and likely, but the project is not yet implementing the recommendation module, recommendation projections, embeddings pipeline, or vector retrieval infrastructure.

Deferring this work avoids over-designing the Drinks Catalog around unproven retrieval requirements while still preserving the current architectural understanding:

- the catalog does not need special AI-first storage now
- the first milestone can safely start with PostgreSQL as canonical storage
- AI-specific projections, semantic retrieval, and vector search can be layered in later when recommendation behavior is being built and evaluated

Reconsider this ADR when the project starts implementing the recommendation module, when natural-language retrieval quality becomes a real product need, or when a cross-process AI integration creates a concrete case for MCP.

## Consequences

- The current `DrinkCatalog` module can continue evolving as a conventional relational bounded context without speculative AI infrastructure.
- Future recommendation work should be designed as an orchestration layer on top of the catalog instead of rewriting catalog storage around the LLM.
- If semantic retrieval is later introduced, the project will need a derived projection pipeline, embedding generation, freshness strategy, and evaluation approach.
- The team keeps a clear distinction between canonical business data, derived AI-facing projection data, and runtime LLM reasoning.

## Alternatives Considered

### Design the Drinks Catalog as an AI-first or vector-first module now

Rejected for now.
There is not enough implementation experience yet to justify shaping the core catalog around embeddings or vector storage before recommendation flows exist.

### Store catalog knowledge primarily in markdown or similar document banks

Rejected for now.
That would make live catalog management, import, correction, and structured querying harder than keeping the catalog in PostgreSQL.
Document banks remain useful for prompts and guidance, not as the main catalog store.

### Introduce MCP as a required part of the first recommendation architecture

Rejected for now.
The first recommendation workflow can run inside the same application boundary and call normal services and tool functions directly.
MCP remains a possible later interoperability layer, not a current architectural requirement.
