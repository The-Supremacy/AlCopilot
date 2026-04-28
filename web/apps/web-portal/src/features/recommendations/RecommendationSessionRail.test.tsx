import type { ReactNode } from 'react';
import { fireEvent, render, screen } from '@testing-library/react';
import type { RecommendationSessionSummaryDto } from '@alcopilot/customer-api-client';
import { RecommendationSessionRail } from '@/features/recommendations/RecommendationSessionRail';

vi.mock('@tanstack/react-router', () => ({
  Link: ({
    children,
    className,
    onClick,
  }: {
    children: ReactNode;
    className?: string;
    onClick?: () => void;
  }) => (
    <a
      href="/chat/session"
      className={className}
      onClick={(event) => {
        event.preventDefault();
        onClick?.();
      }}
    >
      {children}
    </a>
  ),
}));

function buildSessions(count: number): RecommendationSessionSummaryDto[] {
  return Array.from({ length: count }, (_, index) => ({
    sessionId: `session-${index + 1}`,
    title: `Session ${index + 1}`,
    createdAtUtc: '2026-04-15T00:00:00Z',
    updatedAtUtc: '2026-04-15T00:10:00Z',
    lastAssistantMessage: `Assistant reply ${index + 1}`,
  }));
}

describe('RecommendationSessionRail', () => {
  it('limits recent chats by default and can expand the rest', () => {
    render(<RecommendationSessionRail sessions={buildSessions(10)} />);

    expect(screen.getByText('Session 1')).toBeInTheDocument();
    expect(screen.getByText('Session 8')).toBeInTheDocument();
    expect(screen.queryByText('Session 9')).not.toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Show 2 more chats' }));

    expect(screen.getByText('Session 9')).toBeInTheDocument();
    expect(screen.getByText('Session 10')).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Show fewer chats' }));

    expect(screen.queryByText('Session 9')).not.toBeInTheDocument();
  });
});
