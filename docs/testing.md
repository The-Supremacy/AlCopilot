# AlCopilot Testing

## Purpose

This document is the thin testing index for AlCopilot.
It summarizes the testing philosophy and links to detailed backend and frontend testing guides.

---

## Testing Philosophy

- Prefer the lowest test level that gives enough confidence for the behavior under change.
- Keep architecture verification separate from behavior verification.
- Keep host-level integration tests targeted and higher-signal than module-owned tests.
- Use real infrastructure where persistence or infrastructure behavior is the subject under test.

---

## Detailed Testing Guides

| Area                      | Detailed guide                         |
| ------------------------- | -------------------------------------- |
| Backend testing strategy  | [testing/server.md](testing/server.md) |
| Frontend testing strategy | [testing/web.md](testing/web.md)       |

---

## Related Guidance

- [constitution.md](constitution.md) — thin governance index
- [architecture.md](architecture.md) — thin architecture index
