---
name: adr-lint
description: Lint ADR files and ADR-linked guidance for status, structure, and sync drift. Use when reviewing architecture decision records for completeness or checking whether accepted ADRs are reflected in current guidance docs.
license: MIT
---

Lint ADRs and their downstream guidance sync.

## Purpose

This skill checks whether ADRs remain structurally valid, status-appropriate, and reflected accurately in current guidance.
It is the ADR equivalent of `design-lint`: a review tool, not a code mutation workflow.

## Use This Skill When

- reviewing a newly added or updated ADR
- checking whether accepted ADRs are synced into active docs
- auditing ADR hygiene during PR review
- spotting drift between decision records and architecture/testing/workflow guidance

## Checks

### 1) ADR Structure

Verify each ADR includes:

- title
- status
- date
- context
- decision
- reason
- consequences
- alternatives considered

Also verify optional `Supersedes` / `Superseded by` sections are used consistently when referenced.

### 2) Status Discipline

Flag status misuse:

- `Accepted` ADRs that describe obviously unimplemented or deferred behavior
- `Deferred` ADRs that do not state why they are deferred and what should trigger reconsideration
- `Rejected` ADRs that do not clearly explain why the option was rejected
- `Superseded` ADRs that do not point to a replacement

### 3) Sync Drift

For `Accepted` ADRs, check whether active guidance reflects the decision where appropriate:

- `docs/architecture/server.md` or `docs/architecture/web.md`
- `docs/testing/server.md` or `docs/testing/web.md`
- `docs/constitution/server.md` or `docs/constitution/web.md`
- root index docs only when navigation or globally active guidance changed
- area `AGENTS.md` files only when the guidance is truly area-local

Flag:

- accepted ADRs that changed active guidance but are not reflected
- guidance docs that imply behavior contradicting an accepted ADR
- deferred ADRs that are written as active behavior in current guidance

### 4) Thin-Doc Discipline

Verify root docs remain navigational and do not duplicate ADR bodies.
If a guidance doc repeats large chunks of an ADR instead of summarizing the active rule, flag it.

## Output Format

Return a concise lint report:

1. Result: PASS or FAIL
2. Findings: list issues with file references when possible
3. Sync Gaps: accepted ADRs that should update current guidance
4. Minimal Patch Guidance: high-level edits needed to restore ADR/doc alignment

## Guardrails

- Do not rewrite implementation code.
- Do not auto-create ADRs.
- Keep findings focused on ADR quality, status correctness, and sync drift.
- If no issues are found, report PASS explicitly and note any residual review risk.
