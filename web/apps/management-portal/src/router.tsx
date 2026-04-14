import { Outlet, createRootRoute, createRoute, createRouter } from '@tanstack/react-router';
import { ManagementPortalLayout } from '@/features/auth/ManagementPortalLayout';
import {
  catalogBreadcrumbs,
  createBreadcrumb,
  drinksBreadcrumbs,
  ingredientsBreadcrumbs,
  tagsBreadcrumbs,
} from '@/features/navigation/breadcrumbs';
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
  staticData: {
    breadcrumbs: catalogBreadcrumbs(),
  },
});

const catalogDrinksRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/drinks',
  component: CatalogDrinksPage,
  staticData: {
    breadcrumbs: drinksBreadcrumbs(),
  },
});

const catalogDrinkCreateRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/drinks/new',
  component: CatalogDrinkCreatePage,
  staticData: {
    breadcrumbs: drinksBreadcrumbs(createBreadcrumb('New drink')),
  },
});

const catalogDrinkEditRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/drinks/$drinkId',
  component: CatalogDrinkEditPage,
  staticData: {
    breadcrumbs: drinksBreadcrumbs(createBreadcrumb('Edit drink')),
  },
});

const catalogTagsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/tags',
  component: CatalogTagsPage,
  staticData: {
    breadcrumbs: tagsBreadcrumbs(),
  },
});

const catalogTagCreateRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/tags/new',
  component: CatalogTagCreatePage,
  staticData: {
    breadcrumbs: tagsBreadcrumbs(createBreadcrumb('New tag')),
  },
});

const catalogTagEditRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/tags/$tagId',
  component: CatalogTagEditPage,
  staticData: {
    breadcrumbs: tagsBreadcrumbs(createBreadcrumb('Edit tag')),
  },
});

const catalogIngredientsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/ingredients',
  component: CatalogIngredientsPage,
  staticData: {
    breadcrumbs: ingredientsBreadcrumbs(),
  },
});

const catalogIngredientCreateRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/ingredients/new',
  component: CatalogIngredientCreatePage,
  staticData: {
    breadcrumbs: ingredientsBreadcrumbs(createBreadcrumb('New ingredient')),
  },
});

const catalogIngredientEditRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/catalog/ingredients/$ingredientId',
  component: CatalogIngredientEditPage,
  staticData: {
    breadcrumbs: ingredientsBreadcrumbs(createBreadcrumb('Edit ingredient')),
  },
});

const importsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/imports',
  component: ImportsPage,
  staticData: {
    breadcrumbs: [createBreadcrumb('Imports')],
  },
});

const importReviewRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/imports/$batchId/review',
  component: ImportReviewPage,
  staticData: {
    breadcrumbs: [createBreadcrumb('Imports', { to: '/imports' }), createBreadcrumb('Review')],
  },
});

const auditRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/audit',
  component: AuditPage,
  staticData: {
    breadcrumbs: [createBreadcrumb('Audit')],
  },
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
