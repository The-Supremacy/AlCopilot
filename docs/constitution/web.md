# Web Workflow And Quality

## Purpose

This document describes frontend-specific workflow and quality expectations for AlCopilot.
Use it together with the global constitution and the web architecture and testing guides.

---

## Design Expectations

- Build user-facing flows with clear semantics and stable structure.
- Prefer maintainable composition over tightly coupled component trees.
- Keep frontend state responsibilities clear between router state, server state, and client-only state.

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

---

## Related Guidance

- [../architecture/web.md](../architecture/web.md)
- [../testing/web.md](../testing/web.md)
