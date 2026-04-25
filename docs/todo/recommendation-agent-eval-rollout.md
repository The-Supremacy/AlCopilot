# Recommendation Agent Eval Rollout

## Purpose

This document is a working plan for introducing Agent Framework evaluation coverage to the recommendation runtime.
It is intentionally implementation-oriented and may change as the team learns from the first rollout.

---

## Goal

Add a small, explicit evaluation suite that runs the real recommendation agent against the real recommendation runtime and a real configured LLM.

The suite should validate prompt-level behavior that is difficult to prove with unit or regression tests alone.

---

## Target Scope

The first rollout should cover a small number of high-value prompts and avoid broad evaluator complexity.

The initial suite should prove:

- descriptor-led recommendation works through the real agent path
- drink-details prompts handle normalization and typo recovery
- ingredient-constrained prompts can succeed without unnecessary tool usage
- prohibited-ingredient conflicts are handled safely
- the model does not invent unsupported drinks in catalog-grounded scenarios
- follow-up prompts in an existing session resolve prior recommendations correctly
- follow-up corrections can override a prior recommendation without recommending newly excluded ingredients
- disliked ingredients influence recommendations without being treated as hard prohibitions
- follow-up "what else" prompts can move to another suitable option in the same session

---

## Proposed Deliverables

### 1. Dedicated Eval Corpus

Status: complete.

Create a dedicated checked-in corpus file under the recommendation test project.

Suggested location:

- `server/tests/AlCopilot.Recommendation.EvalTests/TestData/maf-eval/recommendation-agent-eval.json`

Each case should include:

- name
- prompt
- profile fixture or explicit profile state
- catalog fixture or seeded catalog reference
- expected response fragments
- forbidden response fragments
- expected tool names
- forbidden tool names
- max tool call count
- optional repetition count

Session cases should include:

- name
- shared profile fixture
- ordered turns with the same response, tool, and max-tool expectations as single-turn cases

A fast corpus validation test should also guard the checked-in JSON before live model calls run.

### 2. Dedicated Eval Seed Strategy

Status: complete for the first rollout.

Create a stable recommendation seed path for evaluation.

This should avoid accidental changes from ordinary local catalog edits.

Recommended direction:

- a dedicated import or preserved seed source for evaluation — implemented as `RecommendationAgentEvalSeedCatalog`
- a dedicated profile fixture set for owned, disliked, and prohibited ingredients — implemented through corpus-owned profile ingredient names resolved against the eval seed

### 3. Recommendation Agent Eval Harness

Status: complete for the recommendation-project seam.

Add a small eval harness around the recommendation runtime.

The harness should:

- resolve the real recommendation DI graph
- build the real agent from `RecommendationNarratorAgentFactory`
- seed the required catalog and profile state
- run the agent with real session-backed state
- reuse the same session across ordered eval turns for follow-up coverage
- capture the final response and tool metadata for evaluator assertions

### 4. Initial Local Evaluator Checks

Status: complete.

Start with local behavior checks rather than judge-style evaluators.

The first checks should cover:

- expected text appears
- forbidden text does not appear
- expected tool call occurs
- no tool call occurs when not needed
- tool call count stays within a limit
- repeated identical tool calls do not occur

The suite also validates corpus shape without requiring an LLM:

- unique case names
- profile ingredients resolve against the eval seed catalog
- session cases include at least two turns
- expected and forbidden tool names are known

### 5. Dedicated Eval Test Project

Status: complete.

Keep live LLM evals in a dedicated project:

- `server/tests/AlCopilot.Recommendation.EvalTests`

The project still uses categories such as `Eval` and `EvalCorpus` for filtering within the eval project, but running the project is the explicit signal for live LLM calls.
It remains in the main solution for restore/build coverage.
Normal development should run the focused unit project by path, while integration and LLM eval projects should run when affected logic needs that coverage or before the actual commit.

---

## Suggested First Cases

The initial corpus should stay small and focused.

Suggested starting prompts:

1. `I'd like to have a fun night. Something strong but sweet will do.`
2. `How do I make a Dark n Stormy?`
3. `How do I make a Dark 'n Stormi?`
4. `I have rum and ginger beer. What can I make?`
5. `I want something sweet with Campari.`

If the first rollout is stable, add:

6. `I have gin, lime, and prosecco. What feels light and celebratory?`
7. `I want something classy and bitter.`
8. `Recommend something bold and sweet with rum.`

Status: prompts 1-8 are now represented in the eval corpus, plus a focused unsupported-drink case, a disliked-Campari preference case, a multi-turn follow-up recipe case, a multi-turn "what else" alternative case, and a multi-turn no-Campari correction case.

---

## Implementation Plan

### Phase 1. Create Stable Inputs

- [x] create the dedicated eval corpus file
- [x] define stable profile fixtures
- [x] define stable catalog or import fixtures
- [x] document how the eval seed differs from ordinary dev data

### Phase 2. Add Harness

- [x] add a recommendation agent eval harness in the recommendation test project
- [x] initialize the real recommendation agent from `RecommendationNarratorAgentFactory`
- [x] confirm the harness can execute a single prompt against the configured LLM
- [x] confirm the harness can execute ordered prompts against one persisted session

### Phase 3. Add First Eval Tests

- [x] add three to five explicit eval cases
- [x] implement local evaluator assertions only
- [x] verify response and tool-call capture works reliably

### Phase 4. Tune Stability

- [ ] add repetition for prompts that are known to vary
- [x] tighten prompt instructions or runtime behavior where the suite exposes noise
- [x] reduce redundant tool usage before growing the suite

### Phase 5. Expand Coverage

- [x] add more descriptor-led prompts
- [x] add hybrid ingredient-plus-descriptor prompts
- [x] add conflict and forbidden-ingredient prompts
- [x] add no-invented-drink coverage for catalog-grounded prompts
- [x] add follow-up coverage for a session that references the prior recommendation
- [x] add follow-up coverage for a latest-turn ingredient exclusion
- [x] add dislike-not-prohibited preference coverage
- [x] add follow-up alternative coverage for "what else" prompts
- [x] add fast corpus validation coverage for eval data drift

### Phase 6. Consider Richer Evaluators

- [x] evaluate whether Agent Framework evaluator APIs add useful built-in scoring
- [ ] add judge-style scoring only after the local checks are trusted
- [ ] keep judge-style scoring supplemental rather than the only signal

---

## Open Questions

- Should the eval suite use a dedicated database and seeded catalog per run, or a smaller in-memory or disposable fixture path where possible? Current answer: use a test-owned in-memory catalog/profile seam while running the real recommendation agent runtime.
- Should tool-call assertions be based on persisted recommendation tool metadata, execution trace metadata, or direct harness capture? Current answer: use persisted recommendation tool metadata from the assistant turn.
- Should the first rollout treat exact recommended drink names as strict assertions, or use acceptable sets for descriptive prompts? Still open. Current corpus uses strict fragments for a small stable seed.
- Should the eval suite live only in the recommendation test project, or later gain a host-level variant for full API-path smoke coverage?

---

## Non-Goals

The first rollout should not:

- replace existing regression tests
- replace lower-level deterministic tests
- depend on the public HTTP API
- require judge-style evaluators on day one
- become part of the default fast PR loop

---

## Success Criteria

The first rollout is successful when:

- the team can run a small explicit suite against the real recommendation agent and real LLM
- the suite catches prompt-level failures that lower-level tests would miss
- the corpus can grow without prompt drift or accidental data changes
- the harness remains clear about whether failures come from deterministic preparation, tool usage, or model narration
