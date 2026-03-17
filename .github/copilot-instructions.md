# AlCopilot — Copilot Instructions

## Project Overview

AlCopilot is an AI-powered drinks suggestion platform. Read [docs/architecture.md](../docs/architecture.md) for the full architecture, tech stack, and design decisions. Read [docs/llm.md](../docs/llm.md) for the local LLM and vector database setup.

## CRITICAL: No Vibe Coding

This project enforces a disciplined, hands-on development workflow. The developer builds first instances of each piece of functionality manually. Copilot assists with planning, explaining, and reviewing — but does NOT generate large blocks of ready-to-paste implementation code unless explicitly authorized via a SKILL file.

### SKILL Gate

- **Before generating any implementation code**, check if a SKILL file exists in `.github/skills/` that covers the task.
- **If a matching SKILL exists**: follow the SKILL's instructions to generate the implementation.
- **If NO matching SKILL exists**: explain the approach step-by-step with small illustrative snippets (5-15 lines max). Do NOT produce complete, ready-to-paste implementations. Guide the developer to write it themselves.
- When in doubt, **ask** rather than generate.

### Plan-First Default

- Always present a plan before implementing anything non-trivial.
- Break work into small, reviewable steps.
- Get explicit approval before proceeding with implementation.
- Reference architecture docs for design decisions — don't reinvent or contradict them.

### What IS Allowed Without a SKILL (but with a permission check)

- Answering questions about code, architecture, or tooling
- Explaining concepts with small illustrative snippets
- Reviewing existing code and suggesting improvements
- Generating configuration files (CI/CD, Docker, tooling config)
- Writing tests for existing code
- Fixing bugs when the fix is localized and clear
- Refactoring existing code (move, rename, restructure — not rewrite)

### What REQUIRES a SKILL

- Creating new module structures (endpoints, domain models, EF configurations)
- Implementing new features end-to-end
- Adding new integration patterns (message handlers, API clients)
- Creating reusable abstractions or base classes

## Repository Structure

```
alcopilot/
├── .github/                      # CI/CD, Copilot instructions, SKILLs
│   ├── workflows/                # GitHub Actions pipelines
│   ├── instructions/             # Per-path Copilot instructions
│   └── skills/                   # SKILL files (created incrementally)
├── deploy/                       # Infrastructure and deployment
│   ├── helm/                     # Helm charts (future)
│   ├── flux/                     # Flux GitOps manifests (future)
│   └── terraform/                # IaC (future)
├── docs/                         # Architecture and documentation
├── server/                       # .NET backend
│   ├── src/
│   │   ├── AlCopilot.AppHost/    # Aspire orchestrator
│   │   ├── AlCopilot.ServiceDefaults/
│   │   ├── AlCopilot.Host/       # ASP.NET Core host + BFF (composes modules, runs workers)
│   │   ├── AlCopilot.Shared/     # Cross-cutting concerns
│   │   ├── AlCopilot.Catalog/    # Module
│   │   ├── AlCopilot.Catalog.Contracts/ # Module contracts (interfaces, DTOs)
│   │   ├── AlCopilot.Identity/   # Module
│   │   ├── AlCopilot.Recommendation/ # Module
│   │   ├── AlCopilot.Social/     # Module
│   │   └── AlCopilot.Inventory/  # Module
│   └── tests/                    # Per-module test projects
└── web/                          # Frontend (pnpm workspace)
    ├── apps/
    │   └── alcopilot-portal/     # Main user-facing app (Vite + React + TS)
    └── packages/                 # Shared packages (future)
```

## Conventions

### Git & Commits

- **Semantic commits only**: `feat:`, `fix:`, `chore:`, `docs:`, `ci:`, `refactor:`, `test:`, `build:`, `perf:`
- Enforced by Husky + commitlint. Non-semantic messages are rejected at commit time.
- Scope is optional but encouraged: `feat(catalog): add drink search endpoint`

### .NET (server/)

- **.NET 10**, Aspire for orchestration
- **Modular monolith**: each module owns its own `DbContext` and Postgres schema
- **Mediator** (Martin Othamar) for in-process dispatch — NOT MediatR
- **Central Package Management**: all NuGet versions in `server/Directory.Packages.props`
- **Version**: managed by Release Please via `server/Directory.Build.props`
- Module entry point: `Add*Module()` extension method in `*Module.cs`
- **Host = BFF**: `AlCopilot.Host` handles OIDC code flow with Keycloak and issues `HttpOnly; Secure; SameSite=Strict` cookies — tokens never reach the browser
- **YARP** for reverse-proxying to extracted services (when modules are split out)
- Container publishing via `dotnet publish /t:PublishContainer` — no Dockerfile
- Warnings are errors (`TreatWarningsAsErrors`)

### .NET (server/) — Contracts Pattern

- Each module has a `.Contracts` project (e.g., `AlCopilot.Catalog.Contracts`)
- Contracts contain interfaces, DTOs, events — NO implementation details
- Modules reference other modules' **Contracts only** — never the module itself
- Mediator dispatches requests defined in Contracts; handlers live in the module

### Frontend (web/)

- **pnpm** workspaces, packages scoped as `@alcopilot/*`
- **React + Vite + TypeScript**
- **TanStack Router** for type-safe routing
- **TanStack Query** for server state, **Zustand** for client-only state (when needed)
- **Tailwind CSS** + **shadcn/ui** for styling (when added)

### Infrastructure (deploy/)

- **No Dockerfile** — .NET SDK container support (`dotnet publish /t:PublishContainer`)
- **AKS + Flux** for production (GitOps)
- **Envoy Gateway** instead of Azure Application Gateway (cost)
