import { act } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import { toast } from 'sonner';
import { ImportReviewPage } from '@/pages/ImportReviewPage';

const reviewImportBatchMutation = {
  mutate: vi.fn(),
  isPending: false,
  error: null as null | Error,
};
const applyImportBatchMutation = {
  mutateAsync: vi.fn(),
  isPending: false,
  error: null as null | Error,
};
const cancelImportBatchMutation = {
  mutateAsync: vi.fn(),
  isPending: false,
  error: null as null | Error,
};

let batchId = 'batch-stale';
const importBatchQuery = {
  data: {
    id: 'batch-stale',
    strategyKey: 'iba-cocktails-snapshot',
    status: 'InProgress',
    requiresReview: true,
    applyReadiness: 'RequiresReview',
    source: { displayName: 'IBA Snapshot' },
    diagnostics: [{ rowNumber: null, code: 'warn', message: 'Check row 1', severity: 'warning' }],
    reviewRows: [
      {
        targetType: 'drink',
        targetKey: 'Negroni',
        action: 'update',
        changeSummary: "Drink 'Negroni' would update metadata, tags, or recipe entries.",
        requiresReview: true,
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

vi.mock('sonner', () => ({
  toast: {
    promise: vi.fn((promise: Promise<unknown>) => promise),
  },
}));

vi.mock('@/features/imports/useImportData', () => ({
  useImportBatch: () => importBatchQuery,
  useReviewImportBatchMutation: () => reviewImportBatchMutation,
  useApplyImportBatchMutation: () => applyImportBatchMutation,
  useCancelImportBatchMutation: () => cancelImportBatchMutation,
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
  applyImportBatchMutation.mutateAsync.mockReset();
  applyImportBatchMutation.isPending = false;
  applyImportBatchMutation.error = null;
  cancelImportBatchMutation.mutateAsync.mockReset();
  cancelImportBatchMutation.isPending = false;
  cancelImportBatchMutation.error = null;
  vi.mocked(toast.promise).mockClear();
  importBatchQuery.isLoading = false;
  importBatchQuery.data = {
    id: 'batch-stale',
    strategyKey: 'iba-cocktails-snapshot',
    status: 'InProgress',
    requiresReview: true,
    applyReadiness: 'RequiresReview',
    source: { displayName: 'IBA Snapshot' },
    diagnostics: [{ rowNumber: null, code: 'warn', message: 'Check row 1', severity: 'warning' }],
    reviewRows: [
      {
        targetType: 'drink',
        targetKey: 'Negroni',
        action: 'update',
        changeSummary: "Drink 'Negroni' would update metadata, tags, or recipe entries.",
        requiresReview: true,
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

test('renders diagnostics and inspection-first review state', async () => {
  importBatchQuery.data = {
    ...importBatchQuery.data,
    id: 'batch-current',
    reviewSummary: { createCount: 0, updateCount: 1, skipCount: 0 },
    reviewedAtUtc: '2026-04-12T01:00:00Z',
  };
  batchId = 'batch-current';

  renderPage();

  expect(screen.getByText('Check row 1')).toBeInTheDocument();
  expect(screen.getByText('Review before apply')).toBeInTheDocument();
  expect(screen.queryByText('Approve update')).not.toBeInTheDocument();
  expect(screen.queryByPlaceholderText('Reason (optional)')).not.toBeInTheDocument();
});

test('applies from the review page when the batch is ready', async () => {
  importBatchQuery.data = {
    ...importBatchQuery.data,
    id: 'batch-ready',
    applyReadiness: 'Ready',
    requiresReview: false,
    reviewSummary: { createCount: 1, updateCount: 0, skipCount: 0 },
    reviewedAtUtc: '2026-04-12T01:00:00Z',
  };
  batchId = 'batch-ready';
  applyImportBatchMutation.mutateAsync.mockResolvedValue({
    batch: { ...importBatchQuery.data, status: 'Completed', applyReadiness: 'Completed' },
    applyReadiness: 'Completed',
    wasApplied: true,
  });

  renderPage();

  await act(async () => {
    screen.getByRole('button', { name: 'Apply' }).click();
  });

  expect(applyImportBatchMutation.mutateAsync).toHaveBeenCalledWith({ id: 'batch-ready' });
  expect(toast.promise).toHaveBeenCalled();
});

test('cancels from the review page while batch is in progress', async () => {
  importBatchQuery.data = {
    ...importBatchQuery.data,
    id: 'batch-cancel',
    reviewSummary: { createCount: 0, updateCount: 1, skipCount: 0 },
    reviewedAtUtc: '2026-04-12T01:00:00Z',
  };
  batchId = 'batch-cancel';
  cancelImportBatchMutation.mutateAsync.mockResolvedValue({
    ...importBatchQuery.data,
    status: 'Cancelled',
    applyReadiness: 'Cancelled',
  });

  renderPage();

  await act(async () => {
    screen.getByRole('button', { name: 'Cancel' }).click();
  });

  expect(cancelImportBatchMutation.mutateAsync).toHaveBeenCalledWith('batch-cancel');
  expect(toast.promise).toHaveBeenCalled();
});
