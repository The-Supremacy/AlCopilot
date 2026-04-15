## Context

The project already has a modular monolith backend, a Host-owned external web boundary, a management portal, and a relational `DrinkCatalog` module.
The customer portal does not exist yet, but the architecture already reserves `web/apps/web-portal` as the user-facing portal boundary.
This change is UI-affecting and therefore uses the customer portal design guide as required context:

- `web/apps/web-portal/DESIGN.md`

The catalog already stores category, description, method, garnish, tags, and recipe entries, which is enough to support a first recommendation loop without semantic retrieval.
The current local auth bootstrap is management-first only, so customer self-signup and customer session boundaries must be introduced as part of this slice.

## Goals / Non-Goals

**Goals:**

- Create a signed-in customer portal foundation that can exercise real recommendation behavior end to end.
- Keep customer and management authentication separate at the Host boundary.
- Introduce two new backend modules, `CustomerProfile` and `Recommendation`, with contracts-based interaction.
- Make hard recommendation constraints deterministic outside the LLM.
- Use the accepted frontend stack directly for the new portal.
- Persist recommendation sessions in a format that can be projected into Semantic Kernel chat history cleanly.

**Non-Goals:**

- Anonymous customer access.
- Customer identity-account administration beyond sign-in, sign-out, and self-registration through Keycloak.
- Vector retrieval, embeddings, or Qdrant-backed ranking in the first slice.
- Catalog-wide customer browsing beyond what recommendation flows require.
- Open-ended agentic tool loops or model-owned writes.
- Quantity-aware or brand-aware home-bar inventory in the first slice.

## Decisions

### Decision 1: Customer authentication is a separate Host auth surface from management

`AlCopilot.Host` will add a dedicated customer authentication configuration, policy, login endpoint, logout endpoint, callback paths, session endpoint, and cookie name.
The customer portal will stay on the main product host while management keeps its dedicated management host.
Keycloak bootstrap will add a dedicated customer client and enable self-registration for the local realm.
Credential, identity, and user-account administration remain in the Keycloak console rather than being reimplemented in the customer portal.

This keeps audience boundaries explicit and allows customer and management auth to evolve independently without frontend token handling.

### Decision 2: The customer slice uses two new modules, not Host-owned orchestration

`CustomerProfile` and `Recommendation` will be introduced as separate backend modules with their own `.Contracts` projects and test projects.
`CustomerProfile` owns customer preference and home-bar state keyed by authenticated user identity.
`Recommendation` owns chat sessions, turns, candidate building, and LLM orchestration.
`Recommendation` depends on `DrinkCatalog.Contracts` and `CustomerProfile.Contracts` only.

This is the cleanest way to establish real cross-module collaboration patterns instead of embedding product logic in the Host.

### Decision 3: `CustomerProfile` models preference and inventory as simple ingredient sets

The first iteration stores four ingredient-ID sets:

- favorites
- dislikes
- prohibited
- owned

The aggregate root is the authenticated customer profile.
Likely value objects include a stable customer identity value object and a preference collection update payload or type-safe ingredient-set wrappers if they improve domain clarity.
No domain events are required by default for the first slice unless a later implementation need appears.

This keeps the profile surface easy to edit, easy to test, and compatible with later richer modeling if product pressure appears.
It also preserves a clear ownership boundary: Keycloak owns identity-account data, while AlCopilot owns recommendation-relevant domain preferences.

### Decision 4: `Recommendation` persists normalized chat and structured outputs, not connector-native blobs

`Recommendation` owns a `ChatSession` aggregate root.
It persists ordered turns with normalized role, textual content, structured recommendation payloads, and limited read-only tool-call metadata.
The implementation reconstructs Semantic Kernel `ChatHistory` at runtime from stored turns instead of serializing Semantic Kernel or Ollama-specific objects directly.

Likely domain model pieces include:

- aggregate root: `ChatSession`
- child entities: `ChatTurn`, `ChatToolInvocation`
- value objects: customer message content, assistant reply summary, tool invocation identifiers, and structured recommendation-group payload components as needed
- domain events are optional and should only be added if same-module reactions prove useful

This keeps storage stable while preserving compatibility with Semantic Kernel chat and function-calling APIs.

### Decision 5: Candidate building is deterministic before LLM ranking

The backend builds recommendation candidates before invoking the model.
The deterministic pipeline will:

- remove drinks containing prohibited ingredients
- compute `make now` versus `buy next` based on owned ingredients
- apply bounded boosts for favorite ingredients
- apply soft penalties for disliked ingredients

The LLM then ranks and explains a bounded candidate set rather than deciding safety-critical filters itself.
Assistant responses return structured recommendation groups plus conversational prose so the UI can render stable recommendation surfaces.

This aligns with ADR 0006 and keeps the first recommendation milestone easier to debug than a model-led filtering approach.

Scoring notes for the first implementation:

- prohibited ingredients are a hard exclusion before scoring
- `make now` starts from a higher base score than `buy next`
- favorite-ingredient matches add a small bounded positive boost
- each missing ingredient reduces the score for near-miss drinks
- disliked ingredients apply a soft penalty instead of exclusion

The exact constants may evolve, but the scoring dimensions above are the durable documented rule set for this slice and should remain visible in code and artifacts rather than becoming hidden heuristics.

### Decision 6: Semantic Kernel uses limited read-only tool calling only

The first Semantic Kernel tool surface is read-only and intentionally small.
Allowed tool categories include:

- customer profile snapshot lookup
- candidate lookup or refresh
- ingredient-gap analysis

The model does not write profile state, does not mutate session records directly, and does not own orchestration loops.
The application layer remains responsible for persistence, authorization, candidate preparation, and final response shaping.

This gives the team function-calling experience without turning the first customer release into an unbounded agent workflow.

Implementation note:

- tool support may stay intentionally simple at first as long as the codebase contains a real working example of read-only tool registration and metadata flow
- deterministic filtering, grouping, and persistence remain application-owned even when tool examples are simplified

### Decision 7: The web portal is a chat-first app that reuses shared frontend patterns

`web/apps/web-portal` will use the same approved frontend stack as the management portal:

- React + TypeScript
- TanStack Router
- TanStack Query
- Tailwind CSS
- shadcn/ui

The portal shell is chat-first with a persistent session-history rail on desktop and a drawer-based secondary navigation model on smaller screens.
The app should use a dedicated customer API client package and a portal-scoped session query pattern similar to the management portal, while keeping customer-specific routes and UI invariants in the new portal.

This preserves cross-portal consistency without forcing the customer portal into the management portal's operational shell model.

## Risks / Trade-offs

- [Two new modules add setup and architecture-test overhead] -> Accept the up-front cost because clear boundaries are a core goal of this change.
- [Customer self-signup changes local auth bootstrap assumptions] -> Keep the new Keycloak client and realm changes explicit and test the customer session flow through the Host.
- [Structured recommendation outputs can drift from free-form chat tone] -> Persist both machine-readable groups and conversational prose so the UI stays stable without flattening the experience.
- [Limited tool calling still adds persistence and debugging complexity] -> Restrict tools to read-only helpers and keep orchestration in application code.
- [Database schema growth across two new modules raises migration complexity] -> Add separate module schemas and keep rollback focused on disabling the new portal and customer route surface if rollout problems appear.

## Migration Plan

1. Add the new customer portal design guide and artifact set so UI and architecture direction are aligned before implementation.
2. Add the customer Keycloak client and Host auth/session configuration.
3. Introduce the `CustomerProfile` module, its contracts, schema, migrations, and tests.
4. Introduce the `Recommendation` module, its contracts, schema, migrations, Semantic Kernel boundary, and tests.
5. Add customer-facing Host route groups and the new `web-portal` app.
6. Verify local login, profile persistence, session history, deterministic filtering, and bounded recommendation behavior before considering retrieval augmentation.

Rollback strategy:

- Roll back the web portal deployment and Host customer route exposure if the first rollout blocks customer access unexpectedly.
- Disable or remove the customer auth configuration while keeping management auth untouched.
- Roll back the new module migrations in their owning schemas if database rollback is required.
- Leave deferred retrieval work out of scope so rollback only concerns the customer portal foundations and not future vector infrastructure.

## Open Questions

- Whether the customer portal should later move from the main product host to a dedicated customer host remains open, but not required for the first slice.
- Whether recommendation-session audit or analytics should be explicit records in addition to chat-turn persistence remains future scope.
- Whether disliked ingredients should later support more nuance than a single soft-penalty tier remains future product scope.
