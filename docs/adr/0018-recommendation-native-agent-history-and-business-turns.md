# ADR 0018: Recommendation Native Agent History And Business Turns

## Status

Accepted

## Date

2026-04-26

## Context

ADR 0015 adopted Microsoft Agent Framework for recommendation narration and established that model-visible conversation state should flow through `ChatClientAgent`, `AgentSession`, and framework-managed chat history.

The current implementation still reconstructs model-visible history from `ChatTurn` business records.
Those records mix several concerns:

- customer-visible user and assistant text
- recommendation groups and item detail
- tool invocation summaries
- optional development execution trace
- feedback fields

This makes the public business transcript the effective model transcript, which limits learning value from Agent Framework and makes compaction less meaningful.
It also causes core recommendation records to rely on JSONB for data that is better represented relationally.

The project's goal is educational as well as functional.
The team wants to learn how Agent Framework native history, tool result groups, per-service-call persistence, `AgentSession` state, and compaction strategies behave in a realistic system without leaking internal model mechanics into customer-facing APIs.

Microsoft Agent Framework compaction strategies operate on in-memory `ChatMessage` lists grouped by `CompactionMessageIndex`, not on database tables.
Storage still matters because the `ChatHistoryProvider` determines which native messages are replayed into those lists across invocations.
`ToolResultCompactionStrategy` specifically collapses older tool-call message groups in the replayed/accumulated message list.

## Decision

The Recommendation module will separate model-native agent history from business-visible recommendation turns while keeping both under the same recommendation session parent.

Specifically:

- `ChatSession` remains the business parent for a customer's recommendation conversation.
- `ChatTurn` remains the customer-facing business projection and stores only user-visible turn identity, role, ordering, feedback, and links to the native message that supplies display content.
- Agent Framework-native history will be persisted separately as `AgentMessage` records associated with the same `ChatSession`.
- `AgentMessage` records are the source for model replay and may include user, assistant, system, tool-call, tool-result, compaction-summary, and provider-specific native messages.
- Tool calls and tool results SHALL remain part of native `AgentMessage` storage rather than being duplicated into a separate tool-call table by default.
- Diagnostics that are not customer-visible turns, including deterministic run context, semantic-search evidence, candidate-building trace, token usage, finish reasons, and provider-exposed reasoning text, SHALL be stored as agent run/message diagnostics rather than on `ChatTurn`.
- Provider-exposed reasoning content SHOULD be retained for learning and debugging, but the system SHALL store only reasoning content returned by the provider/runtime, not private chain-of-thought that the provider does not expose.
- Recommendation groups and recommendation items SHALL be represented as business output linked to assistant turns when they are needed by the customer API or feedback workflows.
- Recommendation group/item details SHOULD be normalized into relational tables when retained as durable business output; JSONB remains acceptable only for raw provider/runtime payloads and diagnostics whose shape is intentionally framework-specific.
- Derived projections such as tool analytics SHALL be built at runtime from `AgentMessage` records unless a concrete query or operational need justifies a durable projection table.
- The public recommendation API SHALL continue returning only customer-visible user and assistant turns, not native system/tool/diagnostic messages.
- Recommendation eval tests SHALL be used as before/after guardrails because changing native replay history can affect model behavior.

## Reason

This decision is accepted because it aligns Recommendation persistence with the project's Agent Framework learning goal while simplifying the business transcript.

Native persisted agent messages make Agent Framework features more meaningful:

- the `ChatHistoryProvider` can replay the actual model transcript instead of approximating it from business turns
- tool-call and tool-result messages can participate in native grouping and compaction
- compaction can be evaluated as context management rather than as a database storage concern
- provider-exposed reasoning and other diagnostics can be inspected without polluting the customer transcript

Keeping business turns as a thin projection preserves product needs such as customer history display and assistant-turn feedback.
It also avoids duplicating message text across business and native storage.

The JSONB reduction follows the existing backend guidance that core aggregates should prefer explicit relational tables while reserving JSONB for raw or flexible workflow payloads.

## Consequences

- The Recommendation module will need a persistence migration that adds native agent-message storage and thins existing `ChatTurn` records.
- The current `RecommendationChatHistoryProvider` will need to become a native message history provider rather than a mapper over business turns.
- Existing tool invocation and execution trace recorders can likely shrink or become diagnostic writers.
- Public API mapping will need to resolve display content from linked native agent messages while filtering out internal system, tool, and diagnostic messages.
- User-message edits should be treated as a new native message/replay branch rather than in-place mutation of past native model history.
- Recommendation eval tests must be run before and after the migration to detect quality regressions from changed replay history.
- The design intentionally avoids durable projection tables for tool calls until query needs justify them.

## Alternatives Considered

### Keep `ChatTurn` As Both Business Transcript And Model Transcript

Rejected.
This keeps the schema smaller, but it blurs customer-visible data with model-runtime internals and prevents Agent Framework native tool/system/compaction messages from being represented cleanly.

### Add Separate Tool Call Tables Immediately

Rejected for the initial migration.
Tool calls and tool results are already native agent messages.
A separate tool-call table would duplicate data unless a concrete analytics or operational query requires a durable projection.

### Store All Recommendation Output Only In Native Agent Messages

Rejected.
Recommendation groups, feedback, and customer-facing assistant turns are product/domain concepts.
They should remain available through business-oriented records rather than requiring the UI and query services to parse framework-native message payloads.

### Keep Recommendation Group JSONB On `ChatTurn`

Rejected as the long-term shape.
Recommendation groups and items are durable business output returned by the API and used for feedback/debugging.
They should be normalized when retained rather than remaining as opaque JSONB on core turn records.

## Supersedes

None.

## Superseded by

None.
