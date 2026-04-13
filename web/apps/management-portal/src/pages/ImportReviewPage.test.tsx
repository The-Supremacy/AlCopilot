import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { ImportReviewPage } from '@/pages/ImportReviewPage';

const reviewImportBatchMutation = {
  mutate: vi.fn(),
  isPending: false,
  error: null as null | Error,
};

let batchId = 'batch-stale';
const importBatchQuery = {
  data: {
    id: 'batch-stale',
    strategyKey: 'iba-cocktails-snapshot',
    status: 'InProgress',
    source: { displayName: 'IBA Snapshot' },
    diagnostics: [{ rowNumber: null, code: 'warn', message: 'Check row 1', severity: 'warning' }],
    reviewConflicts: [
      {
        targetType: 'drink',
        targetKey: 'Negroni',
        action: 'update',
        summary: "Drink 'Negroni' would update metadata, tags, or recipe entries.",
      },
    ],
    reviewRows: [
      {
        targetType: 'drink',
        targetKey: 'Negroni',
        action: 'update',
        changeSummary: "Drink 'Negroni' would update metadata, tags, or recipe entries.",
        hasConflict: true,
        hasError: false,
      },
    ],
    reviewSummary: null as null | { createCount: number; updateCount: number; skipCount: number },
    applySummary: null,
    lastUpdatedAtUtc: '2026-04-12T00:00:00Z',
    reviewedAtUtc: null as null | string,
  },
  isLoading: false,
};

vi.mock('@/features/imports/useImportData', () => ({
  useImportBatch: () => importBatchQuery,
  useReviewImportBatchMutation: () => reviewImportBatchMutation,
}));

vi.mock('@tanstack/react-router', () => ({
  Link: ({ children }: { children: React.ReactNode }) => <span>{children}</span>,
  useParams: () => ({ batchId }),
}));

function renderPage() {
  const queryClient = new QueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      <ImportReviewPage />
    </QueryClientProvider>,
  );
}

beforeEach(() => {
  batchId = 'batch-stale';
  reviewImportBatchMutation.mutate.mockReset();
  reviewImportBatchMutation.isPending = false;
  reviewImportBatchMutation.error = null;
  importBatchQuery.isLoading = false;
  importBatchQuery.data = {
    id: 'batch-stale',
    strategyKey: 'iba-cocktails-snapshot',
    status: 'InProgress',
    source: { displayName: 'IBA Snapshot' },
    diagnostics: [{ rowNumber: null, code: 'warn', message: 'Check row 1', severity: 'warning' }],
    reviewConflicts: [
      {
        targetType: 'drink',
        targetKey: 'Negroni',
        action: 'update',
        summary: "Drink 'Negroni' would update metadata, tags, or recipe entries.",
      },
    ],
    reviewRows: [
      {
        targetType: 'drink',
        targetKey: 'Negroni',
        action: 'update',
        changeSummary: "Drink 'Negroni' would update metadata, tags, or recipe entries.",
        hasConflict: true,
        hasError: false,
      },
    ],
    reviewSummary: null,
    applySummary: null,
    lastUpdatedAtUtc: '2026-04-12T00:00:00Z',
    reviewedAtUtc: null,
  };
});

test('triggers review refresh when review data is stale and batch is in progress', async () => {
  renderPage();

  await waitFor(() => {
    expect(reviewImportBatchMutation.mutate).toHaveBeenCalledWith('batch-stale');
  });
});

test('does not auto-refresh review for completed batches', async () => {
  importBatchQuery.data = {
    ...importBatchQuery.data,
    id: 'batch-complete',
    status: 'Completed',
  };
  batchId = 'batch-complete';

  renderPage();

  await waitFor(() => {
    expect(screen.getByText('Review workspace')).toBeInTheDocument();
  });
  expect(reviewImportBatchMutation.mutate).not.toHaveBeenCalled();
});

test('renders diagnostics and lets the user record a conflict decision', async () => {
  const user = userEvent.setup();
  importBatchQuery.data = {
    ...importBatchQuery.data,
    id: 'batch-current',
    reviewSummary: { createCount: 0, updateCount: 1, skipCount: 0 },
    reviewedAtUtc: '2026-04-12T01:00:00Z',
  };
  batchId = 'batch-current';

  renderPage();

  expect(screen.getByText('Check row 1')).toBeInTheDocument();

  const selects = screen.getAllByRole('combobox');
  await user.selectOptions(selects[3], 'reject-update');
  const reasonInput = screen.getByPlaceholderText('Reason (optional)');
  fireEvent.change(reasonInput, { target: { value: 'Keep the existing recipe' } });

  expect(selects[3]).toHaveValue('reject-update');
  expect(reasonInput).toHaveValue('Keep the existing recipe');
});
