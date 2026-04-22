# Drinks Catalog — Domain Overview

The Drinks Catalog is AlCopilot's shared source of drink knowledge.
It gives the product a common language for what a drink is, what goes into it, and how drinks are grouped and described.

---

## Purpose

The Drinks Catalog exists so the rest of AlCopilot can reason about drinks consistently.
It provides the canonical vocabulary for drinks, ingredients, and drink labels used across browsing, suggestion, and future user-facing experiences.

---

## Core Concepts

- **Drink** — A named beverage concept that users can browse, recognize, and learn about.
- **Recipe** — The ingredient composition that explains what goes into a drink and in what quantity or style.
- **Ingredient** — A reusable drink component such as vodka, lime juice, tonic water, or mint.
- **Tag** — A reusable label that helps describe or classify drinks for discovery and understanding.
- **Drink Category** — A first-class classification label for drinks such as Longdrink, Shortdrink, or Hot Drink.

---

## Relationships

- A drink is described through its recipe, tags, and descriptive presentation.
- A drink can have a category that groups it into a recognizable family for browsing and catalog management.
- A recipe is made of one or more ingredients expressed in drink-specific proportions or notes.
- An ingredient can appear in many different drinks.
- Tags describe drinks, not ingredients.
- Drinks and ingredients are reusable reference concepts for the rest of the product.

---

## Actors And Uses

- **Guests and users** use the catalog to browse drinks, understand what a drink is, and recognize how drinks differ from one another.
- **Product and AI features** use the catalog as shared reference data when presenting, filtering, or reasoning about drinks.
- **Catalog managers** curate the drink library, ingredient library, and classification vocabulary.
- **Catalog managers** also run explicit import sync batches that start with immediate validation, expose row-level review snapshots, and then apply canonical catalog updates.

---

## Business Vocabulary

- A drink can be both a concrete entry in the catalog and a recognizable beverage concept for users.
- Ingredients are shared reference items, not drink-specific duplicates.
- Recommended brands are drink-specific guidance layered onto an ingredient within a recipe context.
- Tags are discovery language for drinks.
- Drink categories are core catalog labels for drinks.
- The current seed dataset reference is [`rasmusab/iba-cocktails`](https://github.com/rasmusab/iba-cocktails), used as curated source material rather than as a runtime dependency.
- The preferred upstream seed file is `iba-web/iba-cocktails-web.json`; `iba-web/iba-cocktails-ingredients-web.csv` is a supporting inspection source when recipe or ingredient parsing needs review.
- Import batch provenance is the stored source context for an import sync run, including source reference and operator-facing metadata.
- Import diagnostics are persisted validation or normalization findings attached to an import batch.
- Audit log entries capture successful mutating commands so operators can review catalog and import changes directly.

---

## Seed Dataset Alignment

The current import-sync direction assumes curated snapshot import from the `rasmusab/iba-cocktails` repository, not direct remote fetching.
That repository is useful because it already expresses the IBA cocktail set in JSON plus ingredient-row CSV forms that are close to our current importer shape.

The current preferred mapping is:

- `iba-web/iba-cocktails-web.json` -> baseline drink seed source
- `iba-web/iba-cocktails-ingredients-web.csv` -> review and troubleshooting source for ingredient-row inspection
- `wikipedia/iba-cocktails-wiki.json` -> optional comparison or enrichment source when recipe wording in the IBA-web version needs operator review

The importer's canonical JSON payload is still AlCopilot-owned and should stay explicit, even when a preserved snapshot is used:

- `drinks[].name` <- upstream cocktail name
- `drinks[].description` <- curated free-text description separate from preparation details
- `drinks[].method` <- curated preparation method stored separately for reuse
- `drinks[].garnish` <- curated garnish instructions stored separately for reuse
- `drinks[].category` <- curated drink-category label derived from the import seed and normalized by AlCopilot
- `drinks[].tagNames` <- derived by AlCopilot curation, not imported blindly from upstream
- `drinks[].recipeEntries[].ingredientName` <- curated ingredient label normalized from upstream ingredient wording
- `drinks[].recipeEntries[].quantity` <- upstream quantity text or curated quantity text
- `ingredients[].name` <- curated canonical ingredient

This means the upstream repository informs our seed workflow, but it does not define our domain model or replace operator curation.

---

## Persistence Notes

Import batches are workflow records rather than core catalog aggregates.
For that reason, the import workflow stores provenance, diagnostics, review rows, workflow-history details, and apply summaries as JSONB payloads on the batch record.
This is a narrow exception used to preserve operator review context without introducing many short-lived workflow tables.
Core business aggregates such as Drink, Tag, and Ingredient should not treat JSONB as the default persistence pattern.

---

## Ownership Boundaries

- The Drinks Catalog **owns** the business meaning of drinks, recipes, ingredients, and tags.
- The Drinks Catalog **does not own** social opinion, recommendation outcomes, personal collections, or identity.
- Other modules should treat the catalog as the shared reference domain for drink-related terminology.

---

## OpenSpec Note

Use this document to understand drinks-catalog language, concepts, and ownership before writing or reviewing OpenSpec changes.
Use OpenSpec specs to define the observable behavior a feature must support.
