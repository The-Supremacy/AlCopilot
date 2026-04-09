# Web Architecture

## Purpose

This document is the detailed frontend architecture guide for AlCopilot.
Use it for human-readable frontend architecture context and current client-side decisions.

---

## Platform Shape

The frontend is a React SPA built in the `web/` workspace.
It talks to the Host over same-origin application routes and remains unaware of future backend module extraction.

---

## Core Stack

| Concern           | Choice                   | Notes                                            |
| ----------------- | ------------------------ | ------------------------------------------------ |
| Build tool        | Vite                     | Fast dev server and aligned test tooling         |
| UI runtime        | React + TypeScript       | Strict mode frontend codebase                    |
| Routing           | TanStack Router          | Type-safe route definitions                      |
| Server state      | TanStack Query           | Shared pattern for backend data fetching         |
| Client-only state | Zustand                  | Small focused client state when needed           |
| Styling           | Tailwind CSS + shadcn/ui | Preferred UI direction once components are added |

The accepted frontend stack decision is recorded in [ADR 0003](../adr/0003-frontend-stack.md).

---

## Backend Interaction

The SPA talks to the Host rather than directly to individual modules.
The Host owns authentication, session management, and future reverse-proxy behavior.
Frontend code should depend on stable application APIs rather than backend implementation details.

---

## Workspace Structure

- `web/apps/alcopilot-portal/` — main user-facing application
- `web/packages/` — shared packages when needed

---

## Related Guidance

- [../testing/web.md](../testing/web.md) — frontend testing strategy
- [../constitution/web.md](../constitution/web.md) — frontend workflow and quality expectations
