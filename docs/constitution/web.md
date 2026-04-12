# Web Workflow And Quality

## Purpose

This document describes frontend-specific workflow and quality expectations for AlCopilot.
Use it together with the global constitution and the web architecture and testing guides.

---

## Design Expectations

- Build user-facing flows with clear semantics and stable structure.
- Prefer maintainable composition over tightly coupled component trees.
- Keep frontend state responsibilities clear between router state, server state, and client-only state.
- Maintain persistent portal design guides at the portal app boundary (`web/apps/<portal>/DESIGN.md`) for layout, navigation, content ownership, and UI composition rules.
- For new portal UI work, use the accepted frontend stack by default rather than inventing a one-off styling approach.
- Tailwind CSS and shadcn/ui are the required default for new portal UI unless an ADR or explicit approved exception says otherwise.
- Treat custom CSS-only component systems as an exception path that must be justified, not as an acceptable starting baseline.
- In this repo, shadcn/ui should be visible as `components.json` plus local UI primitives under `src/components/ui/`; do not describe it as a runtime package dependency requirement.
- If Zustand is not needed for a particular change, omit it intentionally, but do not replace the approved stack with unrelated state libraries.
- Route-level components belong in `src/pages/`, and oversized pages should be split into feature sections plus colocated page hooks before they become monoliths.

---

## Testing Expectations

- Add the cheapest frontend test that proves the changed behavior.
- Prefer component and feature-level tests before expanding E2E coverage.
- Keep Playwright coverage narrow and focused on critical flows.
- Build UI so it can be meaningfully automated later, even when a specific flow does not yet have browser coverage.

---

## Change Management Expectations

- Record significant frontend architecture or workflow decisions as ADRs.
- Sync accepted ADRs into the detailed frontend guidance that changed, not just into the root indexes.
- For UI-affecting changes, update the affected portal `DESIGN.md` guides before implementation (`/opsx:apply`) starts.
- OpenSpec artifacts for UI-affecting changes must reference the affected portal `DESIGN.md` guides and stay aligned with them before archive.
- UI-affecting OpenSpec changes should state whether the accepted frontend stack is being used directly or whether an approved exception exists.
- Use `/design:new` for first-time portal design guide creation, `/design:change` for invariant-focused updates, and `/design:lint` before apply when design docs changed.

---

## Related Guidance

- [../architecture/web.md](../architecture/web.md)
- [../testing/web.md](../testing/web.md)
