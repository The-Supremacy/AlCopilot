# Web Testing Strategy

## Purpose

This document is the detailed frontend testing guide for AlCopilot.
It defines the frontend test taxonomy, preferred tools, and design-for-testability expectations.

---

## Principles

- Prefer the lowest frontend test level that gives enough confidence for the behavior under change.
- Keep most frontend confidence in unit, component, and feature-level tests.
- Use E2E selectively for critical flows and release confidence.
- Build UI so it can be automated meaningfully even when a particular flow does not yet have browser coverage.

---

## Frontend Taxonomy

| Type                         | Purpose                                                                         | Tools                          | Typical scope                                                  |
| ---------------------------- | ------------------------------------------------------------------------------- | ------------------------------ | -------------------------------------------------------------- |
| Unit                         | Pure logic and small hooks                                                      | Vitest                         | Utilities, formatters, reducers, pure helpers                  |
| Component                    | Rendering and interaction behavior                                              | Vitest + React Testing Library | Components, forms, interaction states                          |
| Feature or Route Integration | Router, query, provider, and page-level behavior with mocked backend boundaries | Vitest + React Testing Library | Page flows, route guards, query invalidation, optimistic UI    |
| E2E                          | Real browser flows on a real environment                                        | Playwright                     | Critical happy paths, auth-sensitive flows, release confidence |

---

## Tooling Direction

Use Vitest for frontend non-E2E testing.
Prefer user-centric assertions and accessible queries.
Use Playwright narrowly and intentionally rather than as the default home for frontend confidence.

---

## Design For Testability

Frontend code should be built so it could be automated and verified cleanly.
That means:

- accessible roles and semantics
- stable user-visible selectors and labels
- component boundaries that do not force tests to couple to internals
- state management that keeps router, server, and client-only concerns understandable

---

## Deferred Or Optional Areas

- accessibility automation beyond normal component and feature assertions
- visual regression for a curated set of stable screens
- broader browser-matrix coverage when the product and deployment surface justify it
