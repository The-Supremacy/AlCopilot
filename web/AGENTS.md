# Web Conventions (Frontend)

## Architecture Reference

Read [docs/architecture.md](../docs/architecture.md) for full architecture and tech stack decisions.

## Stack

- **pnpm** workspaces — packages scoped as `@alcopilot/*`
- **Vite** + **React** + **TypeScript** (strict mode)
- **TanStack Router** for type-safe routing
- **TanStack Query** for server state management
- **Zustand** for client-only state (when needed)
- **Tailwind CSS** + **shadcn/ui** for styling (when added)

## Workspace Structure

- `web/apps/alcopilot-portal/` — main user-facing application
- `web/packages/` — shared packages (future: `@alcopilot/ui`, etc.)
- Root `pnpm-workspace.yaml` defines workspace packages

## Conventions

- Package names: `@alcopilot/{name}`
- Use path aliases: `@/` maps to `src/`
- Prefer named exports over default exports (except page components)
- Colocate tests next to source files (`.test.tsx` / `.test.ts`)
- Use `function` declarations for components, not arrow functions

## API Communication

- API proxy configured in `vite.config.ts` — `/api` routes to backend during dev
- Use TanStack Query hooks for all API calls — no raw `fetch` in components
- API types should be shared or generated, not manually duplicated

## Testing

- **Vitest** for component testing — native to Vite, same config and transforms
- **React Testing Library** for rendering and interaction assertions
- **Playwright** for E2E tests — runs against deployed staging environment (nightly/pre-release, not every PR)
- Colocate test files next to source: `Component.test.tsx` / `hook.test.ts`
- Prefer user-centric queries (`getByRole`, `getByText`) over implementation details (`getByTestId`)

## Review Checklist

When reviewing React/TS code, verify:

- [ ] Uses **TanStack Router** for routing — NOT React Router
- [ ] Uses **TanStack Query** for server state — NOT raw `fetch` or Redux
- [ ] Uses **Zustand** for client-only state — NOT Redux or MobX
- [ ] Uses **shadcn/ui + Tailwind** — NOT Material UI or Chakra
- [ ] Uses **Vitest** for tests — NOT Jest
- [ ] Named exports (except page components)
- [ ] `function` declarations for components (not arrow functions)
- [ ] `@/` path alias for `src/` imports
- [ ] Tests colocated: `.test.tsx` / `.test.ts` next to source
- [ ] User-centric queries in tests (not `getByTestId`)
- [ ] Package names scoped as `@alcopilot/{name}`
- [ ] No `any` types (unless genuinely unavoidable)
