---
applyTo: 'server/**'
---

# Server Instructions (.NET Backend)

## Architecture Reference

Read [docs/architecture.md](../../docs/architecture.md) for full architecture, design decisions, and rationale.

## Stack

- **.NET 10** with **Aspire** for orchestration
- **EF Core** + **PostgreSQL** — one `DbContext` and schema per module
- **Mediator** (Martin Othamar, source-generated) — NOT MediatR
- **Rebus** for async messaging (when needed) — NOT MassTransit, NOT Wolverine

## Modular Monolith Rules

- Each module is a separate class library project under `server/src/`
- Each module owns its own `DbContext` with a dedicated Postgres schema
- Module entry point: `Add*Module(this IServiceCollection)` extension method in `*Module.cs`
- No cross-module EF entity references — use IDs only

### Contracts Pattern (Cross-Module Communication)

- Each module has a **Contracts** project: `AlCopilot.{Module}.Contracts`
- Contracts contain: interfaces, request/response DTOs, events, and shared models
- Contracts do NOT contain: EF entities, handlers, internal services, or implementation details
- Modules reference other modules' **Contracts** projects only — never the module implementation directly
- **Mediator** dispatches requests defined in Contracts — the handler lives in the module itself
- Example: `Catalog.Contracts` defines `GetDrinkByIdQuery` → `Catalog` contains the handler → `Recommendation` references `Catalog.Contracts` and sends the query via Mediator

### Container Publishing

- .NET SDK container support (`Microsoft.NET.Build.Containers`) — no Dockerfile needed
- Host project sets `<EnableSdkContainerSupport>true</EnableSdkContainerSupport>`
- Publish via `dotnet publish --os linux --arch x64 /t:PublishContainer`

### BFF & Authentication

- `AlCopilot.Host` is the BFF — it handles OIDC code flow with Keycloak and issues `HttpOnly; Secure; SameSite=Strict` cookies
- Tokens (access, refresh, id) are stored server-side — the SPA never sees them
- Keycloak runs locally via Aspire AppHost; in production as a service in AKS
- When a module is extracted, add YARP reverse proxy routes in the Host — the frontend changes nothing

## Project Structure Conventions

- `server/Directory.Build.props` — shared MSBuild properties and `<Version>`
- `server/Directory.Packages.props` — Central Package Management (all NuGet versions)
- `server/AlCopilot.slnx` — solution file
- Source projects: `server/src/AlCopilot.{ProjectName}/`
- Contract projects: `server/src/AlCopilot.{ProjectName}.Contracts/`
- Test projects: `server/tests/AlCopilot.{ProjectName}.Tests/`

## Code Style

- `TreatWarningsAsErrors` is enabled — all warnings must be resolved
- Nullable reference types enabled
- Follow .NET naming conventions (PascalCase for public, \_camelCase for private fields)
- Async methods suffixed with `Async`
- Use `sealed` on classes that are not designed for inheritance

## Testing

- **xUnit** for test framework
- **Shouldly** for assertions — NOT FluentAssertions (commercial)
- **NSubstitute** for mocking — NOT Moq (SponsorLink controversy)
- **TestContainers** (Postgres) for integration tests — NOT in-memory EF providers
- **NetArchTest.eNhanced** for architecture tests
- Unit + integration tests in the same project: `AlCopilot.{Module}.Tests`
- Integration tests marked with `[Trait("Category", "Integration")]`
- Architecture tests in `AlCopilot.Architecture.Tests`
- Test classes are `sealed`
- Use primary constructors for fixture injection
