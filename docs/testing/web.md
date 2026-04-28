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

| Type                         | Purpose                                                                                          | Tools                          | Typical scope                                                  |
| ---------------------------- | ------------------------------------------------------------------------------------------------ | ------------------------------ | -------------------------------------------------------------- |
| Unit                         | Pure logic and small hooks                                                                       | Vitest                         | Utilities, formatters, reducers, pure helpers                  |
| Component                    | Rendering and interaction behavior                                                               | Vitest + React Testing Library | Components, forms, interaction states                          |
| Feature or Route Integration | Router, query, provider, and page-level behavior with mocked backend boundaries                  | Vitest + React Testing Library | Page flows, route guards, query invalidation, optimistic UI    |
| E2E                          | Real browser flows against a deployed environment or a local app with mocked external boundaries | Playwright                     | Critical happy paths, auth-sensitive flows, release confidence |

---

## Tooling Direction

Use Vitest for frontend non-E2E testing.
Prefer user-centric assertions and accessible queries.
Use Playwright narrowly and intentionally rather than as the default home for frontend confidence.

### Playwright E2E

Playwright coverage currently lives in the local-only `@alcopilot/e2e` workspace package.
Do not add it to pull-request CI until the project has a stable staging path, deterministic test accounts, and clear handling for the self-hosted identity provider and self-hosted LLM.

Run E2E manually from the repository root:

```bash
pnpm --filter @alcopilot/e2e install:browsers
pnpm --filter @alcopilot/e2e test
pnpm --filter @alcopilot/e2e test:video
pnpm --filter @alcopilot/e2e test:ui
pnpm --filter @alcopilot/e2e test:headed
pnpm --filter @alcopilot/e2e report
```

The Playwright config starts the management and customer Vite apps locally and mocks backend API calls inside tests.
This keeps the initial E2E suite useful for learning browser automation, routing, forms, reports, screenshots, traces, and video recording without requiring Keycloak, a backend host, or LLM infrastructure.

Default artifacts:

- HTML report: `web/apps/e2e/playwright-report/`
- JUnit report: `web/apps/e2e/test-results/junit.xml`
- Failure screenshots, retained traces, and retained videos: `web/apps/e2e/test-results/`
- Full-run videos when using `test:video`: `web/apps/e2e/test-results/`

E2E tests should keep using accessible locators such as roles, labels, and visible text.
Use mocked network responses for local-only E2E unless the behavior under test specifically requires a real deployed environment.
Keep LLM behavior deterministic by asserting against mocked recommendation payloads rather than live model output.

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
