# AlCopilot

AI-powered drinks suggestion platform. "Al" is your friendly AI bartender.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22](https://nodejs.org/) (see `.node-version`)
- [pnpm](https://pnpm.io/) (`npm install -g pnpm`)
- [Docker](https://www.docker.com/)

## Quick Start

```bash
# Install Node dependencies and git hooks when first cloning or when the lockfile changes
pnpm install

# Build the solution
dotnet build server/AlCopilot.slnx

# Start the local platform through Aspire
dotnet run --project server/src/AlCopilot.Orchestration/AlCopilot.Orchestration.csproj
```

Aspire is the local development entrypoint. It composes the backend Host, migrator, PostgreSQL, Qdrant, Keycloak, and the Vite-powered portal apps.
Use direct `pnpm --filter ...` commands only for focused frontend checks such as linting, testing, or production builds.

## Repository Structure

| Folder     | Purpose                                           |
| ---------- | ------------------------------------------------- |
| `server/`  | .NET 10 modular monolith and Aspire orchestration |
| `web/`     | Portal apps and shared frontend packages          |
| `deploy/`  | Docker, Helm, Flux, Terraform                     |
| `docs/`    | Architecture and documentation                    |
| `.github/` | CI/CD workflows, Copilot instructions, SKILLs     |

See [docs/architecture.md](docs/architecture.md) for full architecture and design decisions.

## Contributing

- **Semantic commits only** — enforced by Husky + commitlint
  - `feat:`, `fix:`, `chore:`, `docs:`, `ci:`, `refactor:`, `test:`, `build:`, `perf:`
  - Scope encouraged: `feat(catalog): add drink search endpoint`
- Versioning managed by [Release Please](https://github.com/googleapis/release-please)

## License

[Apache 2.0](LICENSE)
