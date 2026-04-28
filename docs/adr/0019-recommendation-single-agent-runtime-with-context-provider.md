# ADR 0019: Recommendation Single Agent Runtime With Context Provider

## Status

Accepted

## Date

2026-04-27

## Context

ADR 0015 adopted Microsoft Agent Framework for recommendation narration and originally expected bounded Agent Framework workflows to be the orchestration layer.

Implementation experience showed that the current recommendation path is still a single conversational agent over deterministic backend context rather than a genuinely multi-step workflow graph.
The module now has clearer boundaries:

- deterministic recommendation policy lives in normal module services
- semantic retrieval and candidate building happen before narration
- model-visible durable history flows through native Agent Framework chat history
- per-run grounded context is assembled through `AIContextProvider`
- public transcript and recommendation outputs remain business projections

The team also wants to keep the current implementation boring while preserving MAF Workflows as the right tool for future complex orchestration.

## Decision

The Recommendation module will use a single Agent Framework `ChatClientAgent` runtime for current recommendation narration, with deterministic per-run context assembled by module-owned `AIContextProvider` code.

Specifically:

- Recommendation narration SHALL use `ChatClientAgent`, `ChatClientAgentOptions`, `AgentSession`, and native framework chat history for model-visible conversation state.
- Recommendation request handling MAY remain ordinary module application-service orchestration when the flow is a single agent call plus persistence.
- Deterministic context assembly SHALL stay in module-owned services and be injected into the agent through `AIContextProvider` rather than through ad hoc prompt mutation.
- Semantic retrieval, intent resolution, candidate building, hard exclusions, availability grouping, feedback, and persistence SHALL remain outside model-owned execution.
- MAF Workflows SHOULD be introduced only when the recommendation flow needs explicit graph orchestration, branching, fan-out, human-in-the-loop steps, or multi-agent coordination.
- Recommendation eval tests SHALL exercise the real `ChatClientAgent`, context-provider chain, tools, and configured LLM provider when model behavior is the risk under review.

## Reason

This ADR is `Accepted` because it matches the code shape that proved simpler and more MAF-native for the current recommendation use case.

A workflow graph would add ceremony without improving correctness while the runtime remains one narrator agent over a deterministic RAG snapshot.
Keeping normal service orchestration around the agent also preserves module boundaries and makes persistence, diagnostics, and business projections easier to test.

The decision still preserves the learning path for MAF:
the module uses native agents, sessions, chat history, context providers, compaction, tool calls, telemetry, and evals.
It only defers Workflow adoption until the problem actually needs Workflow semantics.

## Consequences

- ADR 0015 remains useful for the broader Agent Framework adoption direction, but its requirement to use Workflows for current recommendation orchestration is superseded.
- Recommendation docs should describe the current request-time spine as application-service orchestration plus a single `ChatClientAgent` and `AIContextProvider`.
- Future workflow work should be justified by concrete orchestration complexity rather than by framework uniformity.
- Eval tests remain important because prompt, tool, history, and context-provider changes can affect model behavior even without a Workflow graph.

## Alternatives Considered

### Keep Agent Framework Workflows As The Current Orchestration Layer

Rejected for the current slice.
The recommendation runtime does not yet need graph routing, branching, fan-out, or human-in-the-loop workflow state.
Using Workflows now would make simple service orchestration harder to read without adding meaningful capability.

### Return To Direct Chat Client Calls Without Agent Framework

Rejected.
That would lose the learning value and runtime structure gained from Agent Framework agents, sessions, native chat history, context providers, compaction, tool handling, telemetry, and evals.

### Move Deterministic Policy Into The Agent Or Workflow Definition

Rejected.
Hard exclusions, availability grouping, semantic retrieval policy, candidate scoring, and persistence are module-owned behavior and should remain directly testable without model execution.

## Supersedes

ADR 0015 in the requirement to use Agent Framework Workflows as the current Recommendation orchestration layer.

## Superseded by

None.
