# Agent Eval Testing

## Purpose

This document describes the recommended Agent Framework evaluation approach for AlCopilot.
It focuses on slow, explicit, end-to-end agent validation against the real recommendation runtime rather than fast unit or integration checks.

---

## When To Use Agent Eval

Use Agent Eval when the behavior under review depends on the full agent path:

- the real `ChatClientAgent`
- the real `AIContextProvider` chain
- real recommendation tools
- real session-backed provider state
- real prompt instructions
- real LLM behavior

Agent Eval is especially useful for recommendation prompts where correctness depends on the interaction between deterministic preparation and model narration.

Examples include:

- descriptor-led recommendation prompts such as `bold, sweet, and boozy`
- typo-tolerant drink-details prompts such as `Dark 'n Stormi`
- ingredient-constrained recommendation prompts such as `I have rum and ginger beer`
- conflict prompts such as `I want something sweet with Campari`
- prompts where unnecessary or repeated tool calls are a real product risk

---

## What Agent Eval Is Not

Agent Eval does not replace lower-level tests.

Keep the normal backend layers for:

- intent resolution logic
- candidate-building logic
- semantic hit aggregation
- deterministic exclusions and ranking
- repository and persistence behavior

Agent Eval adds confidence at the end-to-end agent layer.

---

## Recommended Harness Shape

The preferred initialization seam is the recommendation agent runtime, not the public HTTP API.

For recommendation evaluation, start from:

- `RecommendationNarratorAgentFactory`
- the real provider pipeline
- the real recommendation tools
- the real recommendation collaborators needed for current runtime behavior

Avoid using the HTTP API as the first evaluation seam.

HTTP adds unrelated concerns such as:

- auth setup
- controller and serialization behavior
- API-specific persistence noise
- slower and less focused failure modes

Agent Eval should answer the question:
`Does the real recommendation agent behave correctly for this prompt and this seeded state?`

---

## Corpus Design

Recommendation Agent Eval should use a dedicated checked-in corpus file so prompts do not change accidentally during ordinary code edits.

The corpus should live in test-owned data files rather than inline test code when the suite starts growing.

Recommended corpus fields:

- case name
- prompt
- seeded customer profile state
- seeded catalog or catalog fixture reference
- expected response fragments
- forbidden response fragments
- expected recommended drink names
- forbidden recommended drink names
- expected tool names
- forbidden tool names
- max tool call count
- repetition count when stability matters
- notes about whether the case is strict or flexible

Use strict assertions for:

- typo recovery
- explicit drink-details requests
- prohibited-ingredient conflicts
- no-invented-drink requirements

Use softer assertions for:

- descriptive recommendation prompts
- prompts where multiple catalog drinks are acceptable

---

## When To Add A Case

Add an Agent Eval case when a bug or risk depends on the full prompt, model, tool, context-provider, and session path.
Good examples include:

- the model calls the wrong tool or repeats tool calls
- the model loses prior-turn context
- the model recommends a drink that deterministic context marked as prohibited or disliked
- the model invents unavailable catalog details
- prompt wording changes behavior that lower-level tests cannot observe

Do not add Agent Eval cases for behavior that can be proven more directly through deterministic tests.
Prefer lower-level tests for:

- request parsing and intent resolution
- candidate scoring and grouping
- semantic retrieval hit aggregation
- repository persistence and migrations
- DTO mapping and handler orchestration

When a new case is useful, keep it small and attach it to a specific risk.
Avoid broad prompt matrices unless repeated failures show that the extra runtime is buying real confidence.

---

## Seed Data Guidance

Recommendation Agent Eval should run against stable seeded recommendation data.

Use a dedicated import or seed source for evaluation so the catalog, descriptions, and profile expectations do not drift unintentionally.

The evaluation seed should make these cases stable:

- known strong and sweet drinks
- known bitter or classy drinks
- owned-ingredient prompts
- prohibited-ingredient prompts
- typo-tolerant drink-details prompts

Avoid using whatever mutable local development catalog happens to exist at runtime.

The current recommendation eval suite uses `RecommendationAgentEvalSeedCatalog`, a test-owned in-memory catalog that is intentionally smaller and more stable than ordinary dev data.
Corpus profile fixtures name owned, disliked, and prohibited ingredients, and the harness resolves those names against the seed before running the real recommendation agent.
This keeps prompt expectations independent from local catalog imports while still exercising the real recommendation runtime, context providers, tools, and session-backed conversation path.

---

## Evaluator Strategy

Start with simple local evaluator checks before introducing judge-style evaluators.
Keep local checks behind framework-native evaluator abstractions so the suite can later mix local checks and judge-style evaluators in the same run.

Recommendation evals use Agent Framework's `LocalEvaluator` and `FunctionEvaluator` for local checks.
Judge-style evaluators can be added later through the Agent Framework evaluation overloads that accept `Microsoft.Extensions.AI.Evaluation.IEvaluator`.

Good first checks include:

- expected drink name appears
- prohibited drink or ingredient does not appear
- expected drink is presented as a recommendation
- forbidden drink is not presented as a recommendation, while still allowing explanatory mentions such as "we'll skip Negroni"
- expected tool was called
- no tool was called
- tool call count stays below a limit
- repeated identical tool calls do not occur
- response acknowledges profile conflicts when required

These checks are high-signal and easier to trust than free-form judge scoring during the first rollout.

Later, the team may add richer evaluator layers for:

- groundedness
- relevance
- completeness
- consistency across repeated runs

---

## Execution Model

Recommendation Agent Eval is expected to be slow and to call a real LLM.

Keep it in a separate eval test project so ordinary unit-test project runs remain fast and deterministic.

Recommended safeguards:

- keep live evals in a dedicated project such as `AlCopilot.Recommendation.EvalTests`
- keep the eval project in the solution so solution restore/build still catches compile drift
- run unit, integration, and LLM eval projects deliberately based on the affected logic
- mark tests with a dedicated category such as `Eval` for filtering inside the eval project
- let the eval project fail clearly when no real LLM is available
- keep the first suite intentionally small

This protects day-to-day development speed while still giving the team a realistic end-to-end agent signal.
Running the eval test project is the explicit signal to call the configured LLM provider.

---

## Running Recommendation Agent Eval Locally

The recommendation eval suite lives in `server/tests/AlCopilot.Recommendation.EvalTests`.
For normal development, run the focused unit or integration test project you are working in.
Run integration and LLM eval projects when the affected logic needs that coverage or before the actual commit.

Fast corpus validation does not call an LLM and can run by itself:

```bash
dotnet test server/tests/AlCopilot.Recommendation.EvalTests/AlCopilot.Recommendation.EvalTests.csproj --filter "Category=EvalCorpus"
```

Live eval execution calls the configured LLM provider:

```bash
dotnet test server/tests/AlCopilot.Recommendation.EvalTests/AlCopilot.Recommendation.EvalTests.csproj --filter "Category=Eval"
```

The harness reads `Recommendation:Ollama` from `server/src/AlCopilot.Host/appsettings.Development.json` by default.
Override the provider for a specific run with:

```bash
ALCOPILOT_RECOMMENDATION_AGENT_EVAL_OLLAMA_ENDPOINT=http://localhost:11434 \
ALCOPILOT_RECOMMENDATION_AGENT_EVAL_OLLAMA_MODEL=gemma4:e4b \
dotnet test server/tests/AlCopilot.Recommendation.EvalTests/AlCopilot.Recommendation.EvalTests.csproj --filter "Category=Eval"
```

When diagnosing a failure, inspect the xUnit output for the case name, model response, and recorded tool calls.

---

## Suggested First Prompt Set

Start with a small set of prompts that exercise clearly different behaviors:

- `I'd like to have a fun night. Something strong but sweet will do.`
- `How do I make a Dark n Stormy?`
- `How do I make a Dark 'n Stormi?`
- `I have rum and ginger beer. What can I make?`
- `I want something sweet with Campari.`

After the first suite is stable, extend it with:

- descriptor-plus-ingredient prompts
- conflict prompts with prohibited ingredients
- prompts that previously caused repeated tool calls
- prompts that risk invented non-catalog drinks

---

## Relationship To Existing Tests

Recommendation regression tests remain the main fast safety net for the recommendation runtime.

Use them for deterministic and semi-deterministic behavior checks that should stay in the normal backend test loop.

Use Agent Eval for a smaller number of high-value prompts where the real agent harness must be exercised as a whole.

Together, these layers provide:

- fast confidence in core logic
- slower but more realistic confidence in the final agent behavior

---

## Related Guidance

- [server.md](server.md) — detailed backend testing strategy
- [../ai/recommendation.md](../ai/recommendation.md) — recommendation runtime and RAG guidance
- [../architecture/server.md](../architecture/server.md) — backend runtime architecture
