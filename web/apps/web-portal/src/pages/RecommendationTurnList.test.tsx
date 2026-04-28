import { render, screen } from '@testing-library/react';
import { RecommendationTurnList } from '@/features/recommendations/RecommendationTurnList';

describe('RecommendationTurnList', () => {
  it('renders structured recommendation groups directly from the session DTO', () => {
    render(
      <RecommendationTurnList
        session={{
          sessionId: 'session-1',
          title: 'Citrus night',
          createdAtUtc: '2026-04-15T00:00:00Z',
          updatedAtUtc: '2026-04-15T00:10:00Z',
          turns: [
            {
              turnId: 'turn-1',
              sequence: 1,
              role: 'assistant',
              content: 'Here are two strong options.',
              createdAtUtc: '2026-04-15T00:10:00Z',
              toolInvocations: [],
              feedback: null,
              recommendationGroups: [
                {
                  key: 'make-now',
                  label: 'Available now',
                  items: [
                    {
                      drinkId: 'daiquiri',
                      drinkName: 'Daiquiri',
                      description: 'Bright and clean.',
                      missingIngredientNames: [],
                      matchedSignals: ['lime', 'rum'],
                      score: 96,
                      recipeEntries: [
                        { ingredientName: 'White Rum', quantity: '2 oz', isOwned: true },
                        { ingredientName: 'Lime Juice', quantity: '1 oz', isOwned: true },
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
                      description: 'Sharper and herbal.',
                      missingIngredientNames: ['Green Chartreuse'],
                      matchedSignals: ['gin'],
                      score: 82,
                      recipeEntries: [
                        { ingredientName: 'Gin', quantity: '3/4 oz', isOwned: true },
                        { ingredientName: 'Green Chartreuse', quantity: '3/4 oz', isOwned: false },
                      ],
                    },
                  ],
                },
              ],
            },
            {
              turnId: 'turn-2',
              sequence: 2,
              role: 'assistant',
              content: '**Top pick**\n* Bright and citrusy\n* Easy to make',
              createdAtUtc: '2026-04-15T00:12:00Z',
              toolInvocations: [],
              feedback: null,
              recommendationGroups: [],
            },
          ],
        }}
      />,
    );

    expect(screen.getByLabelText('Available now')).toBeInTheDocument();
    expect(screen.getByLabelText('Consider for restock')).toBeInTheDocument();
    expect(
      screen.getByText('Available now: everything is already in your bar.'),
    ).toBeInTheDocument();
    expect(screen.getByText('Consider for restock: Green Chartreuse')).toBeInTheDocument();
    expect(screen.getByText('Matches: lime, rum')).toBeInTheDocument();
    expect(screen.queryByText(/Score/)).not.toBeInTheDocument();
    expect(screen.getByText(/White Rum \(2 oz\)/)).toBeInTheDocument();
    expect(screen.getByText(/Green Chartreuse \(3\/4 oz\)/)).toBeInTheDocument();
    expect(screen.getByText('Top pick')).toBeInTheDocument();
    expect(screen.getByText('Bright and citrusy')).toBeInTheDocument();
    expect(screen.getByText('Easy to make')).toBeInTheDocument();
  });

  it('renders drink details without recommendation scoring or availability copy', () => {
    render(
      <RecommendationTurnList
        session={{
          sessionId: 'session-2',
          title: 'Ramos Fizz',
          createdAtUtc: '2026-04-15T00:00:00Z',
          updatedAtUtc: '2026-04-15T00:10:00Z',
          turns: [
            {
              turnId: 'turn-1',
              sequence: 1,
              role: 'assistant',
              content: 'Here are the details for Ramos Fizz.',
              createdAtUtc: '2026-04-15T00:10:00Z',
              toolInvocations: [],
              feedback: null,
              recommendationGroups: [
                {
                  key: 'drink-details',
                  label: 'Drink Details',
                  items: [
                    {
                      drinkId: 'ramos-fizz',
                      drinkName: 'Ramos Fizz',
                      description: 'Creamy, citrusy, and floral.',
                      missingIngredientNames: [],
                      matchedSignals: ['Ramos Fizz'],
                      score: 94,
                      recipeEntries: [
                        { ingredientName: 'Gin', quantity: '2 oz', isOwned: true },
                        { ingredientName: 'Cream', quantity: '1 oz', isOwned: false },
                      ],
                    },
                  ],
                },
              ],
            },
          ],
        }}
      />,
    );

    expect(screen.getByLabelText('Drink Details')).toBeInTheDocument();
    expect(screen.getByText('Resolved drink')).toBeInTheDocument();
    expect(screen.getByText('Ramos Fizz')).toBeInTheDocument();
    expect(screen.queryByText(/Score/)).not.toBeInTheDocument();
    expect(screen.queryByText(/Matches:/)).not.toBeInTheDocument();
    expect(
      screen.queryByText('Available now: everything is already in your bar.'),
    ).not.toBeInTheDocument();
  });
});
