import type { ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { act, renderHook, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import { toast } from 'sonner';
import { useImportsPageState } from '@/features/imports/useImportsPageState';

const startImportMutation = {
  mutateAsync: vi.fn(),
  error: null as null | Error,
  isPending: false,
};
const applyImportBatchMutation = {
  mutateAsync: vi.fn(),
  error: null as null | Error,
  isPending: false,
};
const cancelImportBatchMutation = {
  mutateAsync: vi.fn(),
  error: null as null | Error,
  isPending: false,
};

vi.mock('sonner', () => ({
  toast: {
    promise: vi.fn((promise: Promise<unknown>) => promise),
  },
}));

const currentBatch = {
  id: 'batch-hook',
  status: 'InProgress',
  requiresReview: true,
  applyReadiness: 'RequiresReview',
  reviewedAtUtc: null as null | string,
  diagnostics: [] as Array<{ severity: string }>,
};

const importHistoryData = [{ id: 'batch-hook', status: 'InProgress' }];

vi.mock('@/features/imports/useImportData', () => ({
  useImportHistory: () => ({
    data: importHistoryData,
  }),
  useImportBatch: (id: string | null) => ({
    data: id ? currentBatch : null,
  }),
  useStartImportMutation: () => startImportMutation,
  useApplyImportBatchMutation: () => applyImportBatchMutation,
  useCancelImportBatchMutation: () => cancelImportBatchMutation,
}));

function createWrapper() {
  const queryClient = new QueryClient();

  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
  };
}

beforeEach(() => {
  startImportMutation.mutateAsync.mockReset();
  startImportMutation.error = null;
  applyImportBatchMutation.mutateAsync.mockReset();
  applyImportBatchMutation.error = null;
  cancelImportBatchMutation.mutateAsync.mockReset();
  cancelImportBatchMutation.error = null;
  vi.mocked(toast.promise).mockClear();
  currentBatch.status = 'InProgress';
  currentBatch.requiresReview = true;
  currentBatch.applyReadiness = 'RequiresReview';
  currentBatch.reviewedAtUtc = null;
  currentBatch.diagnostics = [];
  importHistoryData.splice(0, importHistoryData.length, { id: 'batch-hook', status: 'InProgress' });
});

test('reports review requirement until the batch has been explicitly reviewed', async () => {
  const wrapper = createWrapper();
  const { result, rerender } = renderHook(() => useImportsPageState(), { wrapper });

  expect(result.current.requiresReviewBeforeApply).toBe(true);

  act(() => {
    currentBatch.applyReadiness = 'Ready';
    rerender();
  });

  await waitFor(() => {
    expect(result.current.requiresReviewBeforeApply).toBe(false);
  });
});

test('applies without row-level decision input', async () => {
  const wrapper = createWrapper();
  currentBatch.applyReadiness = 'Ready';

  applyImportBatchMutation.mutateAsync.mockResolvedValue({
    batch: { ...currentBatch, applyReadiness: 'Completed', status: 'Completed' },
    applyReadiness: 'Completed',
    wasApplied: true,
  });

  const { result } = renderHook(() => useImportsPageState(), { wrapper });
  await act(async () => {
    await result.current.applyBatch();
  });

  expect(applyImportBatchMutation.mutateAsync).toHaveBeenCalledWith({
    id: 'batch-hook',
  });

  expect(toast.promise).toHaveBeenCalled();
});

test('cancels without clearing local decision state because none is kept', async () => {
  const wrapper = createWrapper();
  cancelImportBatchMutation.mutateAsync.mockResolvedValue({ id: 'batch-hook' });

  const { result } = renderHook(() => useImportsPageState(), { wrapper });
  await act(async () => {
    await result.current.cancelBatch();
  });

  expect(cancelImportBatchMutation.mutateAsync).toHaveBeenCalledWith('batch-hook');
  expect(toast.promise).toHaveBeenCalled();
});

test('does not resurrect older in-progress batches when the newest batch is already terminal', () => {
  const wrapper = createWrapper();

  importHistoryData.splice(
    0,
    importHistoryData.length,
    { id: 'batch-new', status: 'Cancelled' },
    { id: 'batch-old', status: 'InProgress' },
  );

  const { result } = renderHook(() => useImportsPageState(), { wrapper });

  expect(result.current.currentBatch).toBeNull();
});
