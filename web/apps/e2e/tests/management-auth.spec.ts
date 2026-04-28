import { expect, test } from '@playwright/test';
import { mockManagementSession } from './fixtures/api';

test('management portal keeps unauthenticated users in the local sign-in gate', async ({
  page,
}) => {
  await mockManagementSession(page, {
    isAuthenticated: false,
    displayName: null,
    canAccessManagementPortal: false,
  });
  await page.route('**/api/auth/management/login?**', (route) =>
    route.fulfill({
      status: 200,
      contentType: 'text/html',
      body: '<h1>Management sign in</h1>',
    }),
  );

  await page.goto('/');

  await expect(page.getByRole('heading', { name: 'Sign in required' })).toBeVisible();
  await expect(page.getByText('Management workflows are reserved')).toBeVisible();

  await page.getByRole('button', { name: 'Continue to sign in' }).click();

  await expect(page).toHaveURL(/\/api\/auth\/management\/login\?returnUrl=%2F/);
  await expect(page.getByRole('heading', { name: 'Management sign in' })).toBeVisible();
});
