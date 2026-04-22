# Customer Profile — Domain Overview

The Customer Profile module is AlCopilot's customer-owned preference and home-bar domain.
It gives recommendation and future customer experiences a stable view of what a specific customer likes, avoids, and currently has available.

---

## Purpose

The Customer Profile exists so AlCopilot can treat customer taste and available ingredients as durable customer-owned state rather than temporary recommendation inputs.
It provides the canonical customer snapshot used when personalizing recommendation and profile-editing experiences.

---

## Core Concepts

- **Customer Profile** — The durable customer-owned record of ingredient preferences and current home-bar state.
- **Favorite Ingredient** — An ingredient the customer actively enjoys and would generally like to see represented in suggestions.
- **Disliked Ingredient** — An ingredient the customer would prefer to avoid, but that does not automatically disqualify a drink.
- **Prohibited Ingredient** — An ingredient the customer must not be recommended.
- **Owned Ingredient** — An ingredient the customer currently has available in their home bar.
- **Home Bar** — The customer's current ingredient inventory as used for make-now versus restock-oriented recommendation framing.

---

## Relationships

- A customer profile belongs to one authenticated customer identity.
- Ingredient preference sets and owned-ingredient state are different views of the same customer's relationship to catalog ingredients.
- Favorite, disliked, and prohibited ingredients express taste and safety signals.
- Owned ingredients express availability rather than taste.
- The profile does not define drinks or recommendation outcomes by itself.

---

## Actors And Uses

- **Customers** maintain their favorite, disliked, prohibited, and owned ingredient sets.
- **Recommendation flows** use the customer profile as the stable personalization snapshot before building recommendation candidates.
- **Future customer experiences** may reuse the same profile to prefill preferences, explain suggestions, or support profile editing without redefining the customer vocabulary.

---

## Business Vocabulary

- Customer profile state is customer-owned and scoped to the authenticated customer identity.
- Favorite ingredients are positive preference signals.
- Disliked ingredients are soft negative preference signals.
- Prohibited ingredients are hard exclusion signals for recommendation preparation.
- Owned ingredients describe what the customer can make with now, not what they necessarily prefer.
- An empty customer profile is still a valid profile snapshot and means the customer has not yet expressed preferences or inventory.

---

## Ownership Boundaries

- The Customer Profile module **owns** customer ingredient preferences and home-bar state.
- The Customer Profile module **does not own** the meaning of drink or ingredient reference data.
- The Customer Profile module **does not own** recommendation sessions, recommendation wording, or final recommendation outcomes.
- Other modules should treat the customer profile as the source of truth for customer-owned ingredient-preference and inventory state.

---

## OpenSpec Note

Use this document to understand customer-profile language, concepts, and ownership before writing or reviewing OpenSpec changes.
Use OpenSpec specs to define the observable behavior a feature must support.
