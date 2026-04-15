## Why

The project now has a management portal and a curated catalog, but it still lacks the customer-facing portal and backend capabilities needed to debug recommendation behavior end to end.
We need a first signed-in customer slice now so bar inventory, taste preferences, and recommendation chat can be exercised with real persisted state before retrieval augmentation is added.

## What Changes

- Add a new customer-facing `web-portal` app that uses the accepted React, TanStack, Tailwind CSS, and shadcn/ui stack and follows a new portal design guide at `web/apps/web-portal/DESIGN.md`.
- Add customer authentication through `AlCopilot.Host` using Keycloak, a dedicated customer client, and a dedicated customer cookie-backed session flow separate from management.
- Keep identity account administration in the Keycloak console while keeping customer ingredient preferences and home-bar state in AlCopilot-owned domain storage.
- Add a new `CustomerProfile` module that stores favorite, disliked, prohibited, and owned ingredients for the authenticated customer.
- Add a new `Recommendation` module that owns recommendation chat sessions, turns, deterministic candidate building, and Semantic Kernel orchestration.
- Keep hard recommendation constraints deterministic outside the LLM, including prohibited-ingredient exclusion and `make now` versus `buy next` grouping.
- Use Semantic Kernel with a limited read-only tool surface and machine-readable recommendation payloads plus conversational prose.
- Keep vector retrieval, embeddings, and Qdrant intentionally out of the first implementation slice while preserving them as the next direction.
- Use persistent portal design guides as frontend design source of truth for this change:
  - `web/apps/web-portal/DESIGN.md`

## Capabilities

### New Capabilities

- `customer-authentication`: signed-in customer access, self-service onboarding, and customer-specific Host session behavior for the web portal.
- `customer-profile`: customer-owned preference and home-bar management for ingredients.
- `recommendation-chat`: structured conversational recommendation sessions that use deterministic candidate building plus LLM ranking and explanation.

### Modified Capabilities

- None.

## Impact

- Affected code spans `server/src/AlCopilot.Host`, `server/src/AlCopilot.AppHost`, new backend modules under `server/src/Modules/`, new backend test projects, and the new portal app under `web/apps/web-portal`.
- Impacted portals for UI-affecting work:
  - `web-portal`
- The change introduces new architecture decisions around separate customer auth and a two-module customer recommendation boundary.
- The change adds .NET Semantic Kernel and a local LLM integration boundary to the implementation plan.
- The change keeps Qdrant and embeddings deferred so the first recommendation milestone can focus on deterministic candidate building and customer-state flows.
