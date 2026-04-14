---
name: design-new
description: Create a new portal DESIGN.md document using the repo's low-drift invariants-only format. Use when a new portal is planned and UI design guidance must be established before UI implementation work.
license: MIT
---

Create a new portal design guide at `web/apps/<portal>/DESIGN.md`.

## Purpose

`DESIGN.md` is a persistent UI invariants document.
It is intentionally low-drift and does not replace OpenSpec behavior specs or ADR decision records.

## Use This Skill When

- a new portal app is being introduced
- a portal has no `DESIGN.md` yet
- UI-affecting work is planned and requires design guidance before `/opsx:apply`

## Inputs To Gather From Developer

Collect enough information to define durable invariants:

1. Portal identifier and target path (`web/apps/<portal>/DESIGN.md`)
2. Portal purpose and primary audience
3. Global shell model (header/sidebar/footer stance)
4. Top-level IA/navigation sections
5. Default landing page
6. Global action placement and feedback pattern
7. Visual tone and density baseline
8. Responsive behavior baseline
9. Deferred/future considerations

If information is missing, ask focused questions and prefer stable defaults over speculative detail.

## Output Format

Create the document with these sections:

1. Purpose and Audience
2. Scope and Non-Scope
3. Global Shell Invariants
4. Information Architecture Invariants
5. Page Template Invariants
6. Interaction and Feedback Invariants
7. Visual and Density Principles
8. Accessibility and Responsive Baseline
9. Deferred Items and Future Considerations
10. Change Log and Decision Notes

## Hard Guardrails

- Include only slow-changing UI invariants.
- Do not include feature-specific workflows, API behavior, or acceptance criteria.
- Add explicit anti-drift statements:
  - "Do not document feature workflows here."
  - "When behavior changes, update OpenSpec."
  - "When global UI invariants change, update this DESIGN.md."

## Steps

1. Read current web guidance:
   - `docs/architecture/web.md`
   - `docs/constitution/web.md`
   - `openspec/config.yaml`
2. Confirm target portal path and whether the file already exists.
3. Gather the required inputs listed above.
4. Draft `DESIGN.md` using the required section format and guardrails.
5. Validate anti-drift:
   - no step-by-step flow logic
   - no API contract behavior
   - no Given/When/Then acceptance language
6. Sync references if needed:
   - ensure `docs/architecture/web.md` portal design guide list includes the new file
7. Summarize what was created and which assumptions were used.
