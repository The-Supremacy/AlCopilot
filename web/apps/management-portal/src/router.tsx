import { Link, Outlet, createRootRoute, createRoute, createRouter } from '@tanstack/react-router';
import type { LucideIcon } from 'lucide-react';
import { ClipboardList, CupSoda, LayoutDashboard, ScrollText } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { CatalogDrinkCreatePage } from '@/pages/CatalogDrinkCreatePage';
import { CatalogDrinkEditPage } from '@/pages/CatalogDrinkEditPage';
import { CatalogDrinksPage } from '@/pages/CatalogDrinksPage';
import { CatalogIngredientCreatePage } from '@/pages/CatalogIngredientCreatePage';
import { CatalogIngredientEditPage } from '@/pages/CatalogIngredientEditPage';
import { CatalogIngredientsPage } from '@/pages/CatalogIngredientsPage';
import { AuditPage } from '@/pages/AuditPage';
import { CatalogPage } from '@/pages/CatalogPage';
import { CatalogTagCreatePage } from '@/pages/CatalogTagCreatePage';
import { CatalogTagEditPage } from '@/pages/CatalogTagEditPage';
import { CatalogTagsPage } from '@/pages/CatalogTagsPage';
import { DashboardPage } from '@/pages/DashboardPage';
import { ImportsPage } from '@/pages/ImportsPage';
import { ImportReviewPage } from '@/pages/ImportReviewPage';

function RootLayout() {
  type NavItem = {
    to: string;
    label: string;
    icon: LucideIcon;
    children?: Array<{ to: string; label: string }>;
  };

  const navItems: NavItem[] = [
    { to: '/', label: 'Dashboard', icon: LayoutDashboard },
    {
      to: '/catalog',
      label: 'Catalog',
      icon: CupSoda,
      children: [
        { to: '/catalog', label: 'Overview' },
        { to: '/catalog/drinks', label: 'Drinks' },
        { to: '/catalog/tags', label: 'Tags' },
        { to: '/catalog/ingredients', label: 'Ingredients' },
      ],
    },
    { to: '/imports', label: 'Imports', icon: ClipboardList },
    { to: '/audit', label: 'Audit', icon: ScrollText },
  ];

  return (
    <div className="min-h-screen p-4 md:p-6">
      <div className="mx-auto grid min-h-[calc(100vh-2rem)] max-w-7xl overflow-hidden rounded-[28px] border border-border/60 bg-white/70 shadow-soft backdrop-blur md:grid-cols-[280px_minmax(0,1fr)]">
        <aside className="border-b border-border/70 bg-slate-950 px-5 py-6 text-slate-100 md:border-b-0 md:border-r">
          <div className="space-y-3">
            <div>
              <p className="text-xs uppercase tracking-[0.32em] text-amber-200/80">AlCopilot</p>
              <h1 className="mt-2 text-2xl font-semibold">Management Portal</h1>
            </div>
            <div className="inline-flex rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs text-slate-300">
              Anonymous access
            </div>
            <p className="text-sm leading-6 text-slate-300">
              Catalog curation, import sync review, and audit visibility for operator workflows.
            </p>
          </div>

          <nav className="mt-8 grid gap-3" aria-label="Primary">
            {navItems.map((item) => {
              const Icon = item.icon;

              return (
                <div key={item.to} className="space-y-2">
                  <Link
                    to={item.to}
                    activeOptions={item.to === '/' ? { exact: true } : undefined}
                    className={cn(
                      'inline-flex w-full items-center gap-3 rounded-xl px-4 py-3 text-sm text-slate-300 transition-colors hover:bg-white/10 hover:text-white',
                    )}
                    activeProps={{
                      className:
                        'inline-flex w-full items-center gap-3 rounded-xl bg-gradient-to-r from-amber-500/25 to-cyan-400/15 px-4 py-3 text-sm text-white',
                    }}
                  >
                    <Icon className="h-4 w-4" />
                    <span>{item.label}</span>
                  </Link>
                  {item.children ? (
                    <div className="grid gap-1 pl-10">
                      {item.children.map((child) => (
                        <Link
                          key={child.to}
                          to={child.to}
                          activeOptions={child.to === '/catalog' ? { exact: true } : undefined}
                          className={cn(
                            'rounded-lg px-3 py-2 text-xs font-medium uppercase tracking-wide text-slate-400 transition-colors hover:bg-white/10 hover:text-white',
                          )}
                          activeProps={{
                            className:
                              'rounded-lg bg-white/10 px-3 py-2 text-xs font-semibold uppercase tracking-wide text-white',
                          }}
                        >
                          {child.label}
                        </Link>
                      ))}
                    </div>
                  ) : null}
                </div>
              );
            })}
          </nav>
        </aside>

        <div className="min-w-0 bg-white/40">
          <header className="flex flex-col gap-4 border-b border-border/70 px-6 py-5 md:flex-row md:items-center md:justify-between">
            <div>
              <p className="text-xs uppercase tracking-[0.3em] text-muted-foreground">
                Operator Console
              </p>
              <h2 className="mt-2 text-2xl font-semibold text-slate-900">
                Catalog curation and import sync
              </h2>
            </div>
            <Button variant="outline" className="w-full md:w-auto">
              Anonymous session
            </Button>
          </header>

          <main className="px-6 py-6">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  );
}

const rootRoute = createRootRoute({
  component: RootLayout,
});

const indexRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/',
  component: DashboardPage,
});

const catalogRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog',
  component: CatalogPage,
});

const catalogDrinksRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/drinks',
  component: CatalogDrinksPage,
});

const catalogDrinkCreateRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/drinks/new',
  component: CatalogDrinkCreatePage,
});

const catalogDrinkEditRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/drinks/$drinkId',
  component: CatalogDrinkEditPage,
});

const catalogTagsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/tags',
  component: CatalogTagsPage,
});

const catalogTagCreateRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/tags/new',
  component: CatalogTagCreatePage,
});

const catalogTagEditRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/tags/$tagId',
  component: CatalogTagEditPage,
});

const catalogIngredientsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/ingredients',
  component: CatalogIngredientsPage,
});

const catalogIngredientCreateRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/ingredients/new',
  component: CatalogIngredientCreatePage,
});

const catalogIngredientEditRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/ingredients/$ingredientId',
  component: CatalogIngredientEditPage,
});

const importsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/imports',
  component: ImportsPage,
});

const importReviewRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/imports/$batchId/review',
  component: ImportReviewPage,
});

const auditRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/audit',
  component: AuditPage,
});

const routeTree = rootRoute.addChildren([
  indexRoute,
  catalogRoute,
  catalogDrinksRoute,
  catalogDrinkCreateRoute,
  catalogDrinkEditRoute,
  catalogTagsRoute,
  catalogTagCreateRoute,
  catalogTagEditRoute,
  catalogIngredientsRoute,
  catalogIngredientCreateRoute,
  catalogIngredientEditRoute,
  importsRoute,
  importReviewRoute,
  auditRoute,
]);

export const router = createRouter({ routeTree });

declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router;
  }
}
