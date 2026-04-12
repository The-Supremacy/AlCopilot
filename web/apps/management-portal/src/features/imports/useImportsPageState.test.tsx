import type { ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { act, renderHook, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import {
  useClearImportDecisionBatch,
  useUpsertImportDecision,
} from '@/features/imports/useImportDecisionStore';
import { useImportsPageState } from '@/features/imports/useImportsPageState';

const startImportMutation = {
  mutateAsync: vi.fn(),
  error: null as null | Error,
};
const applyImportBatchMutation = {
  mutate: vi.fn(),
  error: null as null | Error,
};
const cancelImportBatchMutation = {
  mutate: vi.fn(),
  error: null as null | Error,
};

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
  applyImportBatchMutation.mutate.mockReset();
  applyImportBatchMutation.error = null;
  cancelImportBatchMutation.mutate.mockReset();
  cancelImportBatchMutation.error = null;
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

test('applies using stored decisions and clears them after success', () => {
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

  applyImportBatchMutation.mutate.mockImplementation(
    (_input: unknown, options?: { onSuccess?: () => void }) => {
      options?.onSuccess?.();
    },
  );

  const { result } = renderHook(() => useImportsPageState(), { wrapper });
  act(() => {
    result.current.applyBatch();
  });

  expect(applyImportBatchMutation.mutate).toHaveBeenCalledWith(
    {
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
    },
    expect.any(Object),
  );

  const afterClear = renderHook(() => useImportsPageState(), { wrapper });
  expect(afterClear.result.current.hasStoredDecisionForAllConflicts).toBe(false);
});

test('clears stored decisions after cancel success', () => {
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

  cancelImportBatchMutation.mutate.mockImplementation(
    (_id: string, options?: { onSuccess?: () => void }) => {
      options?.onSuccess?.();
    },
  );

  const { result } = renderHook(() => useImportsPageState(), { wrapper });
  act(() => {
    result.current.cancelBatch();
  });

  expect(cancelImportBatchMutation.mutate).toHaveBeenCalledWith('batch-hook', expect.any(Object));

  const afterClear = renderHook(() => useImportsPageState(), { wrapper });
  expect(afterClear.result.current.hasStoredDecisionForAllConflicts).toBe(false);
});
