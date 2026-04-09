# Drinks Catalog — Domain Overview

The Drinks Catalog is AlCopilot's shared source of drink knowledge.
It gives the product a common language for what a drink is, what goes into it, and how drinks are grouped and described.

---

## Purpose

The Drinks Catalog exists so the rest of AlCopilot can reason about drinks consistently.
It provides the canonical vocabulary for drinks, ingredients, ingredient groupings, and drink labels used across browsing, suggestion, and future user-facing experiences.

---

## Core Concepts

- **Drink** — A named beverage concept that users can browse, recognize, and learn about.
- **Recipe** — The ingredient composition that explains what goes into a drink and in what quantity or style.
- **Ingredient** — A reusable drink component such as vodka, lime juice, tonic water, or mint.
- **Ingredient Category** — A business grouping for ingredients such as spirits, juices, syrups, mixers, or garnishes.
- **Tag** — A reusable label that helps describe or classify drinks for discovery and understanding.

---

## Relationships

- A drink is described through its recipe, tags, and descriptive presentation.
- A recipe is made of one or more ingredients expressed in drink-specific proportions or notes.
- An ingredient can appear in many different drinks.
- Ingredient categories organize the ingredient library rather than the drink library.
- Tags describe drinks, not ingredients.
- Drinks and ingredients are reusable reference concepts for the rest of the product.

---

## Actors And Uses

- **Guests and users** use the catalog to browse drinks, understand what a drink is, and recognize how drinks differ from one another.
- **Product and AI features** use the catalog as shared reference data when presenting, filtering, or reasoning about drinks.
- **Catalog managers** curate the drink library, ingredient library, and classification vocabulary.

---

## Business Vocabulary

- A drink can be both a concrete entry in the catalog and a recognizable beverage concept for users.
- Ingredients are shared reference items, not drink-specific duplicates.
- Recommended brands are drink-specific guidance layered onto an ingredient within a recipe context.
- Tags are discovery language for drinks.
- Ingredient categories are organization language for ingredients.

---

## Ownership Boundaries

- The Drinks Catalog **owns** the business meaning of drinks, recipes, ingredients, ingredient categories, and tags.
- The Drinks Catalog **does not own** social opinion, recommendation outcomes, personal collections, or identity.
- Other modules should treat the catalog as the shared reference domain for drink-related terminology.

---

## OpenSpec Note

Use this document to understand drinks-catalog language, concepts, and ownership before writing or reviewing OpenSpec changes.
Use OpenSpec specs to define the observable behavior a feature must support.
