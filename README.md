# AlCopilot

AI-powered drinks suggestion platform. "Al" is your friendly AI bartender.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22](https://nodejs.org/) (see `.node-version`)
- [pnpm](https://pnpm.io/) (`npm install -g pnpm`)
- [Docker](https://www.docker.com/)

## Quick Start

```bash
# Install Node dependencies + git hooks
pnpm install

# Build the .NET backend
cd server && dotnet build

# Build the frontend
pnpm --filter @alcopilot/portal build
```

## Repository Structure

| Folder     | Purpose                                               |
| ---------- | ----------------------------------------------------- |
| `server/`  | .NET 10 modular monolith with Aspire                  |
| `web/`     | Frontend — pnpm workspace (Vite + React + TypeScript) |
| `deploy/`  | Docker, Helm, Flux, Terraform                         |
| `docs/`    | Architecture and documentation                        |
| `.github/` | CI/CD workflows, Copilot instructions, SKILLs         |

See [docs/architecture.md](docs/architecture.md) for full architecture and design decisions.

## Contributing

- **Semantic commits only** — enforced by Husky + commitlint
  - `feat:`, `fix:`, `chore:`, `docs:`, `ci:`, `refactor:`, `test:`, `build:`, `perf:`
  - Scope encouraged: `feat(catalog): add drink search endpoint`
- Versioning managed by [Release Please](https://github.com/googleapis/release-please)

## License

[Apache 2.0](LICENSE)
