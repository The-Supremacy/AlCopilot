## MODIFIED Requirements

### Requirement: Structured Recommendation Responses

The system SHALL return recommendation-chat assistant responses as structured recommendation groups plus conversational prose grounded in a deterministic recommendation run context.

#### Scenario: Recommendation reply includes machine-readable groups

- **WHEN** the assistant returns recommendation results
- **THEN** the system SHALL include machine-readable recommendation group data for stable customer-portal rendering

#### Scenario: Recommendation reply includes conversational explanation

- **WHEN** the assistant returns recommendation results
- **THEN** the system SHALL include prose that explains the recommendation outcome in customer-facing language

#### Scenario: Recommendation run context is bar-aware

- **WHEN** the system prepares recommendation narration for a customer with owned ingredients
- **THEN** the model-visible recommendation run context SHALL include owned ingredient names
- **AND** it SHALL identify which grouped drinks can be made now versus which require missing ingredients

### Requirement: Limited Read-Only Tool Calling

The system SHALL keep recommendation execution bounded so that model-owned execution remains read-only and persistence stays outside model-controlled actions.

#### Scenario: Read-only recipe lookup can contribute to a recommendation response

- **WHEN** recommendation narration needs exact recipe details for a known drink
- **THEN** the recommendation flow SHALL allow a read-only recipe lookup tool result to contribute to the generated recommendation response

#### Scenario: Recommendation flow does not allow model-owned writes

- **WHEN** the recommendation flow runs with recommendation tools enabled
- **THEN** the system SHALL keep persistence and profile mutation outside model-owned execution

#### Scenario: Tool usage is recorded on assistant turns

- **WHEN** the recommendation agent calls an allowed read-only tool during recommendation generation
- **THEN** the persisted assistant turn SHALL record the tool invocation metadata alongside the generated response

## ADDED Requirements

### Requirement: Recommendation Runtime Is Observable

The system SHALL emit recommendation-runtime traces that help developers understand recommendation execution without exposing private model reasoning.

#### Scenario: Recommendation invocation emits agent and model traces

- **WHEN** a recommendation message is processed
- **THEN** the runtime SHALL emit spans for recommendation agent invocation and model/chat execution through the existing OpenTelemetry pipeline

#### Scenario: Tool execution emits trace data

- **WHEN** the recommendation agent invokes the read-only recipe lookup tool
- **THEN** the runtime SHALL emit trace data for that tool execution through the existing OpenTelemetry pipeline

#### Scenario: Deterministic run-context assembly is traceable

- **WHEN** the recommendation module assembles the deterministic run context before narration
- **THEN** the runtime SHALL emit a trace span for that run-context assembly step
