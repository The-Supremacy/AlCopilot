# ADR 0008: Capability-Level Microfrontends Inside Management Portal

## Status

Deferred

## Date

2026-04-10

## Context

The management portal may eventually contain multiple capability slices such as catalog curation, import operations, and other operator tools.
A key decision is whether to deploy those capability slices as independently deployed runtime MFEs now.
The current team intent is to gain most modularity benefits through pnpm workspace boundaries and in-app modular architecture first.

## Decision

Defer capability-level independently deployed MFEs inside the management portal.

For now:

- Management capabilities remain modular within a single management portal deployment.
- Shared workspace packages define stable contracts and reusable primitives.
- Runtime composition, remote module federation, and independent capability deployments are not part of the current portal direction.

## Reason

This ADR is `Deferred` because capability-level runtime MFEs are a valid future direction, but current scope does not justify the additional platform complexity.
The project can deliver manager workflows faster with a modular single-deployment management app while still preserving an extraction path.

Reconsider this ADR when one or more triggers appear:

- multiple teams need independent release cadence for management capabilities
- frequent hotfix isolation requirements appear across management capability boundaries
- CI/CD performance or deployment blast radius becomes a recurring bottleneck

## Consequences

- The current portal direction avoids federation-style runtime complexity while retaining internal modular boundaries.
- Shared contracts and package boundaries must remain disciplined to preserve extraction readiness.
- A future revisit may still introduce capability-level MFEs if operational pressure justifies it.

## Alternatives Considered

### Adopt Capability-Level Runtime MFEs Immediately

Rejected for now.
The added orchestration and runtime coupling costs are not justified at current team and product scale.

### Never Allow Capability-Level MFEs

Rejected.
This could block future team scaling options if independent releases become necessary.

### Use Nx-Orchestrated Capability MFEs Immediately

Rejected for now.
The project wants pnpm-first modularity without adding a heavier orchestration layer before clear need.

## Supersedes

None.

## Superseded by

None.
