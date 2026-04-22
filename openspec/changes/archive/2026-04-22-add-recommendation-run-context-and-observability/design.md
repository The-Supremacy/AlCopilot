## Context

`Recommendation` already performs deterministic candidate preparation outside the model and persists recommendation sessions with Agent Framework session state.

However, the model-visible context is still a narrow snapshot string, recipe drill-down is unavailable unless the prompt already contains enough detail, and the recommendation runtime does not currently expose agent-level traces in Aspire.

This change keeps the bounded recommendation architecture intact while improving how deterministic recommendation context is presented to the model and how developers can inspect the runtime.

## Goals / Non-Goals

**Goals:**

- Rename the current recommendation narration snapshot concept to recommendation run context.
- Give the model a richer deterministic view of owned ingredients and grouped candidate drinks without dumping the full raw catalog into every run.
- Introduce one read-only recipe lookup tool for exact recipe drill-down.
- Record recipe-tool usage on persisted assistant turns.
- Emit agent, chat, tool, and run-context assembly spans to the existing OpenTelemetry pipeline.

**Non-Goals:**

- Add embeddings or Qdrant-backed retrieval.
- Allow model-owned writes or profile mutation.
- Replace deterministic candidate preparation with tool-first or vector-first recommendation logic.
- Introduce multiple recommendation tools in the first slice.

## Decisions

### 1. Recommendation run context stays deterministic and request-scoped

The recommendation module will rename `RecommendationNarrationSnapshot` and related seams toward `RecommendationRunContext`.

The run context will be assembled from module-owned deterministic collaborators before each recommendation run.

It will include:

- customer preference and owned-ingredient names
- grouped make-now and buy-next items
- per-item owned versus missing ingredient summaries
- stable drink identifiers that the tool layer can use for exact drill-down

The run context will not embed every catalog field for every drink.

Why this over passing the full raw recommendation catalog to the model:
the model only needs bounded explanation-oriented facts for narration, while exact recipe detail can come from a read-only tool on demand.

### 2. Recipe drill-down is a single read-only tool

The first tool surface will be a single `lookup_drink_recipe` tool.

The tool will prefer a drink identifier when available and may fall back to a drink-name lookup for agent usability.

The tool result will return structured drink detail information suitable for narration:

- drink identity and name
- description, method, and garnish
- recipe entries with ingredient names, quantities, recommended brands, and notable brands where available

The tool will remain read-only and will use existing contracts-facing query paths.

Why this over adding multiple tools immediately:
the first gap is exact recipe drill-down, and one tool keeps the model surface bounded while still proving the interaction pattern.

### 3. Tool usage is recorded independently of model text

Recommendation persistence already supports assistant-turn tool invocation metadata through `ToolInvocationsJson`.

This change will record actual tool usage through a scoped module-owned recorder rather than trying to infer it from the final prose response.

Why this over parsing tool usage back out of the model response:
recording usage from the execution path is more reliable than trying to recover it from response text or connector-specific message content.

### 4. Recommendation observability uses the existing OpenTelemetry pipeline

The recommendation runtime will instrument:

- agent invocation
- chat/model calls
- function-tool execution
- deterministic run-context assembly

Sensitive prompt, response, and tool payload capture will stay opt-in through configuration and default to disabled.

Why this over a custom recommendation-only logging subsystem:
the host already uses Aspire and OpenTelemetry, so the recommendation module should extend that pipeline rather than introducing a parallel diagnostics path.

## Risks / Trade-offs

- [Prompt bloat] -> Keep the run context bounded to grouped recommendation facts and leave full recipe detail to the tool.
- [Tool misuse by the model] -> Expose only one read-only tool and keep deterministic eligibility logic outside tool execution.
- [Telemetry duplication] -> Use one recommendation source name and avoid enabling sensitive data by default.
- [User worktree drift] -> Apply the change on top of the current `Agents/` folder refactor rather than undoing the user's in-progress file moves.

## Migration Plan

1. Create the OpenSpec change artifacts for the recommendation behavior update.
2. Rename the current snapshot/query/provider/message-builder seams to run-context terminology and enrich the run-context model.
3. Add the scoped recipe lookup tool and tool invocation recorder.
4. Instrument the chat client, agent, and run-context assembly path with OpenTelemetry.
5. Update recommendation tests to cover run-context rendering, recipe tool behavior, persisted tool usage, and observability-safe configuration.

Rollback strategy:

- This change does not require database schema changes.
- Rollback can use normal code reversion without migration reversal.

## Open Questions

- None at the design level.

Implementation may still tune prompt wording, but the architecture and public behavior direction are settled in this change.
