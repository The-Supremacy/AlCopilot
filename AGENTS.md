# AlCopilot — Agent Instructions

## Project

AlCopilot is an AI-powered drinks suggestion platform built as a modular monolith.
Read [docs/architecture.md](docs/architecture.md) for the full architecture, tech stack, and design decisions.

## Plan-First Default

- Always present a plan before implementing anything non-trivial.
- Break work into small, reviewable steps.
- Get explicit approval before proceeding with implementation.
- Reference architecture docs for design decisions — don't reinvent or contradict them.

## Area-Specific Conventions

Each area has its own `AGENTS.md` with detailed conventions:

- [server/AGENTS.md](server/AGENTS.md) — .NET backend conventions
- [web/AGENTS.md](web/AGENTS.md) — Frontend conventions
- [deploy/AGENTS.md](deploy/AGENTS.md) — Infrastructure conventions
- [docs/AGENTS.md](docs/AGENTS.md) — Documentation standards
- [.github/workflows/AGENTS.md](.github/workflows/AGENTS.md) — CI/CD conventions
