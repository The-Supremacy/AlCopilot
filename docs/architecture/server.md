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
Repositories load complete aggregates.
`IUnitOfWork.SaveChangesAsync` is called once at the end of a handler flow.

---

## Messaging Direction

In-process communication uses Mediator.
Durable out-of-process messaging remains deferred.
If a real async choreography or extracted-service use case appears, revisit [ADR 0001](../adr/0001-durable-intermodule-messaging.md) before implementation.

---

## Host And Web Boundary

The Host currently acts as the BFF for the SPA.
It owns the external HTTP boundary, session management, and future reverse-proxy behavior if modules are extracted.
Module endpoints are registered into the Host, but module behavior remains module-owned.

---

## Infrastructure Direction

Local development uses Aspire orchestration.
Production deployment is designed around GitHub Actions, GHCR, AKS, Flux, and PostgreSQL.
Envoy Gateway is the external ingress layer.

---

## Related Guidance

- [../testing/server.md](../testing/server.md) — backend testing strategy
- [../constitution/server.md](../constitution/server.md) — backend workflow and quality expectations
