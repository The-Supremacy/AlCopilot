# AlCopilot — Copilot Compatibility Notes

GitHub Copilot remains supported in this repository, but it is no longer the primary custom workflow surface.

The shared source of truth for agent guidance is:

- [AGENTS.md](../AGENTS.md)
- [docs/constitution.md](../docs/constitution.md)
- [docs/architecture.md](../docs/architecture.md)

## Expectations

- Follow the shared project guidance from `AGENTS.md` and the core docs.
- Prefer plan-first collaboration for non-trivial work.
- Treat `.codex/skills/` as the canonical home for local workflow skills.
- Do not assume `.github/skills/`, `.github/prompts/`, or `.github/agents/` exist for local workflow support.

## What Stays In `.github/`

- `workflows/` for GitHub Actions
- `hooks/` for GitHub and agent-session enforcement that still provides value
- compatibility files that help GitHub-side tooling without duplicating project policy

## Hooks

Hooks in `.github/hooks/` enforce security and audit rules for supported agent workflows.
