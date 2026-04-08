# ADR 0003: Frontend Stack

## Status

Accepted

## Date

2026-04-08

## Context

AlCopilot needs a frontend stack that stays lightweight, type-safe, and friendly to incremental growth in a greenfield codebase.
The project wants strong routing and server-state patterns without introducing a large framework or heavy abstraction layer.
The UI stack should also support rapid product iteration while keeping styling and component composition straightforward.

## Decision

The frontend stack uses:

- React for the UI runtime
- TanStack Router for routing
- TanStack Query for server-state management
- shadcn/ui for component primitives
- Tailwind CSS for styling

This stack favors explicit composition, low magic, and strong TypeScript ergonomics.

## Reason

This ADR is `Accepted` because the selected stack matches the current frontend direction already documented for the project and provides a clear baseline for future frontend work.
It gives the project modern routing and data-fetching primitives without committing to a large opinionated framework.

## Consequences

- Frontend work has a stable default stack for routing, server state, and styling.
- UI code can stay component-oriented and composable rather than framework-heavy.
- Future frontend additions should align with this stack unless a new ADR revisits the choice.

## Alternatives Considered

### Next.js

Rejected.
The project does not currently need a full-stack React framework or the additional deployment and rendering model complexity that comes with it.

### React Router + ad hoc fetch patterns

Rejected.
This would provide a weaker default story for type-safe routing and server-state consistency.

### Heavier component frameworks such as Material UI

Rejected.
The project prefers lighter primitives and direct control over styling and composition.
