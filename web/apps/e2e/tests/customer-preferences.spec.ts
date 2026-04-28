import { expect, test } from '@playwright/test';
import { fulfillJson, mockCustomerSession } from './fixtures/api';

test('customer preferences can select ingredient preferences and save the profile', async ({
  page,
}) => {
  let savedProfile: unknown;

  await mockCustomerSession(page, {
    isAuthenticated: true,
    displayName: 'Casey Customer',
    roles: ['customer'],
    canAccessCustomerPortal: true,
  });
  await page.route('**/api/customer/recommendations/sessions', (route) => fulfillJson(route, []));
  await page.route('**/api/customer/ingredients', (route) =>
    fulfillJson(route, [
      { id: 'gin', name: 'Gin', notableBrands: ['Plymouth'], ingredientGroup: 'Gin' },
      { id: 'dry-gin', name: 'Dry Gin', notableBrands: [], ingredientGroup: 'Gin' },
      { id: 'campari', name: 'Campari', notableBrands: [], ingredientGroup: 'Bitter' },
      { id: 'absinthe', name: 'Absinthe', notableBrands: [], ingredientGroup: null },
    ]),
  );
  await page.route('**/api/customer/profile/', async (route) => {
    if (route.request().method() === 'PUT') {
      savedProfile = route.request().postDataJSON();
      await fulfillJson(route, savedProfile as never);
      return;
    }

    await fulfillJson(route, {
      favoriteIngredientIds: [],
      dislikedIngredientIds: [],
      prohibitedIngredientIds: [],
      ownedIngredientIds: [],
    });
  });

  await page.goto('/preferences');

  await expect(page.getByRole('heading', { name: 'Preferences' })).toBeVisible();

  await page.getByRole('textbox', { name: 'Search Favorite ingredients' }).fill('Gin');
  await page.getByRole('checkbox', { name: 'Select Gin' }).click();
  await page.getByRole('button', { name: 'All Gin ingredients' }).click();
  await expect(page.getByText('2 selected')).toBeVisible();

  await page.getByRole('textbox', { name: 'Search Disliked ingredients' }).fill('Campari');
  await page.getByRole('checkbox', { name: 'Select Campari' }).click();

  await page.getByRole('textbox', { name: 'Search Prohibited ingredients' }).fill('Absinthe');
  await page.getByRole('checkbox', { name: 'Select Absinthe' }).click();

  await page.getByRole('button', { name: 'Save preferences' }).click();

  await expect(page.getByText('Preferences saved.')).toBeVisible();
  expect(savedProfile).toEqual({
    favoriteIngredientIds: ['gin', 'dry-gin'],
    dislikedIngredientIds: ['campari'],
    prohibitedIngredientIds: ['absinthe'],
    ownedIngredientIds: [],
  });
});
