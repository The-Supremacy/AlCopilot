# AlCopilot Management Portal Design Guide

## Purpose and Audience

This guide defines the persistent UI and information-architecture baseline for the management portal.
The primary audience is catalog managers and operators performing data curation, import actualization, and operational review workflows.
Use this as the long-lived source of truth for management layout and navigation decisions that are not tied to one OpenSpec change.

## Scope and Non-Scope

This document contains slow-changing UI invariants only.
It intentionally excludes feature-specific workflow logic, API behavior, and acceptance criteria.

Do not document feature workflows here.
When behavior changes, update OpenSpec.
When global UI invariants change, update this DESIGN.md.

## Global Shell Invariants

- Shell model: left sidebar plus top header.
- Header invariants:
  - portal title
  - environment badge
  - placeholder user menu slot (auth behavior deferred)
- Footer policy: optional/minimal.
- Default landing page: Dashboard.
- Primary page action placement: top-right area of the page header.

## Information Architecture Invariants

- Top-level navigation sections are stable and capability-oriented:
  - Dashboard
  - Catalog
  - Imports
  - Audit
- Navigation is designed for operator workflows, not end-user discovery journeys.
- Breadcrumbs are optional and appear only when hierarchy depth warrants orientation support.

## Page Template Invariants

- List template: operational lists with consistent filter, sort, status, and action affordance zones.
- Detail template: primary object context at top, supporting sections in predictable order.
- Workspace template: action-oriented pages with clear primary/secondary action separation.
- Template definitions here describe layout patterns only, not feature flow steps.

## Interaction and Feedback Invariants

- Global feedback pattern: inline status plus toast confirmation.
- Destructive or high-impact actions require explicit confirmation affordances.
- Status communication should favor clear, plain-language operator-facing messaging.

## Visual and Density Principles

- Visual tone: clean utilitarian.
- Data density baseline: comfortable-compact.
- Keep visual hierarchy focused on scanability, status clarity, and operational confidence.
- Reuse shared primitives and avoid ad hoc visual variants without a documented global reason.

## Accessibility and Responsive Baseline

- Navigation behavior by breakpoint:
  - desktop: persistent sidebar
  - mobile/tablet: drawer navigation
- Primary actions must be keyboard reachable with visible focus affordances.
- Status/warning/success messaging must not rely on color alone.
- Layout must maintain readability at common desktop widths and remain usable on smaller breakpoints.

## Deferred Items and Future Considerations

- Authentication behavior and full user profile flows remain deferred.
- Dashboard operational metrics such as suggestion acceptance/decline are future product backlog/OpenSpec concerns, not design invariants yet.
- Expanded branding and broader design-system evolution are deferred until explicitly prioritized.

## Change Log and Decision Notes

- 2026-04-10: Initial management portal design guide baseline created
- 2026-04-10: Refined to low-drift invariant-only model with explicit scope boundaries and governance alignment
