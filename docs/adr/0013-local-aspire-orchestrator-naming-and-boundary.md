# ADR 0013: Local Aspire Orchestrator Naming And Boundary

## Status

Proposed

## Date

2026-04-16

## Context

The backend runtime currently uses `AlCopilot.Host` for the web and API boundary and `AlCopilot.AppHost` for local Aspire orchestration.

Those names are easy to confuse during development, documentation, code review, and onboarding because both sound like application-entry projects while only one of them is the actual external application boundary.

`AlCopilot.AppHost` is not the backend Host in the architectural sense.
It exists to compose local development infrastructure and application processes through Aspire.

The project needs a clearer naming distinction so the runtime boundary and the local orchestration boundary are not conflated in code or documentation.

## Decision

Rename the local Aspire orchestration project from `AlCopilot.AppHost` to `AlCopilot.Orchestrator`.

Reserve `AlCopilot.Host` for the actual backend web and API boundary.

Treat `AlCopilot.Orchestrator` as the local development orchestration entrypoint that composes infrastructure and application processes for Aspire-based workflows.

## Reason

This ADR is `Proposed` because the decision changes project naming and developer-facing guidance, but the rename itself has not been carried out yet.

Recording the direction now creates a clear source of truth before solution references, docs, and local setup instructions are updated.

`Orchestrator` is preferred over alternatives such as `DevHost` or `AspireHost` because it describes the project's role without implying that it is the primary application Host and without making the name overly environment-specific.

## Consequences

- The distinction between the web/API boundary and the local orchestration boundary becomes clearer.
- Solution files, project references, and local development instructions will need coordinated updates when the rename is implemented.
- Existing references to `AppHost` in developer-facing docs and setup guidance will need to be synced if this ADR is accepted.
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

Preferred.
This best communicates the project's role as the local composition and orchestration entrypoint while keeping `Host` reserved for the external application boundary.

## Supersedes

None.

## Superseded by

None.
