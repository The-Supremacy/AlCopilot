## Why

The recommendation chat can already narrate from deterministic candidate groups, but it still gives the model a compressed context that makes home-bar questions harder to answer cleanly and leaves no first-class drill-down path for exact recipe details.

The current runtime also lacks agent-level observability in Aspire, which makes it harder to understand whether a response came from deterministic candidate preparation, model narration, or read-only tool usage.

This change matters now because the recommendation module has reached the point where richer bar-aware narration, bounded tool calling, and debuggable runtime traces will improve both customer-facing behavior and day-to-day development confidence.

## What Changes

- Add a richer recommendation run context that gives the narrator a clearer home-bar-aware view of owned ingredients, make-now drinks, and near-miss drinks.
- Add a single read-only `lookup_drink_recipe` tool so the agent can fetch exact recipe details when narration needs more than the bounded run context.
- Add recommendation-runtime observability so Aspire can show agent invocation, model call, tool call, and run-context assembly traces.
- Rename the current recommendation narration snapshot concept toward recommendation run context terminology.

## Capabilities

### Modified Capabilities

- `recommendation-chat`: recommendation narration gains richer bar-aware context, a bounded read-only recipe lookup tool, and observable runtime traces for debugging and development.

## Impact

- Affected modules: `server/src/Modules/AlCopilot.Recommendation`, `server/src/Modules/AlCopilot.DrinkCatalog.Contracts`, and related tests.
- Affected dependencies: the existing Microsoft Agent Framework and `Microsoft.Extensions.AI` stack will be used more fully for tools and OpenTelemetry instrumentation.
- Affected documentation: a new OpenSpec behavior change under `recommendation-chat`; no ADR update is expected unless implementation reveals a new durable architecture decision beyond this feature work.
