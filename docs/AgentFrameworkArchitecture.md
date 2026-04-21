# Recommendation Agent Framework Architecture

This note describes the current simplified Recommendation conversation flow.

The module now uses Agent Framework for agent execution, session serialization, and current-turn context enrichment.

It does not use Agent Framework workflows or a custom chat history provider.

---

## Public API Status

The customer-facing HTTP surface did not change.

Routes are still:

- `GET /api/customer/recommendations/sessions`
- `GET /api/customer/recommendations/sessions/{sessionId}`
- `POST /api/customer/recommendations/messages`

The send-message contract is still:

```csharp
public sealed record SubmitRecommendationRequestCommand(
    Guid? SessionId,
    string Message)
```

The response DTOs in `AlCopilot.Recommendation.Contracts` also kept the same shape.

---

## High-Level Flow

```text
UI
  |
  v
POST /api/customer/recommendations/messages
  |
  v
SubmitRecommendationRequestHandler
  |
  v
IRecommendationConversationService
  |
  +--> load or create ChatSession
  |
  +--> build RecommendationNarrationSnapshot
  |
  +--> RecommendationNarratorAgentFactory.Create()
  |      - build configured ChatClientAgent
  |
  +--> RecommendationAgentSessionStore
  |      - restore/create AgentSession
  |      - serialize AgentSession after run
  |
  +--> build messages from ChatSession.Turns + current user message
  |
  +--> agent.RunAsync(messages, session)
  |      |
  |      +--> RecommendationNarrationContextProvider
  |             - inject current-turn recommendation context
  |
  +--> persist:
         - AgentSessionStateJson
         - user turn
         - assistant turn
  |
  v
RecommendationSessionDto
```

---

## Class Responsibilities

`SubmitRecommendationRequestHandler`

- Resolves the current customer id.
- Delegates the request to the conversation service.

`IRecommendationConversationService`

- Feature-level application boundary for one recommendation conversation turn.

`RecommendationConversationService`

- Loads or creates `ChatSession`.
- Builds deterministic recommendation context.
- Restores the Agent Framework session.
- Manually assembles prior chat messages from `ChatSession.Turns`.
- Calls the agent.
- Persists transcript and serialized AF session state.
- Returns `RecommendationSessionDto`.

`RecommendationNarratorAgentFactory`

- Builds the configured `ChatClientAgent`.
- Wires base instructions and the current-turn context provider.
- Future seam for narrator agent vs embedding runtime.

`RecommendationAgentSessionStore`

- Restores `AgentSession` from `ChatSession.AgentSessionStateJson`.
- Creates a new `AgentSession` when no serialized state exists.
- Serializes the updated `AgentSession` after the run.

`RecommendationNarrationContextProvider`

- A `MessageAIContextProvider`.
- Builds the current-turn AI context from app services.
- Injects a system message with the fresh recommendation snapshot.

`IRecommendationNarrationContextQueryService`

- Builds deterministic recommendation context for AI use.
- Shared read-side service used by the context provider and conversation service.

`RecommendationNarrationMessageBuilder`

- Formats `RecommendationNarrationSnapshot` into AI-facing prompt text.

`IRecommendationEmbeddingRuntime`

- Placeholder seam for future embedding support.
- Not used by the current request flow yet.

---

## Persistence Model

Two persistence tracks still exist on purpose.

`ChatSession` and `ChatTurn` are the business-visible transcript.

`ChatSession.AgentSessionStateJson` stores the opaque serialized Agent Framework session.

```text
Business persistence:
  ChatSession
    - title
    - timestamps
    - Turns[]

Agent Framework persistence:
  ChatSession.AgentSessionStateJson
    - serialized AgentSession
    - provider state
    - model/session continuity data
```

The UI should read `RecommendationSessionDto`.

Agent Framework should restore from `AgentSessionStateJson`.

---

## Practical Testing Guidance

The current UI draft should still work if it already uses:

- `POST /api/customer/recommendations/messages` with `{ sessionId, message }`
- `GET /api/customer/recommendations/sessions`
- `GET /api/customer/recommendations/sessions/{sessionId}`

The most valuable end-to-end checks are:

1. Start a new conversation and verify the response includes one user turn and one assistant turn.
2. Send a second message to the same `sessionId` and verify prior conversation is reflected in the answer.
3. Restart the server, send another message to the same `sessionId`, and verify continuity still works.
4. Verify recommendation-context system messages do not appear as persisted chat turns.
