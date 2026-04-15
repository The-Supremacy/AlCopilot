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

The system SHALL return recommendation-chat assistant responses as structured recommendation groups plus conversational prose.

**Scenario: Recommendation reply includes machine-readable groups**

- When the assistant returns recommendation results
- Then the system SHALL include machine-readable recommendation group data for stable customer-portal rendering

**Scenario: Recommendation reply includes conversational explanation**

- When the assistant returns recommendation results
- Then the system SHALL include prose that explains the recommendation outcome in customer-facing language

### Requirement: Limited Read-Only Semantic Kernel Tool Calling

The system SHALL restrict the first recommendation tool-calling surface to read-only helpers.

**Scenario: Read-only tool call can be used during recommendation**

- When the recommendation flow invokes an allowed read-only helper through Semantic Kernel
- Then the system SHALL allow the helper result to contribute to the generated recommendation response

**Scenario: Recommendation flow does not allow model-owned writes**

- When the recommendation flow runs with Semantic Kernel tool calling enabled
- Then the system SHALL keep persistence and profile mutation outside model-owned tool execution
