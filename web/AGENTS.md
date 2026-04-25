# Web Conventions (Frontend)

## Architecture Reference

Read [docs/constitution.md](../docs/constitution.md) for project-wide governance and workflow rules.
Read [docs/constitution/web.md](../docs/constitution/web.md) for frontend workflow and quality expectations.
Read [docs/architecture.md](../docs/architecture.md) for the thin project-wide architecture index.
Read [docs/architecture/web.md](../docs/architecture/web.md) for frontend architecture and stack decisions.
Read [docs/adr/0003-frontend-stack.md](../docs/adr/0003-frontend-stack.md) for the accepted frontend stack decision.
Read [docs/adr/0007-management-portal-architecture-and-envoy-host-routing.md](../docs/adr/0007-management-portal-architecture-and-envoy-host-routing.md) for portal-level frontend boundary and ingress routing direction.
Read portal design guides in `web/apps/*/DESIGN.md` for persistent layout, navigation, and UI composition rules.
Read [docs/testing.md](../docs/testing.md) for the thin project-wide testing index.
Read [docs/testing/web.md](../docs/testing/web.md) for the frontend test taxonomy and when to use each layer.
Use `/design:new` to create a new portal design guide, `/design:change` to refine existing portal design invariants, and `/design:lint` to verify low-drift compliance.

## Stack

- **pnpm** workspaces — packages scoped as `@alcopilot/*`
- **Vite** + **React** + **TypeScript** (strict mode)
- **TanStack Router** for type-safe routing
- **TanStack Query** for server state management
- **Zustand** for client-only state (when needed)
- **Tailwind CSS** + **shadcn/ui** for styling

New portal UI work MUST use Tailwind CSS and shadcn-managed local components by default.
Do not start a new page or portal with a custom CSS-only component layer unless the change explicitly documents an approved exception.
Initialize shadcn visibly with `components.json` and keep generated or normalized primitives under `src/components/ui/`.
Use Radix-backed shadcn primitives when interaction behavior needs accessible component "brains" such as dialogs, menus, popovers, or composition slots.
If Zustand is unnecessary for the change, that is fine; the requirement is to stay within the approved stack, not to force Zustand everywhere.
Stable cross-portal shadcn-derived primitives may be shared from a workspace package such as `@alcopilot/ui`, but each portal should keep thin local entrypoints under `src/components/ui/` so app ownership and shadcn visibility remain clear.

## Workspace Structure

- `web/apps/` — portal apps workspace (user and management portal boundaries)
- `web/apps/web-portal/` — planned user-facing app path
- `web/packages/` — shared cross-cutting packages (for example API contracts, shared UI primitives, reusable service clients)
- Stable shared UI foundations belong in `web/packages/`, while portal-specific layout and feature composition stay inside the portal app
- Root `pnpm-workspace.yaml` defines workspace packages
- Route-level React files belong in `src/pages/`
- Large pages should usually decompose into `src/features/<area>/` sections plus colocated page hooks

## Conventions

- Package names: `@alcopilot/{name}`
- Use path aliases: `@/` maps to `src/`
- Prefer named exports over default exports (except page components)
- Colocate tests next to source files (`.test.tsx` / `.test.ts`)
- Use `function` declarations for components, not arrow functions
- Call React hooks unconditionally at the top level of the component or hook before any conditional return that may skip later hooks
- Prefer the component shape `hooks -> derived values -> conditional render branches -> final render`; put conditions inside hook bodies rather than around hook calls
- If different branches need different hooks, split them into child components instead of conditionally calling hooks in one component
- For route-level screens that outgrow a single file, prefer the shape `Page` + optional `usePageState` hook + `*View` or clearly named feature sections rather than keeping all orchestration and rendering in one component
- Keep route orchestration in pages and move most rendering detail into feature sections or page-local hooks once a page starts to grow
- For UI-affecting changes, update the impacted portal `DESIGN.md` before implementation work starts
- OpenSpec proposal/design/spec/task artifacts for UI-affecting changes should reference impacted portal `DESIGN.md` guides
- For UI-affecting changes, explicitly confirm stack alignment with Tailwind CSS + shadcn/ui in the proposal or design artifact unless an approved exception is being used
- Use `DESIGN.md` only for durable UI invariants; keep behavior and acceptance logic in OpenSpec
- Run `/design:lint` before `/opsx:apply` for UI-affecting changes when design docs were modified

## API Communication

- Local integrated development starts from `AlCopilot.Orchestration`; Aspire launches the portal Vite apps and supplies their Host API proxy targets
- API proxy configured in `vite.config.ts` — `/api` routes to the backend Host during dev
- Use TanStack Query hooks for all API calls — no raw `fetch` in components
- API types should be shared or generated, not manually duplicated

## Testing

- **Vitest** for unit, component, and route-level integration testing — native to Vite, same config and transforms
- **React Testing Library** for rendering and interaction assertions
- **Playwright** for E2E tests — runs against deployed staging environment (nightly/pre-release, not every PR)
- Colocate test files next to source: `Component.test.tsx` / `hook.test.ts`
- Prefer user-centric queries (`getByRole`, `getByText`) over implementation details (`getByTestId`)
- Build UI with accessible semantics and stable user-visible structure so flows can be automated cleanly later

## Review Checklist

When reviewing React/TS code, verify:

- [ ] Uses **TanStack Router** for routing — NOT React Router
- [ ] Uses **TanStack Query** for server state — NOT raw `fetch` or Redux
- [ ] Uses **Zustand** for client-only state when client-only shared state is needed — NOT Redux or MobX
- [ ] Uses **shadcn/ui + Tailwind** — NOT Material UI or Chakra
- [ ] `components.json` exists when a portal claims shadcn alignment
- [ ] Local primitives live under `src/components/ui/`
- [ ] Radix packages appear when richer interactive primitives need accessible behavior
- [ ] Does not introduce a custom CSS-only UI layer as the default path for new portal work
- [ ] Uses **Vitest** for tests — NOT Jest
- [ ] Named exports (except page components)
- [ ] `function` declarations for components (not arrow functions)
- [ ] Hooks are never called conditionally or after an early return that may run on some renders but not others
- [ ] Loading, not-found, auth-gate, and feature-flag branches occur after the component's hook section, or are split into child components
- [ ] Large route components separate orchestration from rendering with a page-state hook and/or dedicated `*View` or section components when that improves clarity
- [ ] `@/` path alias for `src/` imports
- [ ] Tests colocated: `.test.tsx` / `.test.ts` next to source
- [ ] User-centric queries in tests (not `getByTestId`)
- [ ] Package names scoped as `@alcopilot/{name}`
- [ ] No `any` types (unless genuinely unavoidable)
