# Server Workflow And Quality

## Purpose

This document describes backend-specific workflow and quality expectations for AlCopilot.
Use it together with the global constitution and the server architecture and testing guides.

---

## Design Expectations

- Preserve modular-monolith boundaries.
- Keep module ownership clear in code, APIs, and tests.
- Use contracts for cross-module interaction rather than implementation references.
- Keep domain logic in aggregates and domain services rather than handlers.
- Avoid dead code and speculative backend abstractions until a real approved use case exists.

---

## Testing Expectations

- Add the cheapest test that proves the changed backend behavior.
- Keep module-owned behavior in module test projects.
- Use host tests for host-owned or cross-module risks.
- Use Testcontainers with real PostgreSQL for persistence and infrastructure integration tests.

---

## Change Management Expectations

- Update architecture tests when a change affects boundaries or conventions.
- Record significant architecture or workflow decisions as ADRs.
- Sync accepted ADRs into the detailed backend guidance that changed, not just into the root indexes.
- Keep `docs/domain.md` and `docs/modules/*.md` business-facing only; do not use them for architecture guidance, implementation details, endpoint inventory, persistence details, or acceptance criteria.

---

## Related Guidance

- [../domain.md](../domain.md)
- [../architecture/server.md](../architecture/server.md)
- [../testing/server.md](../testing/server.md)
