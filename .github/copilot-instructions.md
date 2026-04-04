# AlCopilot — Copilot Instructions

## Project Overview

AlCopilot is an AI-powered drinks suggestion platform.

- [docs/architecture.md](../docs/architecture.md) — full architecture, tech stack, and design decisions
- [AGENTS.md](../AGENTS.md) — agent workflow, roles, and area-specific conventions

## CRITICAL: No Vibe Coding

The developer builds first instances of each piece of functionality manually. Copilot assists with planning, explaining, and reviewing — but does NOT generate large blocks of ready-to-paste implementation code unless explicitly authorized via a SKILL file.

### SKILL Gate

- **Before generating any implementation code**, check if a SKILL file exists in `.github/skills/` that covers the task.
- **If a matching SKILL exists**: follow the SKILL's instructions to generate the implementation.
- **If NO matching SKILL exists**: explain the approach step-by-step with small illustrative snippets (5-15 lines max). Do NOT produce complete, ready-to-paste implementations.
- When in doubt, **ask** rather than generate.

### What IS Allowed Without a SKILL

- Answering questions about code, architecture, or tooling
- Explaining concepts with small illustrative snippets
- Reviewing existing code and suggesting improvements
- Generating configuration files (CI/CD, tooling config)
- Writing tests for existing code
- Fixing bugs when the fix is localized and clear
- Refactoring existing code (move, rename, restructure — not rewrite)

### What REQUIRES a SKILL

- Creating new module structures (endpoints, domain models, EF configurations)
- Implementing new features end-to-end
- Adding new integration patterns (message handlers, API clients)
- Creating reusable abstractions or base classes

### Plan-First Default

- Always present a plan before implementing anything non-trivial.
- Break work into small, reviewable steps.
- Get explicit approval before proceeding with implementation.
- Reference architecture docs for design decisions — don't reinvent or contradict them.

## Repository Structure

```
alcopilot/
├── .github/                      # CI/CD, Copilot config, SKILLs, hooks
│   ├── workflows/                # GitHub Actions pipelines (see workflows/AGENTS.md)
│   ├── skills/                   # SKILL files (created incrementally)
│   └── hooks/                    # Copilot hooks (enforcement)
├── deploy/                       # Infrastructure and deployment (see deploy/AGENTS.md)
├── docs/                         # Architecture and documentation (see docs/AGENTS.md)
├── server/                       # .NET backend (see server/AGENTS.md)
├── web/                          # Frontend (see web/AGENTS.md)
└── AGENTS.md                     # Agent workflow, roles, conventions index
```

## Conventions

- **Semantic commits only** — enforced by Husky + commitlint
- **Central Package Management** — all NuGet versions in `server/Directory.Packages.props`
- Area-specific conventions in `AGENTS.md` files throughout the repo

## Copilot Hooks

Hooks in `.github/hooks/` enforce security and audit rules for the Copilot coding agent and Copilot CLI.

**Active hooks:**

| Hook           | Script                     | What it does                                                                |
| -------------- | -------------------------- | --------------------------------------------------------------------------- |
| `preToolUse`   | `scripts/security-gate.sh` | Blocks destructive commands, protects `docs/architecture.md` and lock files |
| `postToolUse`  | `scripts/audit-log.sh`     | Logs all tool calls to `logs/copilot-audit.jsonl`                           |
| `sessionStart` | `scripts/log-session.sh`   | Logs session start to `logs/copilot-sessions.jsonl`                         |
