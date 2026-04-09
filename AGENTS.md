# AlCopilot — Agent Instructions

## Project

AlCopilot is an AI-powered drinks suggestion platform built as a modular monolith.
Read [docs/constitution.md](docs/constitution.md) for the thin project-wide governance index.
Read [docs/architecture.md](docs/architecture.md) for the thin project-wide architecture index.
Read [docs/testing.md](docs/testing.md) for the thin project-wide testing index.

## Area-Specific Conventions

Each area has its own `AGENTS.md` with detailed conventions:

- [server/AGENTS.md](server/AGENTS.md) — .NET backend conventions
- [server/tests/AGENTS.md](server/tests/AGENTS.md) — backend test placement and structure conventions
- [web/AGENTS.md](web/AGENTS.md) — Frontend conventions
- [deploy/AGENTS.md](deploy/AGENTS.md) — Infrastructure conventions
- [docs/AGENTS.md](docs/AGENTS.md) — Documentation standards
- [.github/workflows/AGENTS.md](.github/workflows/AGENTS.md) — CI/CD conventions

## Review Rule

Treat root docs as indexes.
Detailed architecture, workflow, and testing guidance should live once in the relevant area-specific documents rather than being duplicated in the root files.
