import type { Page, Route } from '@playwright/test';

type JsonValue = boolean | number | string | null | JsonValue[] | { [key: string]: JsonValue };

export async function fulfillJson(route: Route, body: JsonValue, status = 200) {
  await route.fulfill({
    status,
    contentType: 'application/json',
    body: JSON.stringify(body),
  });
}

export async function mockManagementSession(
  page: Page,
  session: {
    isAuthenticated: boolean;
    displayName: string | null;
    roles?: string[];
    isAdmin?: boolean;
    canAccessManagementPortal: boolean;
  },
) {
  await page.route('**/api/auth/management/session', (route) =>
    fulfillJson(route, {
      roles: [],
      isAdmin: false,
      ...session,
    }),
  );
}

export async function mockCustomerSession(
  page: Page,
  session: {
    isAuthenticated: boolean;
    displayName: string | null;
    roles?: string[];
    canAccessCustomerPortal: boolean;
  },
) {
  await page.route('**/api/auth/customer/session', (route) =>
    fulfillJson(route, {
      roles: [],
      ...session,
    }),
  );
}
