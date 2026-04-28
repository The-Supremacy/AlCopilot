import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { RecommendationTurnList } from '@/features/recommendations/RecommendationTurnList';

describe('RecommendationTurnList', () => {
  it('renders structured recommendation groups as progressive disclosure from the session DTO', async () => {
    const user = userEvent.setup();

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

    expect(screen.getByText('Here are two strong options.')).toBeInTheDocument();
    expect(screen.getByRole('region', { name: 'Recommendations' })).toBeInTheDocument();
    expect(screen.getByText('Recommendations')).toBeInTheDocument();

    const availableNowGroup = screen.getByRole('button', { name: /Available now/ });
    const buyNextGroup = screen.getByRole('button', { name: /Buy next/ });

    expect(availableNowGroup).toHaveAttribute('aria-expanded', 'false');
    expect(buyNextGroup).toHaveAttribute('aria-expanded', 'false');
    expect(screen.queryByRole('button', { name: /Daiquiri/ })).not.toBeInTheDocument();
    expect(screen.queryByText(/White Rum \(2 oz\)/)).not.toBeInTheDocument();
    expect(screen.queryByText('Buy next: Green Chartreuse')).not.toBeInTheDocument();

    await user.click(availableNowGroup);
    const daiquiri = screen.getByRole('button', { name: /Daiquiri/ });
    expect(daiquiri).toHaveAttribute('aria-expanded', 'false');
    expect(screen.getByText('Bright and clean.')).toBeInTheDocument();
    expect(screen.queryByText(/White Rum \(2 oz\)/)).not.toBeInTheDocument();

    await user.click(daiquiri);
    expect(daiquiri).toHaveAttribute('aria-expanded', 'true');
    expect(screen.getByText(/White Rum \(2 oz\)/)).toBeInTheDocument();
    expect(screen.getByText('Matches: lime, rum')).toBeInTheDocument();
    expect(
      screen.getByText('Available now: everything is already in your bar.'),
    ).toBeInTheDocument();

    await user.click(buyNextGroup);
    const lastWord = screen.getByRole('button', { name: /Last Word/ });
    expect(screen.getByText('Sharper and herbal.')).toBeInTheDocument();
    expect(screen.getByText('Missing Green Chartreuse')).toBeInTheDocument();
    expect(screen.queryByText(/Green Chartreuse \(3\/4 oz\)/)).not.toBeInTheDocument();

    await user.click(lastWord);
    expect(screen.getByText('Buy next: Green Chartreuse')).toBeInTheDocument();
    expect(screen.getByText(/Green Chartreuse \(3\/4 oz\)/)).toBeInTheDocument();
    expect(screen.queryByText('Consider for restock')).not.toBeInTheDocument();
    expect(screen.queryByText(/Score/)).not.toBeInTheDocument();
    expect(screen.getByText('Top pick')).toBeInTheDocument();
    expect(screen.getByText('Bright and citrusy')).toBeInTheDocument();
    expect(screen.getByText('Easy to make')).toBeInTheDocument();
  });

  it('renders drink details without recommendation scoring or availability copy', async () => {
    const user = userEvent.setup();

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

    expect(screen.getByRole('region', { name: 'Drink details' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Drink Details/ })).toHaveAttribute(
      'aria-expanded',
      'false',
    );
    expect(screen.getByText('Resolved drink')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Ramos Fizz/ })).not.toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: /Drink Details/ }));
    const ramosFizz = screen.getByRole('button', { name: /Ramos Fizz/ });
    expect(ramosFizz).toBeInTheDocument();
    await user.click(ramosFizz);

    expect(screen.getByText(/Gin \(2 oz\)/)).toBeInTheDocument();
    expect(screen.queryByText(/Score/)).not.toBeInTheDocument();
    expect(screen.queryByText(/Matches:/)).not.toBeInTheDocument();
    expect(
      screen.queryByText('Available now: everything is already in your bar.'),
    ).not.toBeInTheDocument();
  });

  it('omits empty recommendation groups from the summary', () => {
    render(
      <RecommendationTurnList
        session={{
          sessionId: 'session-3',
          title: 'Sparse night',
          createdAtUtc: '2026-04-15T00:00:00Z',
          updatedAtUtc: '2026-04-15T00:10:00Z',
          turns: [
            {
              turnId: 'turn-1',
              sequence: 1,
              role: 'assistant',
              content: 'One option needs a bottle.',
              createdAtUtc: '2026-04-15T00:10:00Z',
              toolInvocations: [],
              feedback: null,
              recommendationGroups: [
                {
                  key: 'make-now',
                  label: 'Available now',
                  items: [],
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
                      recipeEntries: [],
                    },
                  ],
                },
              ],
            },
          ],
        }}
      />,
    );

    expect(screen.queryByRole('button', { name: /Available now/ })).not.toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Buy next/ })).toBeInTheDocument();
    expect(screen.getAllByText('1 drink')).toHaveLength(2);
  });
});
