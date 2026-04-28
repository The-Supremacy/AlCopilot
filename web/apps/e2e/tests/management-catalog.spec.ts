import { expect, test } from '@playwright/test';
import { fulfillJson, mockManagementSession } from './fixtures/api';

const bitterTag = { id: 'tag-bitter', name: 'Bitter', drinkCount: 1 };
const shakenTag = { id: 'tag-shaken', name: 'Shaken', drinkCount: 0 };
const lemon = {
  id: 'ingredient-lemon',
  name: 'Lemon juice',
  notableBrands: [],
  ingredientGroup: 'Citrus',
};
const bourbon = {
  id: 'ingredient-bourbon',
  name: 'Bourbon',
  notableBrands: ['Wild Turkey'],
  ingredientGroup: 'Whiskey',
};

test('management catalog can browse drinks and create a drink with mocked API data', async ({
  page,
}) => {
  const drinks = [
    {
      id: 'drink-negroni',
      name: 'Negroni',
      category: 'Classic',
      description: 'Bitter and balanced.',
      method: 'Stir',
      garnish: 'Orange peel',
      imageUrl: null,
      tags: [bitterTag],
    },
  ];
  let createdDrinkPayload: unknown;

  await mockManagementSession(page, {
    isAuthenticated: true,
    displayName: 'Ada Manager',
    roles: ['manager'],
    canAccessManagementPortal: true,
  });
  await page.route('**/api/drink-catalog/tags/', (route) =>
    fulfillJson(route, [bitterTag, shakenTag]),
  );
  await page.route('**/api/drink-catalog/ingredients/', (route) =>
    fulfillJson(route, [lemon, bourbon]),
  );
  await page.route('**/api/drink-catalog/drinks/**', async (route) => {
    const request = route.request();

    if (request.method() === 'GET') {
      await fulfillJson(route, {
        items: drinks,
        totalCount: drinks.length,
        page: 1,
        pageSize: 50,
      });
      return;
    }

    if (request.method() === 'POST') {
      createdDrinkPayload = request.postDataJSON();
      drinks.push({
        id: 'drink-paper-plane',
        name: 'Paper Plane',
        category: 'Modern classic',
        description: 'Equal-parts whiskey sour riff.',
        method: 'Shake with ice.',
        garnish: 'Lemon twist',
        imageUrl: null,
        tags: [shakenTag],
      });
      await fulfillJson(route, { id: 'drink-paper-plane' }, 201);
      return;
    }

    await route.fallback();
  });
  await page.route('**/api/drink-catalog/audit-log', (route) => fulfillJson(route, []));

  await page.goto('/catalog/drinks');

  await expect(page.getByRole('heading', { name: 'Drink list' })).toBeVisible();
  await page.getByPlaceholder('Search drinks').fill('Negroni');
  await expect(page.getByRole('link', { name: 'Open drink Negroni' })).toBeVisible();

  await page.getByRole('link', { name: 'New' }).click();
  await expect(page.getByRole('heading', { level: 1, name: 'Create drink' })).toBeVisible();

  await page.getByLabel('Drink name').fill('Paper Plane');
  await page.getByLabel('Drink category').fill('Modern classic');
  await page.getByLabel('Description').fill('Equal-parts whiskey sour riff.');
  await page.getByLabel('Method').fill('Shake with ice.');
  await page.getByLabel('Garnish').fill('Lemon twist');
  await page.getByLabel('Shaken').check();
  await page.getByRole('button', { name: 'Add ingredient' }).click();
  await page.getByLabel('Recipe ingredient 1').selectOption('ingredient-bourbon');
  await page.getByLabel('Recipe quantity 1').fill('3/4 oz');
  await page.getByLabel('Recommended brand 1').fill('Wild Turkey 101');
  await page.getByRole('button', { name: 'Save' }).click();

  await expect(page).toHaveURL(/\/catalog\/drinks$/);
  await expect(page.getByRole('link', { name: 'Open drink Paper Plane' })).toBeVisible();
  expect(createdDrinkPayload).toEqual({
    name: 'Paper Plane',
    category: 'Modern classic',
    description: 'Equal-parts whiskey sour riff.',
    method: 'Shake with ice.',
    garnish: 'Lemon twist',
    imageUrl: null,
    tagIds: ['tag-shaken'],
    recipeEntries: [
      {
        ingredientId: 'ingredient-bourbon',
        quantity: '3/4 oz',
        recommendedBrand: 'Wild Turkey 101',
      },
    ],
  });
});
