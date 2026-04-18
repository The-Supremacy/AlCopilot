# ADR 0014: Feature-Oriented Backend Module Structure

## Status

Accepted

## Date

2026-04-17

## Context

The backend recently completed a substantial refactor of the `DrinkCatalog` module.
That work established a cleaner feature-oriented structure than the newer `CustomerProfile` and `Recommendation` modules currently follow.

The team now wants to refactor those newer modules to match the newer backend direction while preserving the modular-monolith and DDD guidance already accepted in earlier ADRs.
At the same time, the team wants a documented answer for a few structure questions that have started to recur during backend work:

- where feature-local interfaces should live
- when to introduce extra folders such as `QueryServices` or `Repositories`
- when to introduce an additional `{Aggregate}` level beneath a feature folder

Without a documented convention, every module refactor risks re-litigating layout choices and drifting into inconsistent structures.

## Decision

Adopt a feature-oriented backend structure with a small set of explicit conventions.

Specifically:

- Backend modules SHALL organize implementation primarily by feature under `Features/<FeatureName>/`.
- Feature-local interfaces and other abstractions SHALL live under `Features/<FeatureName>/Abstractions/`.
- Aggregate repositories remain command-side collaborators and query services remain read-side collaborators, following ADR 0010.
- Additional technical subfolders such as `QueryServices/`, `Repositories/`, or `Workflows/` MAY be introduced inside a feature when the feature is crowded enough that the extra level improves readability.
- An additional aggregate level such as `Features/<FeatureName>/<AggregateName>/` MAY be introduced when a feature owns multiple aggregates or when aggregate-specific collaborators would otherwise make the feature root noisy.
- Optional deeper folders SHOULD NOT be introduced preemptively for small features.
- Common feature artifacts that serve the whole feature SHOULD remain at the feature root until a clear pressure for more structure appears.

## Reason

This ADR is `Accepted` because the project now has enough backend surface area that module structure itself affects maintainability and team velocity.

The `DrinkCatalog` refactor already demonstrated a practical end-state that is more readable than the older flat feature layout.
Documenting that direction now reduces future drift while still leaving room for pragmatic variation when a feature genuinely grows.

The decision deliberately avoids mandating deep folder hierarchies everywhere.
The team wants consistency, but not ceremony for its own sake.

## Consequences

- `CustomerProfile` and `Recommendation` should be refactored toward the same feature-oriented structure already used in `DrinkCatalog`.
- Future backend work has a documented default for where feature abstractions belong.
- Repository and query-service growth can be handled incrementally instead of forcing one universal folder scheme.
- Some judgment remains necessary because optional deeper folders are allowed rather than mandated.

## Alternatives Considered

### Keep the older flat feature layout

Rejected.
The newer backend modules would continue to drift away from `DrinkCatalog`, and feature-local abstractions would remain inconsistently placed.

### Require deep technical subfolders for every feature

Rejected.
This would create unnecessary ceremony for smaller features and reduce readability when the feature surface is still compact.

### Require an aggregate-named folder under every feature

Rejected.
This works well for larger or multi-aggregate features, but it is unnecessary overhead for simple features such as `CustomerProfile/Profile`.

## Supersedes

None.

## Superseded by

None.
