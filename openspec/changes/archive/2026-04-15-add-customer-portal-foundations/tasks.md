## 1. Discovery And Design Alignment

- [x] 1.1 Confirm impacted portal design guides are aligned before apply (`web/apps/web-portal/DESIGN.md`).
- [x] 1.2 Add accepted ADRs for customer auth separation and the `CustomerProfile` plus `Recommendation` module split.
- [x] 1.3 Sync active architecture guidance docs with the accepted ADR decisions for customer portal direction.

## 2. Infrastructure And Host Customer Authentication

- [x] 2.1 Add customer Keycloak client and self-registration support to local AppHost bootstrap and runtime configuration.
- [x] 2.2 Add customer authentication options, cookie scheme, login/logout endpoints, callback paths, session endpoint, and customer access policy in `AlCopilot.Host`.
- [x] 2.3 Add Host tests for anonymous, authorized, and unauthorized customer portal session and API behavior.

## 3. Backend: CustomerProfile Module

- [x] 3.1 Create `AlCopilot.CustomerProfile` and `AlCopilot.CustomerProfile.Contracts` projects with module registration, schema ownership, and contracts-only cross-module boundaries.
- [x] 3.2 Implement the customer profile aggregate, value objects, repositories, query services, commands, queries, endpoints, and migrations for favorite, disliked, prohibited, and owned ingredient sets.
- [x] 3.3 Add architecture tests for the new module boundaries and contracts purity rules involving `CustomerProfile`.
- [x] 3.4 Add unit and integration tests for customer profile handlers, persistence, and authenticated profile scoping.

## 4. Backend: Recommendation Module

- [x] 4.1 Create `AlCopilot.Recommendation` and `AlCopilot.Recommendation.Contracts` projects with contracts-based dependencies on `DrinkCatalog` and `CustomerProfile`.
- [x] 4.2 Add Semantic Kernel dependencies and implement the LLM boundary with limited read-only tool calling only.
- [x] 4.3 Implement chat-session persistence, ordered turn storage, structured recommendation payloads, deterministic candidate building, and module-owned endpoints.
- [x] 4.4 Add architecture tests for the new module boundaries and contracts purity rules involving `Recommendation`.
- [x] 4.5 Add unit and integration tests for recommendation handlers, candidate filtering, session persistence, and structured response behavior.

## 5. Frontend: Web Portal

- [x] 5.1 Scaffold `web/apps/web-portal` on the approved React, TanStack, Tailwind CSS, and shadcn/ui stack.
- [x] 5.2 Add a customer API client package and customer session hooks aligned with the Host customer auth endpoints.
- [x] 5.3 Implement the chat-first shell with session-history rail, sign-in-required state, and customer account affordances.
- [x] 5.4 Implement `Chat`, `My Bar`, `Preferences`, and `History` flows against the new customer APIs.
- [x] 5.5 Verify the web portal remains aligned with Tailwind CSS and shadcn/ui conventions and add frontend tests for auth gating, structured recommendations, and profile-management flows.
