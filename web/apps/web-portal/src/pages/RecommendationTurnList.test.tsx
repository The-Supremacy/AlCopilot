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
              recommendationGroups: [
                {
                  key: 'make-now',
                  label: 'Make now',
                  items: [
                    {
                      drinkId: 'daiquiri',
                      drinkName: 'Daiquiri',
                      description: 'Bright and clean.',
                      missingIngredientNames: [],
                      matchedSignals: ['lime', 'rum'],
                      score: 96,
                    },
                  ],
                },
                {
                  key: 'buy-next',
                  label: 'Buy next',
                  items: [
                    {
                      drinkId: 'last-word',
                      drinkName: 'Last Word',
                      description: 'Sharper and herbal.',
                      missingIngredientNames: ['Green Chartreuse'],
                      matchedSignals: ['gin'],
                      score: 82,
                    },
                  ],
                },
              ],
            },
          ],
        }}
      />,
    );

    expect(screen.getByLabelText('Make now')).toBeInTheDocument();
    expect(screen.getByLabelText('Buy next')).toBeInTheDocument();
    expect(screen.getByText('Ready now: everything is already in your bar.')).toBeInTheDocument();
    expect(screen.getByText('Buy next: Green Chartreuse')).toBeInTheDocument();
    expect(screen.getByText('Matches: lime, rum')).toBeInTheDocument();
  });
});
