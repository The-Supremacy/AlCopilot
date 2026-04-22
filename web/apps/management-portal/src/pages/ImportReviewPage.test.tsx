import { render, screen } from '@testing-library/react';
import { vi } from 'vitest';
import { ImportReviewPage } from '@/pages/ImportReviewPage';

const useImportReviewPageState = vi.fn();

vi.mock('@/features/imports/useImportReviewPageState', () => ({
  useImportReviewPageState: () => useImportReviewPageState(),
}));

vi.mock('@tanstack/react-router', () => ({
  Link: ({ children }: { children: React.ReactNode }) => <span>{children}</span>,
}));

beforeEach(() => {
  useImportReviewPageState.mockReset();
});

test('renders loading state while the review workspace is loading', () => {
  useImportReviewPageState.mockReturnValue({
    kind: 'loading',
  });

  render(<ImportReviewPage />);

  expect(screen.getByText('Loading review workspace...')).toBeInTheDocument();
});

test('renders not-found state when the batch is missing', () => {
  useImportReviewPageState.mockReturnValue({
    kind: 'not-found',
  });

  render(<ImportReviewPage />);

  expect(screen.getByText('Import batch not found.')).toBeInTheDocument();
  expect(screen.getByText('Back to imports')).toBeInTheDocument();
});

test('renders the review workspace when review data is available', () => {
  useImportReviewPageState.mockReturnValue({
    kind: 'ready',
    batch: {
      id: 'batch-ready',
      strategyKey: 'iba-cocktails-snapshot',
      status: 'InProgress',
      requiresReview: true,
      applyReadiness: 'RequiresReview',
      source: {
        sourceReference: null,
        displayName: 'IBA Snapshot',
        contentType: null,
        metadata: {},
      },
      diagnostics: [{ rowNumber: null, code: 'warn', message: 'Check row 1', severity: 'warning' }],
      reviewRows: [
        {
          targetType: 'drink',
          targetKey: 'Negroni',
          action: 'update',
          changeSummary: "Drink 'Negroni' would update metadata.",
          requiresReview: true,
          hasError: false,
        },
      ],
      reviewSummary: null,
      applySummary: null,
      createdAtUtc: '2026-04-12T00:00:00Z',
      validatedAtUtc: null,
      reviewedAtUtc: null,
      appliedAtUtc: null,
      lastUpdatedAtUtc: '2026-04-12T00:00:00Z',
    },
    rows: [
      {
        targetType: 'drink',
        targetKey: 'Negroni',
        action: 'update',
        changeSummary: "Drink 'Negroni' would update metadata.",
        requiresReview: true,
        hasError: false,
      },
    ],
    aggregateFilter: 'all',
    actionFilter: 'all',
    reviewStateFilter: 'all',
    setAggregateFilter: vi.fn(),
    setActionFilter: vi.fn(),
    setReviewStateFilter: vi.fn(),
    reviewIsStale: true,
    canApply: false,
    canCancel: true,
    isMutating: false,
    isApplying: false,
    isCancelling: false,
    reviewErrorMessage: null,
    refreshReview: vi.fn(),
    applyBatch: vi.fn(),
    cancelBatch: vi.fn(),
  });

  render(<ImportReviewPage />);

  expect(screen.getByRole('heading', { name: 'Review workspace' })).toBeInTheDocument();
  expect(screen.getByText('Check row 1')).toBeInTheDocument();
  expect(screen.getByRole('button', { name: 'Generate review' })).toBeInTheDocument();
});
