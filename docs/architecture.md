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

| Module | Responsibility |
|---|---|
| **Identity** | Authentication, user profiles, preference history |
| **Catalog** | Drink database, ingredients, categories — CRUD and search |
| **Recommendation** | LLM integration layer: mood → prompt → suggestions. Core domain with AI patterns (prompt engineering, streaming responses, RAG over catalog) |
| **Social** | Ratings, comments, sharing |
| **Inventory** | "What's in my bar?" — personal bar inventory tracking |

Each module has its own EF Core `DbContext` and Postgres schema, keeping data boundaries clean even within the shared database.

---

## Technology Stack

| Layer | Choice | Why |
|---|---|---|
| **Runtime** | .NET + Aspire | Aspire provides distributed app orchestration and observability (dashboard, OpenTelemetry) without the complexity of Kubernetes in development |
| **Data** | EF Core + Postgres | Battle-tested ORM and database combination. Marten was considered but rejected — too opinionated and introduces too much magic via event sourcing |
| **In-process mediator** | Mediator (by Martin Othamar) | Source-generated, MIT licensed. MediatR went commercial in v13+ (same author as MassTransit). Mediator has a nearly identical API, better performance (no reflection), and zero licensing concerns |
| **Async messaging** | Rebus | Thin transport abstraction, MIT licensed, at-least-once delivery. Not added initially — there is no external messaging target in a monolith. Introduced when extracting services or when durable async processing is needed |
| **Frontend** | React + Vite + Tanstack Query + shadcn + Zustand | Modern, low-magic frontend stack. Tanstack Query for server state, Zustand for the small amount of client-only state |
| **CI/CD** | GitHub Actions + GHCR + Release Please | Full pipeline from day one: build, test, coverage gate, Docker image push, automated versioning and changelog |
| **Infrastructure** | AKS + Flux + Azure Service Bus + Blob Storage + Postgres Flexible Server | GitOps with Flux for deployment. Azure Service Bus (with local emulator for development) when async messaging is needed |
| **Gateway** | Self-hosted Envoy Gateway | Azure Application Gateway has a ~$130/month floor plus capacity units even at low traffic. Envoy Gateway is the Kubernetes Gateway API reference implementation, runs on existing AKS nodes at near-zero marginal cost, and uses standard `Gateway`/`HTTPRoute` resources for portability |

---

## Decision Records: Why Not X

### Not Wolverine
Despite having a built-in outbox and durability features, Wolverine creates deep framework coupling. If Wolverine goes commercial (as MassTransit did), you lose the mediator, outbox, transport, deduplication, and routing in one move — the entire application nervous system. With Mediator + Rebus, we own dispatch, outbox, and deduplication, and only rent the transport abstraction.

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

---

## Messaging Architecture

### In-process (always)

**Mediator** handles all in-process command/query dispatch. All module-to-module communication within the monolith goes through Mediator. This is synchronous and in-memory — no network, no serialization, no transport concerns.

### Out-of-process (when needed)

**Rebus** handles async messaging via Azure Service Bus when out-of-process communication is required. Rebus is not introduced initially because there is no external messaging target in a pure monolith.

Key properties of Rebus in this stack:

- **Competing consumers**: Multiple instances on the same input queue compete for messages. One instance processes each message.
- **At-least-once delivery**: Rebus guarantees delivery but not exactly-once. Handlers must be idempotent.
- **Transport agnostic**: Rebus does not promote or require handlers to be in a specific host. For a monolith, handlers run in the same process as the API.
- **Message format**: Rebus does not have its own wire protocol. Messages are clean JSON. Rebus metadata (`rbs2-*`) travels in transport headers only — not wrapped in the message payload — making messages portable to non-.NET consumers with no Rebus-specific deserialization required.
- **Mixed transports**: Rebus supports configuring different transports for different endpoints. In-process dispatch goes through Mediator; external async messages go through Service Bus. The two are not mixed within Rebus itself — Mediator owns in-process, Rebus owns out-of-process.
- **Interoperability**: Exporting to a non-Rebus system requires only that the consumer can read standard JSON and interpret the transport headers. No Rebus client is required on the receiving end. This contrasts with Wolverine, which has its own envelope protocol and requires Wolverine-specific handling for interoperability.

### Message type naming

A custom `IMessageTypeNameConvention` is used to produce human-readable, version-stable message type names — for example `catalog.drink-added.v1` — instead of .NET assembly-qualified names. This ensures messages are readable by non-.NET consumers and remain stable across refactoring.

### Outbox pattern

The outbox will be implemented manually when needed — approximately 150 lines: an `OutboxMessage` table plus a `BackgroundService` poller. This approach gives full control over which `DbContext`/transaction each outbox write participates in (one per module), and is more educational than depending on a framework implementation.

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
4. Docker image is pushed to GHCR
5. Flux detects the new image and reconciles the AKS cluster

### Local Development
Aspire AppHost orchestrates the full local environment, including the Azure Service Bus emulator and a Postgres instance. No Kubernetes required for development.

---

## Project Structure

```
alcopilot/
├── .github/workflows/            # CI/CD pipelines
├── deploy/flux/                  # Flux kustomizations (base + overlays)
├── docs/                         # Architecture and documentation
├── src/
│   ├── AlCopilot.AppHost/        # Aspire orchestrator
│   ├── AlCopilot.ServiceDefaults/
│   ├── AlCopilot.Api/            # ASP.NET Core host (composes modules)
│   ├── AlCopilot.Shared/         # Cross-cutting concerns
│   ├── AlCopilot.Identity/       # Module
│   ├── AlCopilot.Catalog/        # Module
│   ├── AlCopilot.Recommendation/ # Module
│   ├── AlCopilot.Social/         # Module
│   ├── AlCopilot.Inventory/      # Module
│   └── AlCopilot.Frontend/       # React app
├── tests/                        # Per-module test projects
└── AlCopilot.sln
```
