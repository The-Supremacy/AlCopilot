# Server Architecture

## Purpose

This document is the detailed backend and host architecture guide for AlCopilot.
Use it for human-readable backend architecture context and current runtime decisions.

---

## Platform Shape

The backend is a modular monolith built on .NET 10 and Aspire.
Each module owns its own EF Core `DbContext`, PostgreSQL schema, and internal implementation.
The Host composes modules into the current web-facing application boundary.

---

## Core Stack

| Layer                     | Choice               | Notes                                         |
| ------------------------- | -------------------- | --------------------------------------------- |
| Runtime                   | .NET 10 + Aspire     | Local orchestration and observability         |
| Data                      | EF Core + PostgreSQL | One schema and `DbContext` per module         |
| In-process coordination   | Mediator             | Source-generated and MIT licensed             |
| Host boundary             | `AlCopilot.Host`     | Current BFF and external integration boundary |
| Future extraction support | YARP when needed     | Added only when modules are extracted         |

---

## Modular Monolith Rules

- Modules reference other modules through `.Contracts` projects only.
- No cross-module EF entity references are allowed.
- Module entry points are `Add*Module(this IServiceCollection)` extensions.
- Cross-module orchestration uses contracts and Mediator rather than ad hoc service coupling.

---

## Domain Model Defaults

AlCopilot follows the accepted DDD defaults recorded in [ADR 0002](../adr/0002-domain-driven-design-patterns.md).
Domain logic belongs in aggregates and domain services rather than handlers.
The current backend command/query split follows [ADR 0010](../adr/0010-explicit-in-process-cqrs-and-preserved-domain-events.md):
aggregate repositories are command-side only, while read-side DTO projection belongs in explicit query services.
Repositories load complete aggregates for command-side mutation.
`IUnitOfWork.SaveChangesAsync` is called once at the end of a handler flow.
Domain events improve traceability, but they do not replace explicit audit logging of successful mutating commands.
Backend feature structure follows [ADR 0014](../adr/0014-feature-oriented-backend-module-structure.md):
feature-local abstractions live under `Features/<Feature>/Abstractions`, while deeper technical or aggregate-specific folders remain optional when complexity justifies them.

---

## Messaging Direction

In-process communication uses Mediator.
Durable out-of-process messaging remains deferred.
If a real async choreography or extracted-service use case appears, revisit [ADR 0001](../adr/0001-durable-intermodule-messaging.md) before implementation.
Same-module domain reactions may use preserved transactional domain events.

---

## Host And Web Boundary

The Host currently acts as the BFF for the SPA.
It owns the external HTTP boundary, session management, and future reverse-proxy behavior if modules are extracted.
Management authentication uses Keycloak with Host-managed OpenID Connect and secure cookie sessions, as accepted in [ADR 0009](../adr/0009-management-portal-authentication-with-keycloak-and-host-cookies.md).
Customer portal authentication direction uses a separate Keycloak client and a separate Host-managed customer cookie session, as accepted in [ADR 0011](../adr/0011-customer-portal-authentication-with-keycloak-and-host-cookies.md).
When module-owned write paths need operator traceability, the Host resolves the authenticated principal into a shared `CurrentActor` abstraction and exposes it through `ICurrentActorAccessor` so modules can persist stable actor IDs without depending on `HttpContext`.
Module endpoints are registered into the Host, but module behavior remains module-owned.
Customer-facing recommendation behavior is planned around separate `CustomerProfile` and `Recommendation` modules that collaborate through contracts rather than Host-owned product logic, as accepted in [ADR 0012](../adr/0012-customer-profile-and-recommendation-modules-with-deterministic-candidate-building.md).
Recommendation orchestration now uses ordinary module application-service coordination around a single Microsoft Agent Framework `ChatClientAgent`, while deterministic filtering and grouping remain in plain module code, as accepted in [ADR 0019](../adr/0019-recommendation-single-agent-runtime-with-context-provider.md).
Within the Recommendation module, model-visible conversation state should flow through a stable `ChatClientAgent`, persisted `AgentSession` state, and native Agent Framework message history, while customer-facing recommendation turns remain a business projection under the same session parent, as accepted in [ADR 0018](../adr/0018-recommendation-native-agent-history-and-business-turns.md).
Deterministic narration snapshots should be assembled per run through module-owned `AIContextProvider` code, while the final narrator receives explicit model-visible context messages derived from that deterministic snapshot.
Recommendation semantic retrieval now uses Qdrant as `Recommendation`-owned derived projection storage over contracts-facing catalog reads, while PostgreSQL remains the canonical catalog store, as accepted in [ADR 0016](../adr/0016-recommendation-semantic-retrieval-with-qdrant.md).

---

## Infrastructure Direction

Local development uses Aspire orchestration through `AlCopilot.Orchestration`.
Production deployment is designed around GitHub Actions, GHCR, AKS, Flux, and PostgreSQL.
Envoy Gateway is the external ingress layer.
For recommendation development, the default local CPU-oriented Ollama profile is `gemma4:e4b` unless a newer documented default replaces it.
Local semantic-retrieval development should treat Qdrant as the default vector store rather than PostgreSQL extensions or graph retrieval infrastructure.
Recommendation workflow execution should enable Agent Framework OpenTelemetry spans by default unless a future operational decision changes that baseline.
Operationally queryable audit records should be stored in the owning module when mutation history must be reviewed directly by operators.
Preserved domain events are module-owned machine-readable history and may support aggregate-level audit timelines, but workflow-rich operator audit can still require explicit audit records.

---

## JSONB Usage Guidance

JSONB is allowed as a narrow workflow-storage exception, not as the default persistence style for domain aggregates.
The current approved example is import-batch review state in the Drinks Catalog, where provenance, diagnostics, review rows, and apply summaries need flexible operator-facing storage without introducing many short-lived workflow tables.
Core aggregates should continue using explicit relational columns and child tables by default because that keeps migrations, query intent, and aggregate evolution clearer over time.

---

## Related Guidance

- [../testing/server.md](../testing/server.md) — backend testing strategy
- [../constitution/server.md](../constitution/server.md) — backend workflow and quality expectations
