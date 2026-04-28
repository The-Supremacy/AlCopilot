import { expect, test } from '@playwright/test';
import { fulfillJson, mockCustomerSession } from './fixtures/api';

test('customer chat submits a recommendation request and expands structured results', async ({
  page,
}) => {
  let submittedMessage: unknown;
  let feedbackPayload: unknown;

  await mockCustomerSession(page, {
    isAuthenticated: true,
    displayName: 'Casey Customer',
    roles: ['customer'],
    canAccessCustomerPortal: true,
  });
  await page.route('**/api/customer/recommendations/sessions', (route) =>
    fulfillJson(route, [
      {
        sessionId: 'session-1',
        title: 'Citrus night',
        createdAtUtc: '2026-04-28T00:00:00Z',
        updatedAtUtc: '2026-04-28T00:01:00Z',
        lastAssistantMessage: 'Try a Daiquiri first.',
      },
    ]),
  );
  await page.route('**/api/customer/recommendations/messages', async (route) => {
    submittedMessage = route.request().postDataJSON();
    await fulfillJson(route, { sessionId: 'session-1' }, 201);
  });
  await page.route('**/api/customer/recommendations/sessions/session-1', (route) =>
    fulfillJson(route, {
      sessionId: 'session-1',
      title: 'Citrus night',
      createdAtUtc: '2026-04-28T00:00:00Z',
      updatedAtUtc: '2026-04-28T00:01:00Z',
      turns: [
        {
          turnId: 'turn-user-1',
          sequence: 1,
          role: 'user',
          content: 'Something citrusy with rum.',
          recommendationGroups: [],
          toolInvocations: [],
          feedback: null,
          createdAtUtc: '2026-04-28T00:00:00Z',
        },
        {
          turnId: 'turn-assistant-1',
          sequence: 2,
          role: 'assistant',
          content: 'Try a Daiquiri first, then restock for a Last Word.',
          recommendationGroups: [
            {
              key: 'make-now',
              label: 'Available now',
              items: [
                {
                  drinkId: 'daiquiri',
                  drinkName: 'Daiquiri',
                  description: 'Bright, tart, and clean.',
                  missingIngredientNames: [],
                  matchedSignals: ['lime', 'rum'],
                  score: 96,
                  recipeEntries: [
                    { ingredientName: 'White rum', quantity: '2 oz', isOwned: true },
                    { ingredientName: 'Lime juice', quantity: '1 oz', isOwned: true },
                  ],
                },
              ],
            },
            {
              key: 'buy-next',
              label: 'Consider for restock',
              items: [
                {
                  drinkId: 'last-word',
                  drinkName: 'Last Word',
                  description: 'Herbal, sharp, and memorable.',
                  missingIngredientNames: ['Green Chartreuse'],
                  matchedSignals: ['gin'],
                  score: 82,
                  recipeEntries: [
                    { ingredientName: 'Gin', quantity: '3/4 oz', isOwned: true },
                    {
                      ingredientName: 'Green Chartreuse',
                      quantity: '3/4 oz',
                      isOwned: false,
                    },
                  ],
                },
              ],
            },
          ],
          toolInvocations: [{ toolName: 'recommend_drinks', purpose: 'Find matching drinks' }],
          feedback: null,
          createdAtUtc: '2026-04-28T00:01:00Z',
        },
      ],
    }),
  );
  await page.route(
    '**/api/customer/recommendations/sessions/session-1/turns/turn-assistant-1/feedback',
    async (route) => {
      feedbackPayload = route.request().postDataJSON();
      await route.fulfill({ status: 204 });
    },
  );

  await page.goto('/');

  await page.getByLabel('Ask a question').fill('Something citrusy with rum.');
  await page.getByRole('button', { name: 'Send' }).click();

  await expect(page).toHaveURL(/\/chat\/session-1$/);
  expect(submittedMessage).toEqual({
    sessionId: null,
    message: 'Something citrusy with rum.',
  });

  await expect(page.getByText('Try a Daiquiri first, then restock for a Last Word.')).toBeVisible();
  await expect(page.getByRole('region', { name: 'Recommendations' })).toBeVisible();

  await page.getByRole('button', { name: /Available now/ }).click();
  await page.getByRole('button', { name: /Daiquiri/ }).click();
  await expect(page.getByText(/White rum \(2 oz\)/)).toBeVisible();
  await expect(page.getByText('Available now: everything is already in your bar.')).toBeVisible();

  await page.getByRole('button', { name: /Buy next/ }).click();
  await page.getByRole('button', { name: /Last Word/ }).click();
  await expect(page.getByText('Buy next: Green Chartreuse')).toBeVisible();
  await expect(page.getByText(/Green Chartreuse \(3\/4 oz\)/)).toBeVisible();
  await expect(page.getByText(/Score/)).toHaveCount(0);

  await page.getByRole('button', { name: 'Mark response helpful' }).click();
  expect(feedbackPayload).toEqual({ rating: 'positive', comment: null });
});
