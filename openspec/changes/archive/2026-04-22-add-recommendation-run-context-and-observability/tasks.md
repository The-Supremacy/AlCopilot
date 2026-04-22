## 1. Recommendation Run Context

- [x] 1.1 Rename the current recommendation narration snapshot/query/provider/message-builder seams toward recommendation run context terminology.
- [x] 1.2 Enrich the run-context payload so bar-aware narration includes owned ingredients by name plus owned-versus-missing breakdowns for grouped drinks.
- [x] 1.3 Keep deterministic recommendation grouping and persistence behavior intact while switching assistant-turn persistence to the renamed run-context types.

## 2. Recommendation Tooling

- [x] 2.1 Add a single read-only `lookup_drink_recipe` tool backed by existing contracts-facing drink queries.
- [x] 2.2 Record actual recipe-tool usage during agent execution and persist the resulting tool invocation metadata on assistant turns.
- [x] 2.3 Keep tool execution read-only and outside any profile or session mutation path.

## 3. Recommendation Observability

- [x] 3.1 Instrument the recommendation chat client and agent path with OpenTelemetry spans visible in Aspire.
- [x] 3.2 Add a recommendation-owned span around deterministic run-context assembly.
- [x] 3.3 Keep sensitive prompt, response, and tool payload capture configuration opt-in and disabled by default.

## 4. Validation

- [x] 4.1 Add unit or application tests for run-context rendering and recipe lookup behavior.
- [x] 4.2 Update recommendation conversation tests to cover persisted tool usage and run-context-backed narration.
- [x] 4.3 Run targeted recommendation tests and a solution build to verify the change.
