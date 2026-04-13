import type { ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { act, renderHook, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import { toast } from 'sonner';
import {
  getStoredBatchDecisions,
  useClearImportDecisionBatch,
  useUpsertImportDecision,
} from '@/features/imports/useImportDecisionStore';
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
  diagnostics: [] as Array<{ severity: string }>,
  reviewConflicts: [{ targetType: 'drink', targetKey: 'Negroni' }],
};

vi.mock('@/lib/usePortalData', () => ({
  useImportHistory: () => ({
    data: [{ id: 'batch-hook', status: 'InProgress' }],
  }),
  useImportBatch: () => ({
    data: currentBatch,
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
  currentBatch.diagnostics = [];
  currentBatch.reviewConflicts = [{ targetType: 'drink', targetKey: 'Negroni' }];

  const wrapper = createWrapper();
  const { result } = renderHook(() => useClearImportDecisionBatch(), { wrapper });
  result.current('batch-hook');
});

test('reports unresolved conflicts until explicit decisions are stored', () => {
  const wrapper = createWrapper();
  const { result } = renderHook(() => useImportsPageState(), { wrapper });

  expect(result.current.hasStoredDecisionForAllConflicts).toBe(false);

  const decisionHook = renderHook(() => useUpsertImportDecision(), { wrapper });
  act(() => {
    decisionHook.result.current('batch-hook', {
      targetType: 'drink',
      targetKey: 'Negroni',
      decision: 'approve-update',
      reason: 'Accept the import update',
    });
  });
  return waitFor(() => {
    expect(result.current.hasStoredDecisionForAllConflicts).toBe(true);
  });
});

test('applies using stored decisions and clears them after success', async () => {
  const wrapper = createWrapper();
  const decisionHook = renderHook(() => useUpsertImportDecision(), { wrapper });
  act(() => {
    decisionHook.result.current('batch-hook', {
      targetType: 'drink',
      targetKey: 'Negroni',
      decision: 'reject-update',
      reason: 'Keep the current version',
    });
  });

  applyImportBatchMutation.mutateAsync.mockResolvedValue({ id: 'batch-hook' });

  const { result } = renderHook(() => useImportsPageState(), { wrapper });
  await act(async () => {
    await result.current.applyBatch();
  });

  expect(applyImportBatchMutation.mutateAsync).toHaveBeenCalledWith({
    id: 'batch-hook',
    input: {
      overrideDuplicateFingerprint: false,
      decisions: [
        {
          targetType: 'drink',
          targetKey: 'Negroni',
          decision: 'reject-update',
          reason: 'Keep the current version',
        },
      ],
    },
  });

  expect(toast.promise).toHaveBeenCalled();
  expect(getStoredBatchDecisions('batch-hook')).toEqual([]);
});

test('clears stored decisions after cancel success', async () => {
  const wrapper = createWrapper();
  const decisionHook = renderHook(() => useUpsertImportDecision(), { wrapper });
  act(() => {
    decisionHook.result.current('batch-hook', {
      targetType: 'drink',
      targetKey: 'Negroni',
      decision: 'approve-update',
      reason: 'Ship the update',
    });
  });

  cancelImportBatchMutation.mutateAsync.mockResolvedValue({ id: 'batch-hook' });

  const { result } = renderHook(() => useImportsPageState(), { wrapper });
  await act(async () => {
    await result.current.cancelBatch();
  });

  expect(cancelImportBatchMutation.mutateAsync).toHaveBeenCalledWith('batch-hook');
  expect(toast.promise).toHaveBeenCalled();
  expect(getStoredBatchDecisions('batch-hook')).toEqual([]);
});
