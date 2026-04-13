import { Outlet, createRootRoute, createRoute, createRouter } from '@tanstack/react-router';
import { ManagementPortalLayout } from '@/features/auth/ManagementPortalLayout';
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
  return (
    <ManagementPortalLayout>
      <Outlet />
    </ManagementPortalLayout>
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
