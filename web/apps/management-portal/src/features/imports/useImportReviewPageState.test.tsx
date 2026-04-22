import type { ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { act, renderHook, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import { toast } from 'sonner';
import { useImportReviewPageState } from '@/features/imports/useImportReviewPageState';

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
    source: { sourceReference: null, displayName: 'IBA Snapshot', contentType: null, metadata: {} },
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
    createdAtUtc: '2026-04-12T00:00:00Z',
    validatedAtUtc: null as null | string,
    reviewedAtUtc: null as null | string,
    appliedAtUtc: null as null | string,
    lastUpdatedAtUtc: '2026-04-12T00:00:00Z',
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
  useParams: () => ({ batchId }),
}));

function createWrapper() {
  const queryClient = new QueryClient();

  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
  };
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
    source: { sourceReference: null, displayName: 'IBA Snapshot', contentType: null, metadata: {} },
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
    createdAtUtc: '2026-04-12T00:00:00Z',
    validatedAtUtc: null,
    reviewedAtUtc: null,
    appliedAtUtc: null,
    lastUpdatedAtUtc: '2026-04-12T00:00:00Z',
  };
});

test('returns a loading state while the batch is still loading', () => {
  const wrapper = createWrapper();
  importBatchQuery.isLoading = true;
  importBatchQuery.data = undefined as unknown as typeof importBatchQuery.data;

  const { result } = renderHook(() => useImportReviewPageState(), { wrapper });

  expect(result.current.kind).toBe('loading');
});

test('triggers review refresh when review data is stale and batch is in progress', async () => {
  const wrapper = createWrapper();

  renderHook(() => useImportReviewPageState(), { wrapper });

  await waitFor(() => {
    expect(reviewImportBatchMutation.mutate).toHaveBeenCalledWith('batch-stale');
  });
});

test('does not auto-refresh review for completed batches', async () => {
  const wrapper = createWrapper();
  importBatchQuery.data = {
    ...importBatchQuery.data,
    id: 'batch-complete',
    status: 'Completed',
  };
  batchId = 'batch-complete';

  const { result } = renderHook(() => useImportReviewPageState(), { wrapper });

  await waitFor(() => {
    expect(result.current.kind).toBe('ready');
  });
  expect(reviewImportBatchMutation.mutate).not.toHaveBeenCalled();
});

test('applies from the review page when the batch is ready', async () => {
  const wrapper = createWrapper();
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

  const { result } = renderHook(() => useImportReviewPageState(), { wrapper });

  await act(async () => {
    if (result.current.kind !== 'ready') {
      throw new Error('Expected ready state');
    }

    await result.current.applyBatch();
  });

  expect(applyImportBatchMutation.mutateAsync).toHaveBeenCalledWith({ id: 'batch-ready' });
  expect(toast.promise).toHaveBeenCalled();
});

test('cancels from the review page while batch is in progress', async () => {
  const wrapper = createWrapper();
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

  const { result } = renderHook(() => useImportReviewPageState(), { wrapper });

  await act(async () => {
    if (result.current.kind !== 'ready') {
      throw new Error('Expected ready state');
    }

    await result.current.cancelBatch();
  });

  expect(cancelImportBatchMutation.mutateAsync).toHaveBeenCalledWith('batch-cancel');
  expect(toast.promise).toHaveBeenCalled();
});

test('reports not-found when the batch is missing after loading completes', () => {
  const wrapper = createWrapper();
  importBatchQuery.data = undefined as unknown as typeof importBatchQuery.data;
  importBatchQuery.isLoading = false;

  const { result } = renderHook(() => useImportReviewPageState(), { wrapper });

  expect(result.current.kind).toBe('not-found');
});
