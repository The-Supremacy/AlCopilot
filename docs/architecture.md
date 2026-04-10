# AlCopilot — Architecture

## Purpose

This document is the thin architecture index for AlCopilot.
It summarizes the platform shape and links to the detailed area architecture documents.

---

## System Overview

AlCopilot is an AI-powered drinks suggestion platform built as a modular monolith.
The system is a single deployable unit with internal module boundaries, one `DbContext` per module, and the option to extract modules later if a concrete operational need appears.

| Area                          | Detailed guide                                   |
| ----------------------------- | ------------------------------------------------ |
| Backend and host architecture | [architecture/server.md](architecture/server.md) |
| Frontend architecture         | [architecture/web.md](architecture/web.md)       |

---

## Current Architecture Direction

- The backend is a modular monolith built on .NET, Aspire, EF Core, PostgreSQL, and Mediator.
- The Host is the current web-facing backend boundary and composes module endpoints in-process.
- The frontend direction is portal-level separation with independently deployable user and management portals in the `web/` workspace.
- Portal UI invariants are documented in portal-level `web/apps/*/DESIGN.md` guides and evolve separately from OpenSpec behavior specs.
- Durable out-of-process messaging remains deferred until there is a concrete approved use case.

---

## Key Decision Records

- [ADR 0001: Durable Intermodule Messaging](adr/0001-durable-intermodule-messaging.md) — deferred messaging direction
- [ADR 0002: Domain-Driven Design Patterns](adr/0002-domain-driven-design-patterns.md) — accepted backend DDD defaults
- [ADR 0003: Frontend Stack](adr/0003-frontend-stack.md) — accepted frontend stack
- [ADR 0004: Thin Index Documentation Structure](adr/0004-thin-index-documentation-structure.md) — accepted documentation information architecture
- [ADR 0005: Testing Strategy And Shared Integration Harness](adr/0005-testing-strategy-and-shared-integration-harness.md) — accepted testing strategy and backend harness direction
- [ADR 0006: AI Recommendation Retrieval And Catalog Storage](adr/0006-ai-recommendation-retrieval-and-catalog-storage.md) — deferred AI retrieval and storage direction
- [ADR 0007: Management Portal Architecture And Envoy Host Routing](adr/0007-management-portal-architecture-and-envoy-host-routing.md) — accepted portal boundary and AKS ingress direction
- [ADR 0008: Capability-Level Microfrontends Inside Management Portal](adr/0008-capability-level-microfrontends-inside-management-portal.md) — deferred capability-level runtime MFE direction

---

## Related Guidance

- [constitution.md](constitution.md) — thin governance index
- [domain.md](domain.md) — thin domain index
- [testing.md](testing.md) — thin testing index
