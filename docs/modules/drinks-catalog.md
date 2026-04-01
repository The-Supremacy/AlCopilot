# Drinks Catalog — Domain Overview

The Catalog is AlCopilot's drinks database — the foundation module that every other module depends on. It owns the definition of what a drink is, what goes into it, and how drinks are organized.

## Core Concepts

- **Drink** — A named beverage with a recipe. Has a name, description, optional image, and one or more tags. A drink is defined by its recipe entries.
- **Tag** — A flat label for drink classification (e.g., "Cocktail", "Mocktail", "Shot", "Beer", "Wine", "Hot Drink", "Rum-based"). Tags are many-to-many with drinks — a Mojito can be both "Cocktail" and "Rum-based".
- **IngredientCategory** — A classification for ingredients (e.g., Spirit, Liqueur, Syrup, Juice, Garnish, Mixer). Organizes the ingredient library, not the drinks themselves.
- **Ingredient** — A reusable component (e.g., "Vodka", "Lime Juice", "Simple Syrup"). Belongs to exactly one IngredientCategory. Shared across drinks — "Vodka" in a Martini is the same ingredient as "Vodka" in a Moscow Mule. May list notable brands as metadata (e.g., Vodka → ["Absolut", "Grey Goose", "Stolichnaya"]).
- **RecipeEntry** — The link between a drink and an ingredient, including quantity as free-form text (e.g., "2 oz", "a splash", "to taste") and an optional recommended brand for that specific recipe (e.g., "Captain Morgan" for a spiced rum cocktail).

## Relationships

- A drink has one or more tags
- A tag has zero or more drinks
- A drink has one or more recipe entries (a drink without ingredients is invalid)
- An ingredient belongs to exactly one ingredient category
- An ingredient can appear in many drinks via recipe entries
- An ingredient exists independently of any drink (deleting a drink does not delete its ingredients)

## Business Rules

- Drink names are globally unique
- Tag names are unique
- Ingredient names are unique
- Ingredient category names are unique
- A drink must have at least one recipe entry
- Drinks support soft deletion — removed from browsing/search but retained for audit and referential integrity with other modules (Social ratings, Recommendation history)
- Tags, ingredient categories, and ingredients cannot be deleted if referenced by active drinks

## Actors

- **Anonymous user** — Can browse drinks by tag, view drink details, and search
- **Authenticated user** — Same as anonymous (future: favorites, personal notes)
- **Admin** — Can create, update, and soft-delete drinks; manage tags, ingredient categories, and ingredients

## Module Boundaries

- The Catalog **owns**: drinks, recipe entries, ingredients, ingredient categories, tags
- The Catalog **does NOT own**: ratings/reviews (Social), AI suggestions (Recommendation), personal inventory (Inventory), user identity (Identity)
- Other modules reference drinks by ID via `AlCopilot.Catalog.Contracts` — they never access Catalog internals
