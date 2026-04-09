# AlCopilot — Architecture

## Overview

AlCopilot is an AI-powered drinks suggestion platform that recommends drinks based on mood, preferences, and available options. The name plays on "Al" (a friendly bartender) and "Copilot" (an AI assistant) — approachable, memorable, and clearly AI-related. Alternatives like "Alcopilot" (reads too much like "alcohol-pilot") and "A1Copilot" (loses the bartender personality) were considered and rejected.

---

## Architecture Style: Modular Monolith

AlCopilot is a **modular monolith**, not microservices.

Microservices-first is almost never warranted at the start of a project. A modular monolith gives clear bounded contexts that can be extracted into separate services later if independent scaling is needed, while keeping deployment, debugging, and in-process communication simple. There is no network overhead between modules.

The system is structured as a single deployable unit with internal boundaries. Each module owns its own EF Core `DbContext` and database schema, enforcing separation without requiring separate processes or infrastructure.

---

## Modules (Bounded Contexts)

| Module             | Responsibility                                                                                                                               |
| ------------------ | -------------------------------------------------------------------------------------------------------------------------------------------- |
| **Identity**       | OIDC integration with Keycloak, session management, user profiles, preference history                                                        |
| **Catalog**        | Drink database, ingredients, categories — CRUD and search                                                                                    |
| **Recommendation** | LLM integration layer: mood → prompt → suggestions. Core domain with AI patterns (prompt engineering, streaming responses, RAG over catalog) |
| **Social**         | Ratings, comments, sharing                                                                                                                   |
| **Inventory**      | "What's in my bar?" — personal bar inventory tracking                                                                                        |

Each module has its own EF Core `DbContext` and Postgres schema, keeping data boundaries clean even within the shared database.

---

## Technology Stack

| Layer                   | Choice                                                                               | Why                                                                                                                                                                                                                                                                                       |
| ----------------------- | ------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Runtime**             | .NET + Aspire                                                                        | Aspire provides distributed app orchestration and observability (dashboard, OpenTelemetry) without the complexity of Kubernetes in development                                                                                                                                            |
| **Data**                | EF Core + Postgres                                                                   | Battle-tested ORM and database combination. Marten was considered but rejected — too opinionated and introduces too much magic via event sourcing                                                                                                                                         |
| **In-process mediator** | Mediator (by Martin Othamar)                                                         | Source-generated, MIT licensed. MediatR went commercial in v13+ (same author as MassTransit). Mediator has a nearly identical API, better performance (no reflection), and zero licensing concerns                                                                                        |
| **Async messaging**     | Deferred: Rebus                                                                      | Preferred future transport abstraction if durable async messaging becomes necessary. Not currently implemented because there is no approved use case yet                                                                                                                                  |
| **Identity Provider**   | Keycloak                                                                             | Open-source, runs locally in Docker via Aspire, supports OIDC/OAuth 2.0. Avoids Entra External ID (no local dev story) and Authentik (complex setup). The Host handles OIDC code flow and issues cookies — the SPA never sees tokens                                                      |
| **Frontend**            | React + Vite + TanStack Query + TanStack Router + shadcn/ui + Tailwind CSS + Zustand | Modern, low-magic frontend stack recorded in [ADR 0003](adr/0003-frontend-stack.md). TanStack Query handles server state, TanStack Router provides type-safe routing, Zustand covers the small amount of client-only state, and shadcn/ui plus Tailwind CSS support composable UI work    |
| **CI/CD**               | GitHub Actions + GHCR + Release Please                                               | Full pipeline from day one: build, test, coverage gate, Docker image push, automated versioning and changelog                                                                                                                                                                             |
| **Infrastructure**      | AKS + Flux + Blob Storage + Postgres Flexible Server                                 | GitOps with Flux for deployment. Azure Service Bus remains a deferred option if async messaging is introduced later                                                                                                                                                                       |
| **Gateway**             | Self-hosted Envoy Gateway                                                            | Azure Application Gateway has a ~$130/month floor plus capacity units even at low traffic. Envoy Gateway is the Kubernetes Gateway API reference implementation, runs on existing AKS nodes at near-zero marginal cost, and uses standard `Gateway`/`HTTPRoute` resources for portability |
| **Reverse proxy**       | YARP (when needed)                                                                   | Built into the Host when a module is extracted into a separate service. Frontend remains unaware — same origin, same cookie, same routes. YARP forwards requests and attaches bearer tokens to proxied calls                                                                              |

---

## Decision Records: Why Not X

### Not Wolverine

Despite having a built-in outbox and durability features, Wolverine creates deep framework coupling. If Wolverine goes commercial (as MassTransit did), you lose the mediator, outbox, transport, deduplication, and routing in one move — the entire application nervous system. The preferred future direction is Mediator + Rebus, recorded in `docs/adr/0001-durable-intermodule-messaging.md`.

### Not MassTransit

Commercial license.

### Not MediatR

Commercial license (v13+).

### Not Brighter

Brighter's outbox design assumes one `IAmATransactionConnectionProvider` per host, meaning one database connection/transaction context per outbox. This is a microservice assumption (one service = one database = one outbox). In a modular monolith with schema-per-module, you would need separate outbox configurations per module, each with its own sweeper — working against the framework rather than with it.

### Not Marten

Too opinionated. Introduces event sourcing and document store patterns as defaults, adding significant magic for a project where a straightforward relational model is appropriate.

### Not Azure Application Gateway

~$130/month minimum floor plus per-capacity-unit charges, even at low traffic. Unjustifiable for a project of this scale.

### Not Entra External ID

No local development story. Requires a live Azure tenant for every auth flow test, slowing inner-loop development.

### Not Authentik

Significantly more complex setup than Keycloak for equivalent OIDC functionality. The additional features (proxied auth, outpost architecture) are not needed here.

---

## Messaging Architecture

### In-process (always)

**Mediator** handles all in-process command/query dispatch. All module-to-module communication within the monolith goes through Mediator. This is synchronous and in-memory — no network, no serialization, no transport concerns.

### Out-of-process (when needed)

Durable out-of-process messaging is deferred until a concrete cross-module or extracted-service use case exists.
The preferred future direction is recorded in [ADR 0001](adr/0001-durable-intermodule-messaging.md).
Until then, the runtime stays focused on in-process coordination through Mediator.

## Deferred Decisions

The following ADRs are intentionally visible here for discoverability, but they are not implemented behavior:

- [ADR 0001: Durable Intermodule Messaging](adr/0001-durable-intermodule-messaging.md) — deferred until a real async publisher/consumer use case or module extraction requires durable out-of-process communication.

---

## Authentication & BFF Pattern

### Overview

`AlCopilot.Host` acts as a **BFF (Backend-For-Frontend)**. It handles OIDC code flow, manages sessions, and issues `HttpOnly; Secure; SameSite=Strict` cookies. The SPA never sees access tokens, refresh tokens, or id tokens — they are stored server-side.

This follows the [OAuth 2.0 for Browser-Based Applications](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-browser-based-apps) recommendation.

### Identity Provider: Keycloak

Keycloak runs locally as a container orchestrated by Aspire's AppHost. In production, it runs as a dedicated service in AKS. Keycloak provides:

- OIDC / OAuth 2.0 authorization code flow (with PKCE)
- User registration and management
- Social login providers (future)
- Admin console for realm/client configuration

### Auth Flow

1. SPA navigates to a protected route
2. Host initiates OIDC authorization code flow → redirects to Keycloak
3. User authenticates at Keycloak → redirected back to Host callback
4. Host exchanges code for tokens, stores them server-side (in-memory or Redis)
5. Host sets `HttpOnly; Secure; SameSite=Strict` session cookie
6. All subsequent SPA requests carry the cookie — no tokens in the browser

### Envoy Gateway Role

Envoy Gateway is the **external ingress gateway** only — TLS termination, rate limiting, and routing via `Gateway`/`HTTPRoute` resources. It does **not** handle OIDC flows, cookies, or session management. That is application-layer logic in the Host.

```
Internet ──TLS──▶ Envoy Gateway ──▶ AlCopilot.Host (BFF)
```

### Module Extraction via YARP

When a module is extracted into a separate service, **YARP** reverse proxy is added to the Host. The frontend changes nothing — same cookie, same origin, same routes.

**Before extraction (monolith):**

```
Browser ──cookie──▶ AlCopilot.Host (BFF + all modules in-process)
```

**After extracting a module:**

```
Browser ──cookie──▶ AlCopilot.Host (BFF + remaining modules)
                        │
                        ├── YARP ──▶ Extracted.Service
                        ├── Catalog (in-process)
                        └── ...
```

The Host attaches bearer tokens to proxied requests via YARP request transforms. The extracted service validates JWTs. The browser is unaware of the split.

---

## Infrastructure

### AKS

Managed control plane (no cost regardless of cluster SKU tier), Standard_B-series nodes for cost efficiency. The Free cluster SKU is used, which carries a limited SLA — acceptable for this project's scale.

### Flux

Installed as a native AKS extension. Watches `deploy/flux/` in this repository and reconciles cluster state automatically.

### CI/CD Flow

1. Push to `main`
2. GitHub Actions builds and tests the application
3. Release Please manages versioning and generates changelogs
4. Container image is published to GHCR via `dotnet publish /t:PublishContainer`
5. Flux detects the new image and reconciles the AKS cluster

### Local Development

Aspire AppHost orchestrates the full local environment, including Keycloak and a Postgres instance. No Kubernetes required for development.

---

## Project Structure

```
alcopilot/
├── .codex/                        # Codex skills and local agent workflow assets
│   └── skills/                   # ADR and OpenSpec skills
├── .github/
│   ├── workflows/                # CI/CD pipelines
│   ├── hooks/                    # Agent and GitHub-side enforcement scripts
│   └── copilot-instructions.md   # Thin compatibility note for Copilot
├── deploy/                       # Infrastructure and deployment
│   ├── helm/                     # Helm charts (future)
│   ├── flux/                     # Flux GitOps manifests (future)
│   └── terraform/                # IaC (future)
├── docs/                         # Architecture and documentation
├── server/                       # .NET backend
│   ├── src/
│   │   ├── AlCopilot.AppHost/    # Aspire orchestrator
│   │   ├── AlCopilot.ServiceDefaults/
│   │   ├── AlCopilot.Host/        # ASP.NET Core host + BFF (composes modules)
│   │   ├── AlCopilot.Shared/     # Cross-cutting concerns
│   │   ├── AlCopilot.Catalog/    # Module
│   │   ├── AlCopilot.Catalog.Contracts/ # Module contracts
│   │   ├── AlCopilot.Identity/   # Module
│   │   ├── AlCopilot.Recommendation/ # Module
│   │   ├── AlCopilot.Social/     # Module
│   │   └── AlCopilot.Inventory/  # Module
│   ├── tests/                    # Per-module test projects
│   ├── Directory.Build.props     # Shared MSBuild properties + Version
│   ├── Directory.Packages.props  # Central Package Management
│   └── AlCopilot.slnx            # Solution file
└── web/                          # Frontend (pnpm workspace)
    ├── apps/
    │   └── alcopilot-portal/     # Main user-facing app (Vite + React + TS)
    └── packages/                 # Shared packages (future)
```

---

## Domain-Driven Design

Each module follows DDD principles. Domain logic lives in aggregates and domain services — never in handlers or infrastructure code.

### Aggregates

An **aggregate root** is the consistency boundary. Repositories load and persist the complete aggregate atomically. Child entities (e.g., `RecipeEntry` within `Drink`) are part of the aggregate and never accessed independently. Cross-aggregate references use IDs only.

Base types live in `AlCopilot.Shared`:

- `AggregateRoot<TId>` — base class with `Id`, `DomainEvents` list, protected `Raise(IDomainEvent)` method
- `Entity<TId>` — base class for child entities within an aggregate
- `ValueObject<T>` — base class with `Value` property, implicit conversion to `T`, equality by value

### Value Objects

Prefer value objects for any property with validation rules (length limits, format, non-empty). Value objects validate in their constructor/factory method — invalid values are structurally impossible. EF Core maps them via `HasConversion(v => v.Value, raw => TypeName.Create(raw))`.

### Repositories & Unit of Work

`IRepository<TRoot, TId>` provides `GetByIdAsync`, `Add`, `Remove`. One repository per aggregate root. Implementations are `internal sealed` classes wrapping the module's `DbContext`.

`IUnitOfWork` provides `SaveChangesAsync`. The module's `DbContext` implements `IUnitOfWork`. Handlers call it once at the end — no mid-handler saves.

### Domain Events & Outbox

Aggregates raise domain events via `Raise(new SomeEvent(...))`. A `SaveChangesInterceptor` implements a **dispatch-before-commit loop**:

1. Collect events from tracked aggregates, clear their lists
2. Persist `DomainEventRecord` rows to the module's `domain_events` table
3. Dispatch each event to registered `IDomainEventHandler<T>` implementations (in-process, same scope)
4. Repeat (handlers may cause new events) — max depth 5, throw if exceeded
5. Final `SaveChanges` commits everything atomically (state changes + event records)

`DomainEventRecord` schema per module: `Id (long)`, `AggregateId (Guid)`, `AggregateType`, `EventType`, `Payload (jsonb)`, `OccurredAtUtc`. `EventType` stores a logical name from `[DomainEventName]` (e.g., `drink-catalog.drink-created.v1`), decoupled from CLR type names. Records serve as an audit log and event replay source. Indexes: `(AggregateId, Id)` for per-aggregate streams, `(OccurredAtUtc)` for time-range queries.

#### Cross-Module Communication Patterns

Two complementary patterns cover all cross-module interaction:

| Pattern                                    | Use case                                          | Transaction                                  | Coupling                           |
| ------------------------------------------ | ------------------------------------------------- | -------------------------------------------- | ---------------------------------- |
| **Mediator commands**                      | Orchestration — "do this, tell me if it worked"   | Same transaction (rolls back together)       | Caller knows the Contracts command |
| **Integration events + outbox (deferred)** | Choreography — "this happened, react if you want" | Separate transactions (eventual consistency) | Publisher doesn't know consumers   |

**Mediator commands** (via Contracts): Module A sends a command/query defined in Module B's Contracts project. Synchronous, in-process, request/response. Use when the caller needs a result or needs the operation to succeed atomically.

**Integration events** are a deferred direction, not a current runtime capability.
If a real async use case appears, revisit the design in [ADR 0001](adr/0001-durable-intermodule-messaging.md) and approve a new change before implementation.

### Why Not Event Sourcing

Event sourcing (Marten, EventStoreDB) introduces significant complexity for a project where a straightforward relational model is appropriate. Domain events are persisted for replay and outbox purposes, but the source of truth is the aggregate state in the relational tables — not the event stream.

---

## Testing Strategy

### Backend (.NET)

| Type             | Purpose                                          | Tools                                                                | Project Pattern                                            | When     |
| ---------------- | ------------------------------------------------ | -------------------------------------------------------------------- | ---------------------------------------------------------- | -------- |
| **Unit**         | Pure logic, no I/O, isolated                     | xUnit + Shouldly + NSubstitute                                       | `AlCopilot.{Module}.Tests`                                 | Every PR |
| **Integration**  | Real DB, real HTTP, cross-layer                  | xUnit + Shouldly + WebApplicationFactory + TestContainers (Postgres) | `AlCopilot.{Module}.Tests` (same project, trait-separated) | Every PR |
| **Architecture** | Module boundaries, dependency rules, conventions | xUnit + NetArchTest.eNhanced                                         | `AlCopilot.Architecture.Tests`                             | Every PR |

**Unit and integration tests live in the same project** per module (`AlCopilot.{Module}.Tests`). Integration tests are marked with `[Trait("Category", "Integration")]` so they can be filtered if the suite grows too slow. Currently, all tests run on every PR — TestContainers is fast enough.

**Architecture tests** enforce modular monolith boundaries: modules only reference Contracts, handlers are sealed, entities don't leak across schemas, naming conventions are followed.

**Assertion library: Shouldly** — FluentAssertions is commercial. Shouldly is MIT, readable, and has good error messages.

**Mocking: NSubstitute** — Moq had a telemetry controversy (SponsorLink). NSubstitute is MIT, clean API, no controversy.

**No in-memory database** — EF Core's in-memory provider doesn't support transactions, relational constraints, or Postgres-specific features. SQLite in-memory diverges too. TestContainers with real Postgres catches real issues.

### Frontend (Web)

| Type          | Purpose                                     | Tools                          | When                  |
| ------------- | ------------------------------------------- | ------------------------------ | --------------------- |
| **Component** | Render + interaction testing                | Vitest + React Testing Library | Every PR              |
| **E2E**       | Full browser flows against real environment | Playwright                     | Nightly / pre-release |

**Vitest** is native to Vite — same config, same transform pipeline, fast watch mode. Component tests verify rendered output and user interactions via React Testing Library.

**Playwright** runs against a deployed staging environment. This is the "nightly build" — browser startup, real services, real network. Too slow and brittle for every PR.

### Future Stages

| Type                 | Purpose                                 | Tools                      | When to Add                                       |
| -------------------- | --------------------------------------- | -------------------------- | ------------------------------------------------- |
| **Load testing**     | Performance under sustained traffic     | k6 or NBomber              | When deployed to staging with representative data |
| **Mutation testing** | Verify test quality by injecting faults | Stryker.NET / Stryker (JS) | When test suite is mature and coverage is high    |

### Not Used

- **Pact (contract testing)**: Cross-module contracts are shared via `.Contracts` projects — no HTTP boundary to test
- **Snapshot tests**: Brittle and low-value for this kind of application
- **FluentAssertions**: Commercial license

---

## AI Tooling

AI agent configuration follows a portable hierarchy:

- **AGENTS.md hierarchy**: Root [AGENTS.md](../AGENTS.md) indexes area-specific conventions; `AGENTS.md` files in `server/`, `web/`, `deploy/`, `docs/`, `.github/workflows/` contain per-area conventions
- **Codex skills**: `.codex/skills/` is the primary home for local repeatable workflows such as ADR management and OpenSpec helpers
- **GitHub compatibility**: `.github/` contains workflows, hooks, and thin compatibility files rather than local custom skill, prompt, or agent definitions
- **Hooks**: `.github/hooks/` enforce security and audit rules for supported agent workflows
