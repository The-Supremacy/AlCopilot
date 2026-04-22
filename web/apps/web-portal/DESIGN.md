# AlCopilot Web Portal Design Guide

## Purpose and Audience

This guide defines the persistent UI and information-architecture baseline for the customer web portal.
The primary audience is signed-in customers using AlCopilot to manage their bar, express taste preferences, and chat for drink suggestions.
Use this as the long-lived source of truth for customer portal layout and navigation decisions that are not tied to one OpenSpec change.

## Scope and Non-Scope

This document contains slow-changing UI invariants only.
It intentionally excludes feature-specific workflow logic, API behavior, and acceptance criteria.

Do not document feature workflows here.
When behavior changes, update OpenSpec.
When global UI invariants change, update this DESIGN.md.

## Global Shell Invariants

- Shell model: chat-first application shell with a persistent session-history rail on desktop.
- Header invariants:
  - portal title or brand mark
  - compact account and session affordances
  - contextual entry points to `My Bar` and `Preferences`
- Desktop shell keeps the current chat session central and treats supporting profile areas as secondary navigation destinations.
- Mobile shell favors a compact top bar with drawer access to chat history, primary navigation, and account/session controls.
- Footer policy: optional and minimal.
- Default landing page: Chat.
- Primary page action placement: inline with the active workspace header rather than in a global command bar.

## Information Architecture Invariants

- Top-level navigation remains stable and customer-task oriented:
  - Chat
  - My Bar
  - Preferences
  - History
  - Account
- The `Account` area is lightweight and session-oriented; identity-account administration remains delegated to the identity provider rather than becoming a portal-owned management surface.
- Session history is a first-class navigation surface rather than a hidden utility.
- Chat remains the primary destination and emotional center of the portal.
- `My Bar` and `Preferences` are durable supporting areas that feed recommendation quality rather than separate product silos.
- Navigation should feel personal and task-oriented rather than operational or admin-like.
- Breadcrumbs are optional and should be used sparingly because the portal is intentionally shallow.

## Page Template Invariants

- Chat template: wide centered conversation workspace with the message history as the dominant surface and a composer pinned to the lower edge of the viewport or workspace.
- Sidebar template: session history, account state, and secondary navigation may share one rail when screen width allows.
- Form template: `My Bar` and `Preferences` use focused edit surfaces with clear save feedback and low-friction search-first selection controls rather than overwhelming full lists by default.
- Account template: if present, it focuses on session state, sign-out, and links out to the identity-provider-managed account surface rather than in-portal credential editing.
- History template: previous sessions appear as lightweight revisit entries rather than dense operational tables.
- Empty states should be encouraging and guided, especially before the user has set preferences or bar inventory.
- Template definitions here describe layout patterns only, not feature flow steps.

## Interaction and Feedback Invariants

- Global feedback pattern: inline guidance plus toast confirmation.
- Recommendation results should combine conversational explanation with stable grouped summaries that stay easy to scan, especially through simple available-now versus restock-oriented presentation.
- Signed-in gating uses a consistent local sign-in-required state before redirecting to the identity provider.
- Potentially sensitive profile edits such as prohibited ingredients should use clear plain-language confirmations and summaries.
- The portal should keep users oriented about whether a recommendation is available now or better framed as a restock candidate without relying on chat prose alone.

## Visual and Density Principles

- Visual tone: warm, confident, and guidance-oriented.
- Data density baseline: comfortable.
- The customer portal should feel more inviting and expressive than the management portal while still staying grounded and readable.
- The chat workspace should remain visually dominant over supporting controls.
- User and assistant turns should be visually distinct, with user turns right-aligned and assistant turns left-aligned to preserve conversational orientation.
- Reuse shared brand foundations from workspace packages instead of creating a separate portal-local visual identity system.

## Accessibility and Responsive Baseline

- Desktop behavior keeps session history visible when space allows.
- Mobile behavior moves session history and secondary navigation into a drawer or sheet.
- Recommendation groupings must remain understandable without color alone.
- Primary chat, save, and revisit actions must be keyboard reachable with visible focus affordances.
- The portal must remain usable before profile setup is complete and should not assume wide-screen chat layouts.

## Deferred Items and Future Considerations

- Anonymous customer access is deferred.
- Rich catalog-browsing experiences are deferred until recommendation-first flows are established.
- Voice, image, or multimodal chat affordances are future considerations rather than current invariants.
- Real-time collaborative or social recommendation features are future backlog concerns, not shell invariants.

## Change Log and Decision Notes

- 2026-04-14: Initial customer web portal design guide created for the first signed-in recommendation-focused portal baseline.
- 2026-04-21: Refined the chat shell toward a centered conversation workspace with a pinned composer, search-first ingredient pickers, mobile drawer utility consolidation, and simpler grouped recommendation summaries.
- 2026-04-22: Recommendation turns render lightweight emphasis and bullet formatting from assistant prose, and grouped summaries use softer available-now versus restock language.
