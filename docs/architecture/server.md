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

---

## Infrastructure Direction

Local development uses Aspire orchestration.
Production deployment is designed around GitHub Actions, GHCR, AKS, Flux, and PostgreSQL.
Envoy Gateway is the external ingress layer.
Operationally queryable audit records should be stored in the owning module when mutation history must be reviewed directly by operators.
Preserved domain events are module-owned machine-readable history and may support aggregate-level audit timelines, but workflow-rich operator audit can still require explicit audit records.

---

## JSONB Usage Guidance

JSONB is allowed as a narrow workflow-storage exception, not as the default persistence style for domain aggregates.
The current approved example is import-batch review state in the Drinks Catalog, where provenance, diagnostics, review rows, conflict summaries, and apply summaries need flexible operator-facing storage.
Core aggregates should continue using explicit relational columns and child tables by default because that keeps migrations, query intent, and aggregate evolution clearer over time.

---

## Related Guidance

- [../testing/server.md](../testing/server.md) — backend testing strategy
- [../constitution/server.md](../constitution/server.md) — backend workflow and quality expectations
