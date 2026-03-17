---
applyTo: '.github/workflows/**'
---

# CI Instructions (GitHub Actions)

## Workflows

- `ci.yml` — runs on PRs and pushes to main: build, test, lint, coverage report
- `release.yml` — runs on push to main: Release Please for versioning + `dotnet publish` container push

## Conventions

- Use specific action versions (pin to SHA or major version tag)
- Cache dependencies (NuGet, pnpm store) for faster builds
- Run server and web jobs in parallel where possible
- Use `--frozen-lockfile` for pnpm installs in CI
- Use `--no-restore` after explicit restore steps for dotnet
- No Dockerfile — use .NET SDK container support (`/t:PublishContainer`)

## Code Coverage

- `coverlet.collector` generates coverage during `dotnet test`
- `danielpalme/ReportGenerator-GitHub-Action` produces summary report
- Coverage summary posted as PR comment — no hard gate, informational only

## Release Please

- Manifest mode with `release-please-config.json` and `.release-please-manifest.json`
- Single version for the entire monolith
- `<Version>` in `server/Directory.Build.props` updated via `extra-files`
- Conventional commits drive semver bumps

## Secrets & Security

- Use GitHub's built-in `GITHUB_TOKEN` where possible
- Container images pushed to GHCR via `dotnet publish`
- No hardcoded secrets — use GitHub Actions secrets or environment variables
