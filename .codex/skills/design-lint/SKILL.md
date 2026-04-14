---
name: design-lint
description: Lint a portal DESIGN.md for low-drift invariants-only compliance and report what should move to OpenSpec or ADRs.
license: MIT
---

Lint `web/apps/<portal>/DESIGN.md` against the project's design-doc guardrails.

## Purpose

`DESIGN.md` should remain a durable UI invariants document.
This skill checks for drift into feature behavior specs and reports corrective actions.

## Use This Skill When

- before `/opsx:apply` on UI-affecting work
- after updating a portal `DESIGN.md`
- during PR review to keep design docs stable and low-churn

## Checks

### 1) Invariant Scope

Verify the document is primarily stable UI guidance:

- global layout invariants
- IA/navigation invariants
- template-level patterns
- interaction/feedback principles
- accessibility and responsiveness baseline

### 2) Drift Signals

Flag content that likely belongs elsewhere:

- step-by-step feature workflows
- API behavior details
- acceptance criteria language (Given/When/Then)
- detailed feature state machines tied to one capability

### 3) Guardrail Presence

Verify anti-drift statements remain present:

- Do not document feature workflows here.
- When behavior changes, update OpenSpec.
- When global UI invariants change, update this DESIGN.md.

### 4) Cross-Artifact Routing

For flagged items, classify destination:

- OpenSpec: behavior/flow/acceptance criteria
- ADR: architecture/workflow decision record
- backlog: future idea not yet stable

## Output Format

Return a concise lint report:

1. Result: PASS or FAIL
2. Findings: list of drift issues with line references when possible
3. Suggested Moves: where each issue should be captured (OpenSpec/ADR/backlog)
4. Minimal Patch Guidance: high-level edits needed to make DESIGN.md compliant

## Guardrails

- Do not rewrite implementation code.
- Keep recommendations focused on doc quality and artifact boundaries.
- If no issues are found, report PASS explicitly and note residual risk if any.
