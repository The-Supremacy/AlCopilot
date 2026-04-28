# Recommendation Command Response Simplification

## Context

The Recommendation module is moving toward MAF-native history where persisted `AgentMessage` records are the source of truth for model-visible history and customer-visible transcript projection.

`RecommendationConversationService` currently still contains transient mapping logic that builds a `RecommendationSessionDto` from the immediate `AgentResponse` plus previously loaded session data. That creates a second projection path beside `RecommendationSessionQueryService`, and it generates temporary user turn ids that are not necessarily the persisted `AgentMessage` ids.

The desired direction is:

- command paths persist state and return minimal command results
- query paths project customer-visible transcript from persisted `AgentMessage` records
- frontend reloads session data through the normal GET endpoint after mutations

## Decision

Use the existing `RecommendationSessionQueryService` as the visible transcript projection boundary.

Do not add a separate `IUserMessagesQueryService` for now. The existing query service already owns session DTO projection and can be refocused internally around visible `AgentMessage` filtering.

Message submission should return only the created or reused `sessionId`.

Feedback submission should stop returning the full session DTO. Prefer `204 NoContent`; returning `{ sessionId }` is also acceptable if the frontend client wants a typed mutation result.

## Implementation Plan

### Backend Contracts And Endpoints

- Add a small DTO for message submission results, for example `SubmitRecommendationMessageResultDto(Guid SessionId)`.
- Change `SubmitRecommendationRequestCommand` from `IRequest<RecommendationSessionDto>` to `IRequest<SubmitRecommendationMessageResultDto>`.
- Change `IRecommendationConversationService.SendMessageAsync` to return the same result DTO or a `Guid` session id.
- Update `/api/customer/recommendations/messages` to return the new result shape.
- Change `SubmitRecommendationTurnFeedbackCommand` to return either `Unit`/no value or a minimal result, not `RecommendationSessionDto`.
- Update `/api/customer/recommendations/sessions/{sessionId}/turns/{turnId}/feedback` to return `204 NoContent` unless a minimal result is chosen.

### RecommendationConversationService

- Remove `previousSession` loading.
- Remove `BuildSessionDto`.
- Remove transient response-to-turn mapping helpers.
- Keep orchestration responsibilities only:
  - load or create `ChatSession`
  - create `AgentRun`
  - create MAF agent
  - restore `AgentSession`
  - run the agent with the user `ChatMessage`
  - validate non-empty assistant content as today
  - serialize and persist agent session state
  - complete `AgentRun`
  - record diagnostics
  - persist deterministic recommendation output groups
  - save once through unit of work
  - return the session id

### Projection

- Keep `RecommendationSessionQueryService.GetSessionAsync` as the only source for `RecommendationSessionDto`.
- Ensure it projects visible turns from persisted `AgentMessages`.
- Visible messages should remain user/assistant text messages only.
- System, tool-call, tool-result, native diagnostics, and internal framework messages should stay hidden from the public API.
- Recommendation groups should attach to visible assistant messages through `AgentRunId`, as they do today.

### Frontend

- Update `customer-api-client` so `submitRecommendationRequest` returns `{ sessionId }`.
- Update feedback API client to expect `void` or minimal result.
- Update recommendation React Query hooks:
  - on message submit success, invalidate session summaries
  - invalidate/refetch `recommendationSession(sessionId)`
  - for a brand-new chat, navigate to `/chat/$sessionId` using returned `sessionId`
  - do not `setQueryData` with mutation output
- Update feedback mutation:
  - invalidate/refetch current session
  - invalidate summaries if summary display can be affected

## Tests

- Update `SubmitRecommendationRequestHandlerTests` for the new result DTO.
- Update `RecommendationConversationServiceTests` to assert orchestration and persistence effects instead of returned transcript shape.
- Add or adjust query-service tests proving `RecommendationSessionDto` comes from persisted `AgentMessages`.
- Update feedback handler tests for no-content/minimal result behavior.
- Update frontend hook/page tests for invalidation and navigation behavior.
- Do not run eval tests for this change unless explicitly requested.

## Assumptions

- No database migration is required.
- The existing GET session endpoint remains the public source for full session state.
- The command response only needs to unblock frontend navigation after creating a new session.
- Eval tests are intentionally not part of normal validation for this refactor.
