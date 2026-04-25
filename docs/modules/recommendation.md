# Recommendation — Domain Overview

The Recommendation module is AlCopilot's conversational suggestion and recommendation-session domain.
It turns customer requests, customer-profile signals, and drinks-catalog knowledge into persisted recommendation conversations and grouped recommendation outcomes.

---

## Purpose

The Recommendation module exists so AlCopilot can own recommendation conversations and recommendation outcomes separately from both catalog curation and customer profile storage.
It provides the durable business record of what the customer asked, how the assistant responded, and which recommendation groupings were presented.

---

## Core Concepts

- **Recommendation Session** — A persisted customer conversation about what to drink or how to make something.
- **Recommendation Turn** — A single user or assistant message within a recommendation session.
- **Customer Request Intent** — The interpreted shape of the customer's request, currently centered on recommendation requests versus drink-details requests, with ingredients and descriptive cues carried as request attributes.
- **Recommendation Candidate** — A drink that remains eligible after deterministic recommendation preparation.
- **Recommendation Group** — A customer-facing grouping of recommendation outcomes, currently centered on drinks available now versus drinks better framed for restock.
- **Recommendation Outcome** — The combined result of grouped machine-readable suggestions plus conversational assistant explanation.

---

## Relationships

- A recommendation session belongs to one authenticated customer identity.
- Recommendation sessions use customer-profile state and drinks-catalog data, but do not own either of those reference domains.
- Recommendation candidates are shaped by hard exclusions, soft preference signals, and owned-ingredient availability before conversational explanation.
- Recommendation groups organize the resulting outcomes into practical choices for the customer.
- A recommendation turn may include both conversational prose and structured recommendation-group data.

---

## Actors And Uses

- **Customers** ask for drink suggestions, ingredient-constrained recommendations, or drink details through recommendation chat.
- **The recommendation assistant** returns grouped suggestions and practical conversational guidance.
- **Customer portal experiences** use persisted recommendation sessions so past conversations can be reopened and understood in order.
- **Developers and operators** may inspect recommendation-session and tool-usage history when troubleshooting recommendation behavior in development workflows.

---

## Business Vocabulary

- Recommendation chat is a persisted customer conversation, not a one-off transient prompt exchange.
- Deterministic preparation happens before conversational recommendation wording so hard constraints stay explicit.
- Prohibited ingredients remove drinks from consideration.
- Disliked ingredients lower priority rather than automatically excluding a drink.
- Owned ingredients help distinguish drinks the customer can make now from drinks that are better framed as restock candidates.
- Recommendation groups are stable customer-facing outcome buckets rather than arbitrary assistant formatting.
- Recipe lookup is part of helping the customer understand a specific drink, not a separate catalog-management workflow.

---

## Ownership Boundaries

- The Recommendation module **owns** recommendation sessions, recommendation turns, grouped recommendation outcomes, and recommendation-oriented conversational responses.
- The Recommendation module **does not own** customer taste or home-bar state as durable source data.
- The Recommendation module **does not own** the canonical meaning of drinks, ingredients, tags, or categories.
- Other modules should treat Recommendation as the source of truth for persisted recommendation conversations and recommendation-result history.

---

## OpenSpec Note

Use this document to understand recommendation language, concepts, and ownership before writing or reviewing OpenSpec changes.
Use OpenSpec specs to define the observable behavior a feature must support.
