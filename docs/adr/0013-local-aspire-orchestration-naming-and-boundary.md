# ADR 0013: Local Aspire Orchestration Naming And Boundary

## Status

Accepted

## Date

2026-04-16

## Context

The backend runtime currently uses `AlCopilot.Host` for the web and API boundary and `AlCopilot.AppHost` for local Aspire orchestration.

Those names are easy to confuse during development, documentation, code review, and onboarding because both sound like application-entry projects while only one of them is the actual external application boundary.

`AlCopilot.AppHost` is not the backend Host in the architectural sense.
It exists to compose local development infrastructure and application processes through Aspire.

The project needs a clearer naming distinction so the runtime boundary and the local orchestration boundary are not conflated in code or documentation.

## Decision

Rename the local Aspire orchestration project from `AlCopilot.AppHost` to `AlCopilot.Orchestration`.

Reserve `AlCopilot.Host` for the actual backend web and API boundary.

Treat `AlCopilot.Orchestration` as the local development orchestration entrypoint that composes infrastructure and application processes for Aspire-based workflows.

## Reason

`Orchestration` is preferred over alternatives such as `DevHost`, `AspireHost`, or `Orchestrator` because it describes the project's responsibility as a boundary and capability rather than as a runtime actor.
It avoids implying that the Aspire project is the primary application Host while keeping the name broad enough for local infrastructure and application-process composition.

## Consequences

- The distinction between the web/API boundary and the local orchestration boundary becomes clearer.
- Solution files, project references, and local development instructions point to `AlCopilot.Orchestration`.
- Existing references to `AppHost` in active developer-facing docs and setup guidance need to stay aligned with `AlCopilot.Orchestration`.
- The rename adds short-term churn across project names, namespaces, and local workflow documentation.

## Alternatives Considered

### Keep `AlCopilot.AppHost`

Rejected.
This preserves current confusion between the architectural Host boundary and the Aspire orchestration project.

### Rename To `AlCopilot.DevHost`

Rejected.
This would be clearer than `AppHost`, but it frames the project too narrowly around development environment naming rather than its orchestration responsibility.

### Rename To `AlCopilot.AspireHost`

Rejected.
This is more explicit about the tool, but it still uses `Host` terminology in a way that can be confused with the actual backend Host boundary.

### Rename To `AlCopilot.Orchestrator`

Rejected.
This communicates the local composition role, but it frames the project as an actor rather than the orchestration boundary itself.

### Rename To `AlCopilot.Orchestration`

Accepted.
This best communicates the project's role as the local composition and orchestration entrypoint while keeping `Host` reserved for the external application boundary.

## Supersedes

None.

## Superseded by

None.
