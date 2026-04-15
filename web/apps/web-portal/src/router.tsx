import {
  Navigate,
  Outlet,
  createRootRoute,
  createRoute,
  createRouter,
} from '@tanstack/react-router';
import { CustomerPortalLayout } from '@/features/auth/CustomerPortalLayout';
import { AccountPage } from '@/pages/AccountPage';
import { ChatPage } from '@/pages/ChatPage';
import { HistoryPage } from '@/pages/HistoryPage';
import { MyBarPage } from '@/pages/MyBarPage';
import { PreferencesPage } from '@/pages/PreferencesPage';

function RootLayout() {
  return (
    <CustomerPortalLayout>
      <Outlet />
    </CustomerPortalLayout>
  );
}

const rootRoute = createRootRoute({
  component: RootLayout,
});

const indexRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/',
  component: ChatPage,
});

const chatSessionRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/chat/$sessionId',
  component: ChatPage,
});

const myBarRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/my-bar',
  component: MyBarPage,
});

const preferencesRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/preferences',
  component: PreferencesPage,
});

const historyRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/history',
  component: HistoryPage,
});

const accountRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/account',
  component: AccountPage,
});

const legacyHistorySessionRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/history/$sessionId',
  component: function LegacyHistoryRedirect() {
    const { sessionId } = legacyHistorySessionRoute.useParams();
    return <Navigate to="/chat/$sessionId" params={{ sessionId }} />;
  },
});

const routeTree = rootRoute.addChildren([
  indexRoute,
  chatSessionRoute,
  myBarRoute,
  preferencesRoute,
  historyRoute,
  accountRoute,
  legacyHistorySessionRoute,
]);

export const router = createRouter({ routeTree });

declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router;
  }
}
