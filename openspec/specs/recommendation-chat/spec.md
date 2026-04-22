# Spec: Recommendation Chat

### Requirement: Start And Continue Recommendation Chat Sessions

The system SHALL allow an authenticated customer to create and continue persisted recommendation chat sessions.

**Scenario: First customer message creates a chat session**

- When an authenticated customer sends a first recommendation message without an existing session
- Then the system SHALL create a persisted chat session and store the resulting conversation turns

**Scenario: Existing session can be revisited**

- When an authenticated customer opens a previously saved recommendation session
- Then the system SHALL return the stored conversation history for that session in order

### Requirement: Deterministic Candidate Building Before Model Ranking

The system SHALL apply deterministic recommendation preparation before invoking the model.

**Scenario: Prohibited ingredients exclude drinks from candidates**

- When the customer profile contains prohibited ingredients
- Then the system SHALL exclude drinks containing those ingredients from the recommendation candidate set before model ranking

**Scenario: Disliked ingredients reduce ranking priority**

- When candidate drinks contain ingredients the customer marked as disliked
- Then the system SHALL treat those drinks as lower-priority candidates rather than automatically excluding them

**Scenario: Inventory split distinguishes available and near-miss drinks**

- When the system evaluates candidate drinks against the customer's owned ingredients
- Then the system SHALL separate recommendation results into drinks the customer can make now and drinks that require additional ingredients

### Requirement: Structured Recommendation Responses

The system SHALL return recommendation-chat assistant responses as structured recommendation groups plus conversational prose grounded in a deterministic recommendation run context.

**Scenario: Recommendation reply includes machine-readable groups**

- When the assistant returns recommendation results
- Then the system SHALL include machine-readable recommendation group data for stable customer-portal rendering

**Scenario: Recommendation reply includes conversational explanation**

- When the assistant returns recommendation results
- Then the system SHALL include prose that explains the recommendation outcome in customer-facing language

**Scenario: Recommendation run context is bar-aware**

- When the system prepares recommendation narration for a customer with owned ingredients
- Then the model-visible recommendation run context SHALL include owned ingredient names
- And it SHALL identify which grouped drinks can be made now versus which require missing ingredients

### Requirement: Limited Read-Only Tool Calling

The system SHALL keep recommendation execution bounded so that model-owned execution remains read-only and persistence stays outside model-controlled actions.

**Scenario: Read-only recipe lookup can contribute to a recommendation response**

- When recommendation narration needs exact recipe details for a known drink
- Then the recommendation flow SHALL allow a read-only recipe lookup tool result to contribute to the generated recommendation response

**Scenario: Recommendation flow does not allow model-owned writes**

- When the recommendation flow runs with recommendation tools enabled
- Then the system SHALL keep persistence and profile mutation outside model-owned tool execution

**Scenario: Tool usage is recorded on assistant turns**

- When the recommendation agent calls an allowed read-only tool during recommendation generation
- Then the persisted assistant turn SHALL record the tool invocation metadata alongside the generated response

### Requirement: Recommendation Runtime Is Observable

The system SHALL emit recommendation-runtime traces that help developers understand recommendation execution without exposing private model reasoning.

**Scenario: Recommendation invocation emits agent and model traces**

- When a recommendation message is processed
- Then the runtime SHALL emit spans for recommendation agent invocation and model/chat execution through the existing OpenTelemetry pipeline

**Scenario: Tool execution emits trace data**

- When the recommendation agent invokes the read-only recipe lookup tool
- Then the runtime SHALL emit trace data for that tool execution through the existing OpenTelemetry pipeline

**Scenario: Deterministic run-context assembly is traceable**

- When the recommendation module assembles the deterministic run context before narration
- Then the runtime SHALL emit a trace span for that run-context assembly step
